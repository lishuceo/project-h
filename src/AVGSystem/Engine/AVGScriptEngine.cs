using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCore;
using GameEntry.AVGSystem.Data;

namespace GameEntry.AVGSystem.Engine;

/// <summary>
/// AVG剧本执行引擎 - 数据驱动的剧情系统
/// </summary>
public static class AVGScriptEngine
{
    // 已加载的剧本
    private static Dictionary<string, AVGScript> loadedScripts = new();
    
    // 当前执行状态
    private static AVGScript? currentScript;
    private static string? currentNodeId;
    private static bool isExecuting = false;

    /// <summary>
    /// 注册剧本数据
    /// </summary>
    public static void RegisterScript(AVGScript script)
    {
        if (string.IsNullOrEmpty(script.Id))
        {
            Game.Logger.LogWarning("⚠️ 剧本ID为空，跳过注册");
            return;
        }
        
        loadedScripts[script.Id] = script;
        Game.Logger.LogInformation("📚 注册剧本: {ScriptId} - {ScriptName}", script.Id, script.Name);
    }

    /// <summary>
    /// 执行剧本
    /// </summary>
    public static async Task PlayScript(string scriptId)
    {
        try
        {
            if (isExecuting)
            {
                Game.Logger.LogWarning("⚠️ 已有剧本在执行中，跳过新剧本");
                return;
            }

            if (!loadedScripts.TryGetValue(scriptId, out var script))
            {
                Game.Logger.LogError("❌ 未找到剧本: {ScriptId}", scriptId);
                return;
            }

            Game.Logger.LogInformation("🎬 开始执行剧本: {ScriptId} - {ScriptName}", script.Id, script.Name);
            
            isExecuting = true;
            currentScript = script;
            currentNodeId = script.StartNodeId;

            // 执行剧本流程
            await ExecuteScript();
            
            Game.Logger.LogInformation("✅ 剧本执行完成: {ScriptId}", scriptId);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 剧本执行失败: {ScriptId}", scriptId);
        }
        finally
        {
            isExecuting = false;
            currentScript = null;
            currentNodeId = null;
        }
    }

    /// <summary>
    /// 执行剧本流程
    /// </summary>
    private static async Task ExecuteScript()
    {
        while (!string.IsNullOrEmpty(currentNodeId) && currentScript != null)
        {
            var node = FindNode(currentScript, currentNodeId);
            if (node == null)
            {
                Game.Logger.LogError("❌ 未找到节点: {NodeId}", currentNodeId);
                break;
            }

            // 根据节点类型执行不同逻辑
            switch (node.Type)
            {
                case AVGNodeType.Dialog:
                    await ExecuteDialogNode((DialogNode)node);
                    break;
                case AVGNodeType.Choice:
                    await ExecuteChoiceNode((ChoiceNode)node);
                    return; // 选择节点会设置下一个节点，不需要自动继续
                case AVGNodeType.Background:
                    await ExecuteBackgroundNode((BackgroundNode)node);
                    break;
                case AVGNodeType.End:
                    return; // 结束执行
            }

            // 自动跳转到下一个节点
            currentNodeId = node.NextNodeId;
        }
    }

    /// <summary>
    /// 执行对话节点
    /// </summary>
    private static async Task ExecuteDialogNode(DialogNode node)
    {
#if CLIENT
        try
        {
            // 设置背景（如果指定）
            if (!string.IsNullOrEmpty(node.Background))
            {
                AVGFullScreen.SetBackground(node.Background);
            }

            // 显示角色（如果指定）
            foreach (var character in node.Characters)
            {
                if (character.Visible)
                {
                    AVGFullScreen.ShowCharacter(character.Name, character.ImagePath, (GameEntry.AVGSystem.CharacterPosition)character.Position);
                }
            }

            // 显示对话
            await AVGFullScreen.ShowDialog(node.Speaker, node.Content);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 对话节点执行失败: {NodeId}", node.Id);
        }
#endif
    }

    /// <summary>
    /// 执行选择节点
    /// </summary>
    private static async Task ExecuteChoiceNode(ChoiceNode node)
    {
#if CLIENT
        try
        {
            // 准备选择选项文本
            var optionTexts = new string[node.Options.Count];
            for (int i = 0; i < node.Options.Count; i++)
            {
                optionTexts[i] = node.Options[i].Text;
            }

            // 显示选择
            var selectedIndex = await AVGFullScreen.ShowChoice(node.Title, optionTexts);
            
            // 根据选择跳转到对应节点
            if (selectedIndex >= 0 && selectedIndex < node.Options.Count)
            {
                currentNodeId = node.Options[selectedIndex].NextNodeId;
                
                // 继续执行下一个节点
                await ExecuteScript();
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 选择节点执行失败: {NodeId}", node.Id);
        }
#endif
    }

    /// <summary>
    /// 执行背景节点
    /// </summary>
    private static async Task ExecuteBackgroundNode(BackgroundNode node)
    {
#if CLIENT
        try
        {
            AVGFullScreen.SetBackground(node.ImagePath);
            await Task.Delay(100); // 短暂延迟确保背景设置完成
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 背景节点执行失败: {NodeId}", node.Id);
        }
#endif
    }

    /// <summary>
    /// 查找节点
    /// </summary>
    private static AVGNode? FindNode(AVGScript script, string nodeId)
    {
        return script.Nodes.Find(n => n.Id == nodeId);
    }

    /// <summary>
    /// 获取已注册的剧本列表
    /// </summary>
    public static IReadOnlyDictionary<string, AVGScript> GetRegisteredScripts()
    {
        return loadedScripts;
    }

    /// <summary>
    /// 检查是否有剧本在执行
    /// </summary>
    public static bool IsExecuting => isExecuting;

    /// <summary>
    /// 清理引擎资源
    /// </summary>
    public static void Cleanup()
    {
        loadedScripts.Clear();
        currentScript = null;
        currentNodeId = null;
        isExecuting = false;
        Game.Logger.LogInformation("🧹 AVG剧本引擎清理完成");
    }
}
