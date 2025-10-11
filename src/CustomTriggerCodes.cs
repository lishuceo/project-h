#pragma warning disable CS1998

using TriggerEncapsulation;

namespace p_0tja
{
    public partial class Scope
    {
        static public double CustomFanOfBladesDamage(GameCore.Execution.Effect context)
        {
            uint param = 0;
            double param1 = 0;
            param = context.Level;
            param1 = (double)param;
            return 100 + 50 * (double)param;
        }

        static public bool LightningChainInitialize(GameCore.Execution.Effect effect)
        {
            // 初始化弹射单位列表到施法者上
            var caster = effect.Caster;
            var chainedUnits = new global::System.Collections.Generic.HashSet<global::GameCore.EntitySystem.Entity>();
            caster.UserDataSet("LightningChainTargets", chainedUnits);

            Game.Logger.LogInformation("闪电链初始化：在施法者 {CasterId} 上创建弹射目标列表", caster.EntityId);

            return true; // 初始化成功，继续执行后续效果
        }

        static public bool LightningChainLogicJudgment(GameCore.Execution.Effect effect)
        {
            // 获取当前目标单位
            var currentTarget = effect.Target?.Unit;
            if (currentTarget == null) return false;

            // 从施法者获取已弹射单位列表
            var caster = effect.Caster;
            Game.Logger.LogInformation("caster{Target} source{Source} Target{Target} ", caster.EntityId, effect.Source.Unit.EntityId, currentTarget.EntityId);
            var chainedUnits = caster.UserDataGet("LightningChainTargets") as global::System.Collections.Generic.HashSet<global::GameCore.EntitySystem.Entity>;
            if (chainedUnits == null)
            {
                Game.Logger.LogWarning("闪电链目标记录：未找到弹射目标列表，可能初始化失败");
                return false;
            }

            Game.Logger.LogInformation("闪电链目标记录：当前目标 {CurrentTarget}，已弹射 {ChainCount} 个单位",
                currentTarget.EntityId, chainedUnits.Count);
            // 记录当前目标
            chainedUnits.Add(effect.Target?.Unit);
            Game.Logger.LogInformation("闪电链目标记录：将目标 {CurrentTarget} 添加到弹射列表，当前列表大小 {Count}",
                effect.Target?.Unit.EntityId, chainedUnits.Count);

            // 检查是否已达到最大弹射次数（5次）
            if (chainedUnits.Count >= 5)
            {
                Game.Logger.LogInformation("闪电链目标记录：已达到最大弹射次数(5次)，停止弹射");
                return false;
            }

            return true; // 继续执行后续效果（伤害和下一次弹射）
        }

        static public GameCore.BaseType.CmdResult LightningChainSearchValidator(GameCore.Execution.Effect effect)
        {
            // 从施法者获取已弹射单位列表
            var caster = effect.Caster;
            Game.Logger.LogInformation("caster{Target} source{Source} DefaultTarget{DefaultTarget} ", caster.EntityId, effect.Source.Unit.EntityId, effect.DefaultTarget?.Unit.EntityId);
            var chainedUnits = caster.UserDataGet("LightningChainTargets") as global::System.Collections.Generic.HashSet<global::GameCore.EntitySystem.Entity>;
            foreach (var unit in chainedUnits)
            {
                Game.Logger.LogInformation("搜索{Target} ", unit.EntityId);
                if (unit == effect.Target?.Unit)
                {
                    Game.Logger.LogInformation("闪电链搜索验证：目标 {Target} 已被弹射过，停止搜索", effect.Target?.Unit.EntityId);
                    return global::GameCore.BaseType.CmdError.InvalidTarget;
                }
            }
            return global::GameCore.BaseType.CmdResult.Ok;
        }

        // =================== 闪电箭技能函数 ===================

        static public bool LightningArrowInitialize(GameCore.Execution.Effect effect)
        {
            // 初始化闪电箭弹射单位列表到施法者上
            var caster = effect.Caster;
            var chainedUnits = new global::System.Collections.Generic.HashSet<global::GameCore.EntitySystem.Entity>();
            caster.UserDataSet("LightningArrowTargets", chainedUnits);

            Game.Logger.LogInformation("闪电箭初始化：在施法者 {CasterId} 上创建弹射目标列表", caster.EntityId);

            return true; // 初始化成功，继续执行后续效果
        }

        static public double LightningArrowDamageCalculation(GameCore.Execution.Effect effect)
        {
            // 闪电箭伤害计算：基础伤害85，每次弹射衰减15%
            var caster = effect.Caster;
            var chainedUnits = caster.UserDataGet("LightningArrowTargets") as global::System.Collections.Generic.HashSet<global::GameCore.EntitySystem.Entity>;

            double baseDamage = 85.0;
            int bounceCount = chainedUnits?.Count ?? 0;

            // 伤害计算：85 * (85/100)^弹射次数
            double damage = baseDamage * global::System.Math.Pow(0.85, bounceCount);

            Game.Logger.LogInformation("闪电箭伤害计算：弹射次数 {BounceCount}，伤害 {Damage}", bounceCount, damage);

            return damage;
        }

