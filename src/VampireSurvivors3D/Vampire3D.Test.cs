#if SERVER
using Events;
using GameCore.Components;
using GameCore.Container;
using GameCore.Event;
using GameCore.Interface;
using GameCore.SceneSystem;
using GameData.Extension;

namespace GameEntry.VampireSurvivors3D;

internal class PassiveAbilitiesTest : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.VampireSurvivors3D)
        {
            return;
        }

        Trigger<EventPlayerKeyDown> testKeyTrigger = new(async (s, d) =>
        {
            var player1 = Player.GetById(1);
            var hero = player1?.MainUnit;

            switch (d.Key)
            {
                case GameCore.Platform.SDL.VirtualKey.F5:
                    await TestPassiveAbilities(hero);
                    break;
            }
            return false;
        }, keepReference: true);
        testKeyTrigger.Register(Game.Instance);

        Game.Logger.Log(LogLevel.Warning, "🧪 Passive Abilities Test System - Press F5 to test");
    }

    private static async Task TestPassiveAbilities(Unit? hero)
    {
        if (hero == null)
        {
            Game.Logger.LogWarning("❌ No hero unit found for testing");
            return;
        }

        Game.Logger.LogInformation("🧪 Testing Passive Abilities for Hero: {hero}", hero.Cache.Name);

        var abilities = hero.Cache.Abilities;
        if (abilities == null || abilities.Count == 0)
        {
            Game.Logger.LogWarning("❌ Hero has no abilities configured");
            return;
        }

        Game.Logger.LogInformation("✅ Hero has {count} abilities configured", abilities.Count);
        
        foreach (var abilityLink in abilities)
        {
            var ability = abilityLink?.Data;
            if (ability != null)
            {
                var name = ability.DisplayName?.ToString() ?? "Unknown";
                var period = ability.PassivePeriod?.Invoke(null);
                var hasPassiveEffect = ability.PassivePeriodicEffect != null;
                
                Game.Logger.LogInformation("   📝 {name} - Period: {period} - HasEffect: {hasEffect}", 
                    name, period, hasPassiveEffect);
            }
        }
    }
}
#endif 