using GameCore.PlayerAndUsers;

namespace GameEntry.ModernUITest;

/// <summary>
/// ModernUITest项目中扩展的PropertyPlayerUI属性
/// 演示可扩展枚举功能和UI属性序列化测试
/// </summary>
[EnumExtension(Extendable = false, Extends = typeof(PropertyPlayerUI))]
public enum EPropertyPlayerUIExtended
{
    /// <summary>
    /// 主题颜色偏好设置 - 存储用户选择的主题颜色(如深色/浅色模式)
    /// </summary>
    ThemeColorPreference,
    
    /// <summary>
    /// 通知设置 - JSON对象，包含各种通知开关和参数
    /// </summary>
    NotificationSettings,
    
    /// <summary>
    /// 窗口大小和位置 - JSON对象，存储自定义窗口布局信息
    /// </summary>
    WindowLayoutData,
    
    /// <summary>
    /// 用户自定义快捷操作 - JSON数组，存储用户定义的快捷操作序列
    /// </summary>
    CustomQuickActions,
    
    /// <summary>
    /// 界面透明度设置 - 浮点数值，控制UI元素的透明度
    /// </summary>
    UITransparencyLevel,
    
    /// <summary>
    /// 语言偏好设置 - 字符串，用户选择的语言代码
    /// </summary>
    LanguagePreference,
    
    /// <summary>
    /// 高级显示选项 - JSON对象，包含帧率显示、调试信息等开关
    /// </summary>
    AdvancedDisplayOptions,
    
    /// <summary>
    /// 音频设置 - JSON对象，包含音量、音效开关等音频相关设置
    /// </summary>
    AudioSettings,
    
    /// <summary>
    /// 自动保存间隔 - 整数，表示自动保存的时间间隔(秒)
    /// </summary>
    AutoSaveInterval,
    
    /// <summary>
    /// 游戏偏好设置 - JSON对象，存储各种游戏玩法偏好
    /// </summary>
    GameplayPreferences
}
