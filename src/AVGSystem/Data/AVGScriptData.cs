using System;
using System.Collections.Generic;

namespace GameEntry.AVGSystem.Data;

/// <summary>
/// AVG剧本数据结构 - 数据和逻辑完全分离
/// </summary>
public class AVGScript
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<AVGNode> Nodes { get; set; } = new();
    public string StartNodeId { get; set; } = "";
}

/// <summary>
/// AVG节点基类
/// </summary>
public abstract class AVGNode
{
    public string Id { get; set; } = "";
    public AVGNodeType Type { get; set; }
    public string? NextNodeId { get; set; } // 下一个节点ID，null表示结束
}

/// <summary>
/// 对话节点
/// </summary>
public class DialogNode : AVGNode
{
    public string Speaker { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Background { get; set; } // 背景图片路径
    public List<CharacterDisplay> Characters { get; set; } = new(); // 角色立绘
    
    public DialogNode()
    {
        Type = AVGNodeType.Dialog;
    }
}

/// <summary>
/// 选择节点
/// </summary>
public class ChoiceNode : AVGNode
{
    public string Title { get; set; } = "";
    public List<ChoiceOption> Options { get; set; } = new();
    
    public ChoiceNode()
    {
        Type = AVGNodeType.Choice;
    }
}

/// <summary>
/// 选择选项
/// </summary>
public class ChoiceOption
{
    public string Text { get; set; } = "";
    public string NextNodeId { get; set; } = "";
    public Dictionary<string, object> Conditions { get; set; } = new(); // 选择条件（预留）
}

/// <summary>
/// 角色显示信息
/// </summary>
public class CharacterDisplay
{
    public string Name { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public CharacterPosition Position { get; set; } = CharacterPosition.Center;
    public bool Visible { get; set; } = true;
}

/// <summary>
/// 背景设置节点
/// </summary>
public class BackgroundNode : AVGNode
{
    public string ImagePath { get; set; } = "";
    
    public BackgroundNode()
    {
        Type = AVGNodeType.Background;
    }
}

/// <summary>
/// AVG节点类型
/// </summary>
public enum AVGNodeType
{
    Dialog,     // 对话
    Choice,     // 选择
    Background, // 背景设置
    Character,  // 角色设置
    End         // 结束
}

/// <summary>
/// 角色立绘位置枚举 - 服务端客户端共享
/// </summary>
public enum CharacterPosition
{
    Left,
    Center,
    Right
}
