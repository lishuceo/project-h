#if CLIENT
using Events;
using GameCore.Event;
using GameCore.BaseType;
using GameCore.OrderSystem;
using GameCore.PlayerAndUsers;
using GameCore.Components;
using GameCore.AbilitySystem;
using GameCore.AbilitySystem.Manager;
using GameCore.EntitySystem;
using GameCore.SceneSystem.Data.Struct;
using EngineInterface.BaseType;
using GameCore.SceneSystem;
using GameUI.Brush;
using GameUI.Control;
using GameUI.Control.Advanced;
using GameUI.Control.Enum;
using GameUI.Control.Primitive;
using GameUI.Control.Struct;
using GameUI.Device;
using GameUI.Enum;
using GameUI.Struct;
using System.Drawing;
using System.Numerics;

namespace GameEntry.NormalTest;

/// <summary>
/// 充能技能UI系统，显示技能状态和提供施放按钮
/// </summary>
public class ChargedSpellUI : IGameClass
{
    #region Fields

    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Panel? mainPanel;
    private static Label? titleLabel;
    private static Label? cooldownLabel;
    private static Label? chargeTimeLabel;
    private static Label? chargeCountLabel;
    private static Button? castSpellButton;
    private static Label? statusLabel;
    public static Unit? PlayerMainUnit => Player.LocalPlayer.MainUnit;
    private static AbilityActive? chargedSpellAbility;
    
    private static bool isUIActive = false;

    #endregion

    #region Initialization

    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("🎯 ChargedSpellUI: Registering game class");
        Game.OnGameTriggerInitialization += Game_OnGameTriggerInitialization;
        
        // 注册UI初始化事件
        Game.OnGameUIInitialization += OnGameUIInitialization;
        