        static public bool LightningArrowLogicJudgment(GameCore.Execution.Effect effect)
        {
            // 获取当前目标单位
            var currentTarget = effect.Target?.Unit;
            if (currentTarget == null) return false;

            // 从施法者获取已弹射单位列表
            var caster = effect.Caster;
            Game.Logger.LogInformation("闪电箭弹射判断：caster{Caster} source{Source} target{Target}",
                caster.EntityId, effect.Source.Unit?.EntityId, currentTarget.EntityId);

            var chainedUnits = caster.UserDataGet("LightningArrowTargets") as global::System.Collections.Generic.HashSet<global::GameCore.EntitySystem.Entity>;
            if (chainedUnits == null)
            {
                Game.Logger.LogWarning("闪电箭弹射判断：未找到弹射目标列表，可能初始化失败");
                return false;
            }

            Game.Logger.LogInformation("闪电箭弹射判断：当前目标 {CurrentTarget}，已弹射 {ChainCount} 个单位",
                currentTarget.EntityId, chainedUnits.Count);

            // 记录当前目标
            chainedUnits.Add(currentTarget);
            Game.Logger.LogInformation("闪电箭弹射判断：将目标 {CurrentTarget} 添加到弹射列表，当前列表大小 {Count}",
                currentTarget.EntityId, chainedUnits.Count);

            return true; // 记录完成，继续执行后续效果
        }

        static public GameCore.BaseType.CmdResult LightningArrowSearchValidator(GameCore.Execution.Effect effect)
        {
            // 验证搜索目标是否已被弹射过，并检查弹射次数上限
            Game.Logger.LogInformation("验证搜索目标是否已被弹射过, caster{Caster} target{Target}", effect.Caster.EntityId, effect.Target?.Unit.EntityId);
            var caster = effect.Caster;
            var chainedUnits = caster.UserDataGet("LightningArrowTargets") as global::System.Collections.Generic.HashSet<global::GameCore.EntitySystem.Entity>;
            Game.Logger.LogInformation("chainedUnits{ChainedUnits}", chainedUnits?.Count);
            if (chainedUnits == null) return global::GameCore.BaseType.CmdResult.Ok;

            // 检查是否已达到最大弹射次数（3次）
            if (chainedUnits.Count >= 3)
            {
                Game.Logger.LogInformation("闪电箭搜索验证：已达到最大弹射次数(3次)，停止搜索");
                return global::GameCore.BaseType.CmdError.InvalidTarget;
            }

            var targetUnit = effect.Target?.Unit;
            if (targetUnit != null && chainedUnits.Contains(targetUnit))
            {
                Game.Logger.LogInformation("闪电箭搜索验证：目标 {Target} 已被弹射过，跳过此目标", targetUnit.EntityId);
                return global::GameCore.BaseType.CmdError.InvalidTarget;
            }

            return global::GameCore.BaseType.CmdResult.Ok;
        }

        // =================== 寒冰球技能函数 ===================

        static public global::GameCore.BaseType.Angle IceOrbRandomAngle(GameCore.Execution.Effect effect)
        {
            // 返回0-360度之间的随机角度
            var random = new global::System.Random();
            float angle = (float)(random.NextDouble() * 360.0);
            
            Game.Logger.LogInformation("寒冰球碎片随机角度：{Angle}度", angle);
            
            return global::GameCore.BaseType.Angle.FromDegree(angle);  // 使用FromDegree创建Angle
        }

        static public float IceOrbRandomDistance(GameCore.Execution.Effect effect)
        {
            // 返回随机距离，用于碎片发射的目标点距离
            var random = new global::System.Random();
            float distance = (float)(400.0 + random.NextDouble() * 600.0);  // 400-1000之间的随机距离
            
            Game.Logger.LogInformation("寒冰球碎片随机距离：{Distance}", distance);
            
            return distance;
        }

        // =================== 预警陨石技能函数 ===================

        static public global::System.TimeSpan MeteorRandomDelay(GameCore.Execution.Effect effect)
        {
            // 返回0-300毫秒的随机延迟
            var random = new global::System.Random();
            double delayMs = random.NextDouble() * 300.0;  // 0-300毫秒随机延迟
            
            Game.Logger.LogInformation("陨石随机延迟：{Delay}毫秒", delayMs);
            
            return global::System.TimeSpan.FromMilliseconds(delayMs);
        }

    }
}

#pragma warning restore CS1998
