using GameCore.AbilitySystem;
using GameCore.Execution;
using GameCore.Execution.Data;
using GameCore.BaseType;
using GameCore.EntitySystem;
using GameCore.EntitySystem.Data;
using GameCore.PlayerAndUsers;
using GameCore.SceneSystem;
using GameData;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace GameEntry.BuildingSystem;

/// <summary>
/// 建造效果 - 在指定位置创建建筑单位
/// 基于 spell_assist_control.lua 的建造逻辑
/// </summary>
[GameDataNodeType<GameDataEffect, GameDataEffect>]
public partial class GameDataBuildingEffect
{
    /// <summary>
    /// 要建造的单位类型
    /// </summary>
    public IGameLink<GameDataUnit>? BuildingUnit { get; set; }

    /// <summary>
    /// 建造成功率 (1.0 = 100%)
    /// </summary>
    public float SuccessRate { get; set; } = 1.0f;

    /// <summary>
    /// 是否检查碰撞
    /// </summary>
    public bool CheckCollision { get; set; } = true;

    /// <summary>
    /// 建造偏移量
    /// </summary>
    public Vector3 Offset { get; set; } = Vector3.Zero;

    public override void Execute(Effect context)
    {
        var logger = Game.Logger;
        
        try
        {
            logger.LogInformation("🏗️ 执行建造效果");

            // 获取施法者和目标位置
            var caster = context.Caster;
            
            if (caster == null)
            {
                logger.LogWarning("⚠️ 建造效果: 找不到施法者");
                return;
            }

            if (!(context.Target is ScenePoint targetPoint))
            {
                logger.LogWarning("⚠️ 建造效果: 目标不是有效的场景位置: {TargetPoint}", context.Target);
                return;
            }

            if (BuildingUnit?.Data == null)
            {
                logger.LogWarning("⚠️ 建造效果: 建筑单位配置无效");
                return;
            }

            logger.LogInformation("📥 接收到的目标位置: ({X}, {Y}, {Z})", 
                                 targetPoint.X, targetPoint.Y, targetPoint.Z);
            logger.LogInformation("📐 配置的偏移量: ({X}, {Y}, {Z})", 
                                 Offset.X, Offset.Y, Offset.Z);

            // 🏗️ 直接使用目标位置，不添加偏移量（精确建造在鼠标位置）
            var buildPosition = new ScenePoint(
                targetPoint.X,  // 不添加偏移量
                targetPoint.Y,  // 不添加偏移量  
                targetPoint.Scene)
            {
                Z = targetPoint.Z
            };

            logger.LogInformation("🎯 最终建造位置: ({X}, {Y}, {Z})", 
                                 buildPosition.X, buildPosition.Y, buildPosition.Z);

            // 检查建造成功率
            var successRate = SuccessRate;
            if (successRate < 1.0f)
            {
                var random = Random.Shared.NextSingle();
                if (random > successRate)
                {
                    logger.LogInformation("🎲 建造失败 - 成功率检查未通过 ({Rate:P})", successRate);
                    return;
                }
            }

            // 检查碰撞 (简化版)
            if (CheckCollision && HasCollisionAtPosition(buildPosition))
            {
                logger.LogInformation("🚫 建造失败 - 位置有碰撞");
                return;
            }

            // 创建建筑单位
            var building = CreateBuildingUnit(caster, buildPosition);
            if (building != null)
            {
                logger.LogInformation("✅ 建筑创建成功: {Building} 在位置 ({X}, {Y}, {Z})", 
                                     building.Cache.Name, buildPosition.X, buildPosition.Y, buildPosition.Z);

                // 触发建筑建造完成事件
                TriggerBuildingCompletedEvent(caster, building, buildPosition);
            }
            else
            {
                logger.LogWarning("❌ 建筑创建失败");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ 建造效果执行失败");
        }
    }

    /// <summary>
    /// 检查位置是否有碰撞
    /// </summary>
    private bool HasCollisionAtPosition(ScenePoint position)
    {
        // 这里应该实现实际的碰撞检测逻辑
        // 包括静态地形碰撞、动态单位碰撞等
        // 简化处理，暂时返回false
        
        var logger = Game.Logger;
        logger.LogDebug("🔍 检查位置碰撞: ({X}, {Y}, {Z})", position.X, position.Y, position.Z);
        
        // TODO: 实现真正的碰撞检测
        // 1. 检查静态地形碰撞
        // 2. 检查动态单位碰撞  
        // 3. 检查建筑足迹重叠
        
        return false;
    }

    /// <summary>
    /// 创建建筑单位 - 参考怪物创建方式
    /// </summary>
    private Unit? CreateBuildingUnit(Entity caster, ScenePoint position)
    {
        try
        {
            var logger = Game.Logger;
            
            if (BuildingUnit?.Data == null)
            {
                logger.LogError("❌ 建筑单位数编表为空");
                return null;
            }

            // 获取施法者的玩家 (建筑归属于施法者的玩家)
            var casterPlayer = GetPlayerFromCaster(caster);
            if (casterPlayer == null)
            {
                logger.LogError("❌ 无法确定施法者的玩家归属");
                return null;
            }

            // 参考怪物创建方式：使用数编表的CreateUnit方法
            var building = BuildingUnit.Data.CreateUnit(
                casterPlayer,
                position,
                0  // 朝向角度，0度表示默认朝向
            );

            if (building != null)
            {
                logger.LogInformation("🏗️ 建筑单位创建成功: {Building} 在位置 ({X}, {Y}) 归属玩家 {Player}", 
                                     building.Cache.Name, position.X, position.Y, casterPlayer.Id);
                
                // 建筑特殊设置
                SetupBuildingProperties(building);
                
                return building;
            }
            else
            {
                logger.LogError("❌ 建筑单位创建失败");
                return null;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 创建建筑单位异常");
            return null;
        }
    }

    /// <summary>
    /// 从施法者获取玩家
    /// </summary>
    private Player? GetPlayerFromCaster(Entity caster)
    {
        // 尝试从单位获取玩家归属
        if (caster is Unit unit)
        {
            // 单位可能有Owner属性指向玩家
            // 这里简化处理，使用玩家1作为默认建筑归属
            return Player.GetById(1);
        }
        
        // 默认返回玩家1
        return Player.GetById(1);
    }

    /// <summary>
    /// 设置建筑属性
    /// </summary>
    private void SetupBuildingProperties(Unit building)
    {
        try
        {
            // 建筑特殊设置
            // 例如：禁用移动、设置为建筑类型等
            
            // 🔧 为塔单位添加UnitLeveling组件
            if (IsTowerUnit(building))
            {
                try
                {
#if SERVER
                    // 尝试为塔添加等级系统组件  
                    var unitLeveling = GameCore.Leveling.UnitLevelingExtension.GetComponent<GameCore.Leveling.UnitLeveling>(building);
                    if (unitLeveling == null)
                    {
                        // 如果没有等级组件，需要通过GameData配置添加
                        // 这里先记录日志，实际需要在GameData中配置UnitLeveling
                        Game.Logger.LogInformation("🏗️ 塔 {Building} 需要在GameData中配置UnitLeveling组件", building.Cache.Name);
                    }
                    else
                    {
                        // 设置初始等级为1
                        unitLeveling.ForceSetLevel(1);
                        Game.Logger.LogInformation("✅ 塔 {Building} UnitLeveling组件已初始化为1级", building.Cache.Name);
                    }
#endif
                    
                }
                catch (Exception ex)
                {
                    Game.Logger.LogWarning(ex, "⚠️ 初始化塔等级系统失败: {Building}", building.Cache.Name);
                }
            }
            
            Game.Logger.LogInformation("🔧 建筑 {Building} 属性设置完成", building.Cache.Name);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 设置建筑属性失败");
        }
    }

    /// <summary>
    /// 触发建筑建造完成事件
    /// </summary>
    private void TriggerBuildingCompletedEvent(Entity caster, Unit building, ScenePoint position)
    {
        try
        {
            // 这里可以触发建筑建造完成的事件
            // 例如播放建造音效、显示建造特效等
            
            var logger = Game.Logger;
            logger.LogInformation("🎉 建筑建造完成事件触发: {Building} 在位置 ({X}, {Y})", 
                                 building.Cache.Name, position.X, position.Y);

            // 将建筑注册到塔防系统（仅服务端）
#if SERVER
            // 调用塔防服务端注册建筑
            try
            {
                GameEntry.TowerDefenseGame.TowerDefenseServer.RegisterBuilding(building);
                
                // 🤖 AI系统已暂时禁用，塔将使用被动技能自动攻击
                // StartTowerAI(building);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ 无法注册建筑到塔防系统");
            }
#endif
            
            // TODO: 可以在这里添加：
            // 1. 播放建造完成音效
            // 2. 显示建造完成特效
            // 3. 发送建造完成消息给客户端
            // 4. 更新玩家资源
            // 5. 触发任务/成就检查
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 触发建筑完成事件失败");
        }
    }

    /// <summary>
    /// 检查是否是塔单位
    /// </summary>
    private bool IsTowerUnit(Unit building)
    {
        if (building?.Cache?.Name == null) return false;
        
        var unitName = building.Cache.Name;
        return unitName.Contains("塔") || 
               unitName.Contains("Tower") ||
               unitName == "单体减速塔" ||
               unitName == "光环减速塔" ||
               unitName == "群体伤害塔" ||
               unitName == "向量穿透塔";
    }

#if SERVER
    /// <summary>
    /// 🤖 为塔启动AI系统
    /// </summary>
    private void StartTowerAI(Unit building)
    {
        try
        {
            var logger = Game.Logger;
            
            // 检查是否是塔单位
            if (!IsTowerUnit(building))
            {
                logger.LogDebug("🔍 单位 {Building} 不是塔类型，跳过AI启动", building.Cache.Name);
                return;
            }
            
            // 尝试为塔添加AI
            var aiThinkTree = GameCore.AISystem.AIThinkTree.AddDefaultAI(building);
            if (aiThinkTree != null)
            {
                logger.LogInformation("🤖 成功为塔 {Tower} 启动AI系统", building.Cache.Name);
                
                // 获取AI攻击范围
                var aiAttackRange = aiThinkTree.Attack?.Range ?? 0;
                
                logger.LogInformation("🎯 AI配置 - 扫描范围: {ScanRange}", aiThinkTree.ScanRange);
                logger.LogInformation("🎯 AI攻击范围: {AIAttackRange} (如果为0说明塔使用特殊技能而非基础攻击)", aiAttackRange);
                
                // 简化日志，避免复杂的组件访问
                logger.LogInformation("🎯 塔配置了AI，将使用配置的技能进行自动攻击");
                
                // 启动AI思考
                aiThinkTree.Enable();
                logger.LogInformation("✅ 塔 {Tower} AI已启动并开始自动攻击", building.Cache.Name);
            }
            else
            {
                logger.LogWarning("⚠️ 无法为塔 {Tower} 添加AI - 可能缺少TacticalAI配置", building.Cache.Name);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 启动塔AI失败");
        }
    }
#endif
}