        Game.Logger.LogInformation("✅ ChargedSpellUI: Game class registered");
    }

    private static void Game_OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != GameCore.ScopeData.GameMode.Default)
        {
            return;
        }
        // 注册游戏开始事件
        gameStartTrigger = new Trigger<EventGameStart>(OnGameStartAsync, true);
        gameStartTrigger.Register(Game.Instance);
    }

    private static async Task<bool> OnGameStartAsync(object sender, EventGameStart eventArgs)
    {        
        // 检查当前游戏模式是否为默认模式

        Game.Logger.LogInformation("🚀 ChargedSpellUI: Game started, setting up UI...");
        SetupUI();
        return true;
    }

    private static void OnGameUIInitialization()
    {
        // 检查当前游戏模式是否为默认模式
        if (Game.GameModeLink != GameCore.ScopeData.GameMode.Default)
        {
            Game.Logger.LogInformation("⏭️ ChargedSpellUI: Not default game mode, skipping UI initialization");
            return;
        }

        Game.Logger.LogInformation("✅ ChargedSpellUI: Default game mode detected, initializing UI...");
        SetupUI();
    }

    private static void SetupUI()
    {
        try
        {
            if (isUIActive)
            {
                Game.Logger.LogWarning("⚠️ ChargedSpellUI: UI already active, skipping setup");
                return;
            }

            CreateChargedSpellUI();
            isUIActive = true;
            _ = StartUpdateLoop();
            
            Game.Logger.LogInformation("✅ ChargedSpellUI: UI setup completed");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ ChargedSpellUI: Failed to setup UI");
        }
    }

    #endregion

    #region UI Creation

    private static void CreateChargedSpellUI()
    {
        try
        {
            // 创建主面板
            mainPanel = new Panel()
            {
                Width = 300,
                Height = 200,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                CornerRadius = 8,
                Padding = new Thickness(15, 15, 15, 15)
            };

            // 标题标签
            titleLabel = new Label()
            {
                Text = "⚡ 充能测试技能",
                FontSize = 16,
                TextColor = new SolidColorBrush(Color.Gold),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10),
                Parent = mainPanel
            };

            // 冷却时间标签
            cooldownLabel = new Label()
            {
                Text = "⏱️ 冷却时间: --",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 30, 0, 5),
                Parent = mainPanel
            };

            // 充能时间标签
            chargeTimeLabel = new Label()
            {
                Text = "🔋 充能时间: --",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 50, 0, 5),
                Parent = mainPanel
            };

            // 充能次数标签
            chargeCountLabel = new Label()
            {
                Text = "💎 剩余充能: --",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 70, 0, 5),
                Parent = mainPanel
            };

            // 施放按钮
            castSpellButton = new Button()
            {
                Width = 120,
                Height = 35,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 100, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(200, 100, 150, 255)),
                CornerRadius = 6,
                Parent = mainPanel
            };

            // 按钮文本
            var buttonLabel = new Label()
            {
                Text = "🔮 施放技能",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = castSpellButton
            };

            // 状态标签
            statusLabel = new Label()
            {
                Text = "状态: 等待获取单位信息...",
                FontSize = 10,
                TextColor = new SolidColorBrush(Color.LightGray),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 145, 0, 0),
                Parent = mainPanel
            };

            // 绑定按钮点击事件
            castSpellButton.OnPointerClicked += OnCastSpellButtonClick;

            // 添加到UI根
            UIRoot.Instance.AddChild(mainPanel);

            Game.Logger.LogInformation("🎨 ChargedSpellUI: UI created successfully");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ ChargedSpellUI: Failed to create UI");
        }
    }

    #endregion

    #region Event Handlers

    private static void OnCastSpellButtonClick(object? sender, PointerEventArgs args)
    {
        try
        {
            if (PlayerMainUnit == null)
            {
                UpdateStatusLabel("❌ 找不到主控单位");
                Game.Logger.LogWarning("⚠️ ChargedSpellUI: Cannot cast spell - no main unit");
                return;
            }

            if (chargedSpellAbility == null)
            {
                UpdateStatusLabel("❌ 找不到充能技能");
                Game.Logger.LogWarning("⚠️ ChargedSpellUI: Cannot cast spell - no charged spell ability");
                return;
            }

            // 检查技能是否可用
            if (!chargedSpellAbility.Charge?.Affordable ?? false)
            {
                UpdateStatusLabel("❌ 充能不足");
                Game.Logger.LogInformation("⚠️ ChargedSpellUI: Cannot cast spell - not enough charges");
                return;
            }

            if (!chargedSpellAbility.Cooldown?.Affordable ?? false)
            {
                UpdateStatusLabel("❌ 技能冷却中");
                Game.Logger.LogInformation("⚠️ ChargedSpellUI: Cannot cast spell - on cooldown");
                return;
            }

            // 获取目标位置（向前方施放）
            var unitPosition = PlayerMainUnit.Position;
            var facing = PlayerMainUnit.Facing;
            var targetDistance = 300f; // 向前300单位施放
            var targetX = unitPosition.X + Math.Cos(facing.Radian) * targetDistance;
            var targetY = unitPosition.Y + Math.Sin(facing.Radian) * targetDistance;
            var targetPosition = new ScenePoint(new Vector3((float)targetX, (float)targetY, unitPosition.Z), PlayerMainUnit.Scene);

            // 发出技能施放指令
            var command = new Command
            {
                Index = CommandIndex.Execute,
                Type = ComponentTagEx.AbilityManager,
                AbilityLink = ScopeData.Ability.ChargedTestSpell,
                Target = targetPosition,
                Player = PlayerMainUnit.Player,
                Flag = CommandFlag.IsUser
            };

            var result = command.IssueOrder(PlayerMainUnit);
            if (result.IsSuccess)
            {
                UpdateStatusLabel("✅ 技能施放成功!");
                Game.Logger.LogInformation("✨ ChargedSpellUI: Spell cast successfully");
            }
            else
            {
                UpdateStatusLabel($"❌ 施放失败: {result.Error}");
                Game.Logger.LogWarning("⚠️ ChargedSpellUI: Spell cast failed: {Result}", result.Error);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ ChargedSpellUI: Error in cast spell button click");
            UpdateStatusLabel("❌ 施放出错");
        }
    }

    #endregion

    #region Update Loop

    private static async Task StartUpdateLoop()
    {
        try
        {
            Game.Logger.LogInformation("🔄 ChargedSpellUI: Starting update loop");
            
            while (isUIActive)
            {
                UpdateUIData();
                await Game.Delay(TimeSpan.FromMilliseconds(100)); // 每100ms更新一次
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ ChargedSpellUI: Error in update loop");
        }
    }

    private static void UpdateUIData()
    {
        try
        {
            // 获取玩家主控单位
            if (PlayerMainUnit == null)
            {
                UpdateStatusLabel("状态: 等待获取主控单位...");
                return;
            }

            // 获取充能技能
            if (chargedSpellAbility == null)
            {
                chargedSpellAbility = GetChargedSpellAbility();
                if (chargedSpellAbility == null)
                {
                    UpdateStatusLabel("状态: 等待获取技能...");
                    return;
                }
            }

            // 更新UI信息
            UpdateCooldownInfo();
            UpdateChargeInfo();
            UpdateButtonState();
            UpdateStatusLabel("状态: 准备就绪");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ ChargedSpellUI: Error updating UI data");
            UpdateStatusLabel("状态: 更新出错");
        }
    }

    private static AbilityActive? GetChargedSpellAbility()
    {
        try
        {
            if (PlayerMainUnit == null) return null;

            var abilityManager = PlayerMainUnit.GetComponent<AbilityManager>();
            if (abilityManager == null) return null;

            return abilityManager.Get(ScopeData.Ability.ChargedTestSpell) as AbilityActive;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ ChargedSpellUI: Error getting charged spell ability");
            return null;
        }
    }

    private static void UpdateCooldownInfo()
    {
        if (cooldownLabel == null || chargedSpellAbility?.Cooldown == null) return;

        var cooldown = chargedSpellAbility.Cooldown;
        var remainingTime = cooldown.RemainingTime;
        var maxTime = cooldown.MaxTime;

        if (remainingTime.HasValue && remainingTime.Value.TotalSeconds > 0)
        {
            cooldownLabel.Text = $"⏱️ 冷却时间: {remainingTime.Value.TotalSeconds:F1}s";
            cooldownLabel.TextColor = new SolidColorBrush(Color.Orange);
        }
        else
        {
            cooldownLabel.Text = "⏱️ 冷却时间: 就绪";
            cooldownLabel.TextColor = new SolidColorBrush(Color.LightGreen);
        }
    }

    private static void UpdateChargeInfo()
    {
        if (chargeTimeLabel == null || chargeCountLabel == null || chargedSpellAbility?.Charge == null) return;

        var charge = chargedSpellAbility.Charge;
        var remainingTime = charge.RemainingTime;
        var chargeRemain = charge.ChargeRemain;
        var chargeMax = charge.ChargeMax;

        // 更新充能时间
        if (remainingTime.HasValue && remainingTime.Value.TotalSeconds > 0)
        {
            chargeTimeLabel.Text = $"🔋 充能时间: {remainingTime.Value.TotalSeconds:F1}s";
            chargeTimeLabel.TextColor = new SolidColorBrush(Color.Yellow);
        }
        else
        {
            chargeTimeLabel.Text = "🔋 充能时间: 充能完成";
            chargeTimeLabel.TextColor = new SolidColorBrush(Color.LightGreen);
        }

        // 更新充能次数
        chargeCountLabel.Text = $"💎 剩余充能: {chargeRemain:F0}/{chargeMax:F0}";
        
        if (chargeRemain >= chargeMax)
        {
            chargeCountLabel.TextColor = new SolidColorBrush(Color.LightGreen);
        }
        else if (chargeRemain >= 1)
        {
            chargeCountLabel.TextColor = new SolidColorBrush(Color.Yellow);
        }
        else
        {
            chargeCountLabel.TextColor = new SolidColorBrush(Color.Red);
        }
    }

    private static void UpdateButtonState()
    {
        if (castSpellButton == null || chargedSpellAbility == null) return;

        var canCast = (chargedSpellAbility.Charge?.Affordable ?? false) && 
                       (chargedSpellAbility.Cooldown?.Affordable ?? true);

        if (canCast)
        {
            castSpellButton.Background = new SolidColorBrush(Color.FromArgb(200, 100, 150, 255));
            castSpellButton.Disabled = false;
        }
        else
        {
            castSpellButton.Background = new SolidColorBrush(Color.FromArgb(100, 80, 80, 80));
            castSpellButton.Disabled = true;
        }
    }

    private static void UpdateStatusLabel(string status)
    {
        if (statusLabel != null)
        {
            statusLabel.Text = $"状态: {status}";
        }
    }

    #endregion
}
#endif