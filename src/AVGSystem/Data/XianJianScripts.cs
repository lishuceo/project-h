using GameEntry.AVGSystem.Data;
using GameEntry.AVGSystem.Engine;
using GameCore;

namespace GameEntry.AVGSystem.Data;

/// <summary>
/// 仙剑奇侠传剧本数据 - 数据驱动的剧情内容
/// </summary>
public static class XianJianScripts
{
    /// <summary>
    /// 注册所有仙剑剧本
    /// </summary>
    public static void RegisterAllScripts()
    {
        Game.Logger.LogInformation("📚 开始注册仙剑奇侠传剧本...");
        
        // 注册各个剧本
        AVGScriptEngine.RegisterScript(CreateOpeningScript());
        AVGScriptEngine.RegisterScript(CreateTestScript());
        AVGScriptEngine.RegisterScript(CreateCharacterMeetingScript());
        AVGScriptEngine.RegisterScript(CreateBlackWukongScript());
        AVGScriptEngine.RegisterScript(CreateBackgroundTestScript());
        
        Game.Logger.LogInformation("✅ 仙剑剧本注册完成");
    }

    /// <summary>
    /// 创建开场剧本
    /// </summary>
    private static AVGScript CreateOpeningScript()
    {
        return new AVGScript
        {
            Id = "XianJian_Opening",
            Name = "仙剑奇侠传 - 开场",
            Description = "李逍遥在余杭镇客栈的平凡生活即将改变",
            StartNodeId = "opening_001",
            Nodes = new List<AVGNode>
            {
                new BackgroundNode
                {
                    Id = "opening_bg",
                    ImagePath = "image/AVGSystem/Resources/bg1.png",
                    NextNodeId = "opening_001"
                },
                new DialogNode
                {
                    Id = "opening_001",
                    Speaker = "旁白",
                    Content = "余杭镇，一个江南小镇，青石板路，小桥流水...",
                    NextNodeId = "opening_002"
                },
                new DialogNode
                {
                    Id = "opening_002",
                    Speaker = "旁白", 
                    Content = "在这里有一家小客栈，客栈里有一个名叫李逍遥的少年。",
                    NextNodeId = "opening_003"
                },
                new DialogNode
                {
                    Id = "opening_003",
                    Speaker = "李逍遥",
                    Content = "哎，又是平凡的一天。什么时候才能有点刺激的事情发生呢？",
                    Characters = new List<CharacterDisplay>
                    {
                        new CharacterDisplay
                        {
                            Name = "李逍遥",
                            ImagePath = "image/AVGSystem/Resources/Characters/LiXiaoYao/avatar.png",
                            Position = CharacterPosition.Center,
                            Visible = true
                        }
                    },
                    NextNodeId = "opening_choice"
                },
                new ChoiceNode
                {
                    Id = "opening_choice",
                    Title = "这时，李逍遥决定...",
                    Options = new List<ChoiceOption>
                    {
                        new ChoiceOption { Text = "去镇上逛逛", NextNodeId = "town_walk" },
                        new ChoiceOption { Text = "继续在客栈工作", NextNodeId = "inn_work" },
                        new ChoiceOption { Text = "去后山练剑", NextNodeId = "mountain_practice" }
                    }
                },
                // 分支1：去镇上逛逛
                new DialogNode
                {
                    Id = "town_walk",
                    Speaker = "李逍遥",
                    Content = "难得有空闲时间，去镇上走走看看有什么新鲜事。",
                    NextNodeId = "end"
                },
                // 分支2：继续工作
                new DialogNode
                {
                    Id = "inn_work", 
                    Speaker = "李逍遥",
                    Content = "还是老老实实工作吧，婶婶交代的事情不能马虎。",
                    NextNodeId = "end"
                },
                // 分支3：去练剑
                new DialogNode
                {
                    Id = "mountain_practice",
                    Speaker = "李逍遥", 
                    Content = "去后山练练剑法，说不定能遇到什么奇遇。",
                    NextNodeId = "end"
                },
                // 结束节点
                new DialogNode
                {
                    Id = "end",
                    Speaker = "旁白",
                    Content = "无论选择什么，李逍遥的冒险即将开始...",
                    NextNodeId = null // null表示剧本结束
                }
            }
        };
    }

    /// <summary>
    /// 创建测试剧本
    /// </summary>
    private static AVGScript CreateTestScript()
    {
        return new AVGScript
        {
            Id = "AVG_Test",
            Name = "AVG系统测试",
            Description = "用于测试AVG系统各项功能",
            StartNodeId = "test_001",
            Nodes = new List<AVGNode>
            {
                new DialogNode
                {
                    Id = "test_001",
                    Speaker = "AVG系统",
                    Content = "🎉 欢迎来到AVG全屏模式测试！",
                    NextNodeId = "test_002"
                },
                new DialogNode
                {
                    Id = "test_002",
                    Speaker = "AVG系统",
                    Content = "这是完整的AVG模式，包含全屏背景图和对话系统。",
                    NextNodeId = "test_choice"
                },
                new ChoiceNode
                {
                    Id = "test_choice",
                    Title = "你想测试什么功能？",
                    Options = new List<ChoiceOption>
                    {
                        new ChoiceOption { Text = "基础对话测试", NextNodeId = "basic_test" },
                        new ChoiceOption { Text = "复杂分支测试", NextNodeId = "complex_test" },
                        new ChoiceOption { Text = "角色对话演示", NextNodeId = "character_test" },
                        new ChoiceOption { Text = "退出测试", NextNodeId = "test_end" }
                    }
                },
                new DialogNode
                {
                    Id = "basic_test",
                    Speaker = "测试员",
                    Content = "✅ 基础对话测试完成！",
                    NextNodeId = "test_end"
                },
                new DialogNode
                {
                    Id = "complex_test",
                    Speaker = "测试员", 
                    Content = "✅ 复杂分支测试完成！",
                    NextNodeId = "test_end"
                },
                new DialogNode
                {
                    Id = "character_test",
                    Speaker = "测试员",
                    Content = "✅ 角色对话演示完成！",
                    NextNodeId = "test_end"
                },
                new DialogNode
                {
                    Id = "test_end",
                    Speaker = "AVG系统",
                    Content = "感谢测试AVG系统！再见！",
                    NextNodeId = null
                }
            }
        };
    }

    /// <summary>
    /// 创建角色相遇剧本
    /// </summary>
    private static AVGScript CreateCharacterMeetingScript()
    {
        return new AVGScript
        {
            Id = "XianJian_CharacterMeeting",
            Name = "仙剑奇侠传 - 角色相遇",
            Description = "李逍遥与赵灵儿、林月如的初次相遇",
            StartNodeId = "meet_001",
            Nodes = new List<AVGNode>
            {
                new DialogNode
                {
                    Id = "meet_001",
                    Speaker = "李逍遥",
                    Content = "我是李逍遥，一个普通的客栈小二。",
                    Characters = new List<CharacterDisplay>
                    {
                        new CharacterDisplay
                        {
                            Name = "李逍遥",
                            ImagePath = "image/AVGSystem/Resources/Characters/LiXiaoYao/avatar.png",
                            Position = CharacterPosition.Center
                        }
                    },
                    NextNodeId = "meet_002"
                },
                new DialogNode
                {
                    Id = "meet_002",
                    Speaker = "赵灵儿",
                    Content = "逍遥哥哥，我是赵灵儿，很高兴认识你。",
                    Characters = new List<CharacterDisplay>
                    {
                        new CharacterDisplay
                        {
                            Name = "李逍遥", 
                            ImagePath = "image/AVGSystem/Resources/Characters/LiXiaoYao/avatar.png",
                            Position = CharacterPosition.Left
                        },
                        new CharacterDisplay
                        {
                            Name = "赵灵儿",
                            ImagePath = "image/AVGSystem/Resources/Characters/ZhaoLingEr/avatar.png", 
                            Position = CharacterPosition.Right
                        }
                    },
                    NextNodeId = "meet_003"
                },
                new DialogNode
                {
                    Id = "meet_003",
                    Speaker = "林月如",
                    Content = "哼！我是林月如，你们好像很熟的样子。",
                    Characters = new List<CharacterDisplay>
                    {
                        new CharacterDisplay
                        {
                            Name = "李逍遥",
                            ImagePath = "image/AVGSystem/Resources/Characters/LiXiaoYao/avatar.png",
                            Position = CharacterPosition.Left
                        },
                        new CharacterDisplay
                        {
                            Name = "赵灵儿",
                            ImagePath = "image/AVGSystem/Resources/Characters/ZhaoLingEr/avatar.png",
                            Position = CharacterPosition.Center
                        },
                        new CharacterDisplay
                        {
                            Name = "林月如",
                            ImagePath = "image/AVGSystem/Resources/Characters/LinYueRu/avatar.png",
                            Position = CharacterPosition.Right
                        }
                    },
                    NextNodeId = "meet_choice"
                },
                new ChoiceNode
                {
                    Id = "meet_choice",
                    Title = "李逍遥，你更喜欢谁？",
                    Options = new List<ChoiceOption>
                    {
                        new ChoiceOption { Text = "赵灵儿（温柔善良）", NextNodeId = "choose_linger" },
                        new ChoiceOption { Text = "林月如（活泼直率）", NextNodeId = "choose_yueru" },
                        new ChoiceOption { Text = "都喜欢（贪心！）", NextNodeId = "choose_both" }
                    }
                },
                // 选择赵灵儿分支
                new DialogNode
                {
                    Id = "choose_linger",
                    Speaker = "赵灵儿",
                    Content = "逍遥哥哥...💕",
                    NextNodeId = "linger_reaction"
                },
                new DialogNode
                {
                    Id = "linger_reaction",
                    Speaker = "林月如",
                    Content = "哼！没眼光！",
                    NextNodeId = "meet_end"
                },
                // 选择林月如分支
                new DialogNode
                {
                    Id = "choose_yueru",
                    Speaker = "林月如",
                    Content = "哈哈！我就知道你有眼光！",
                    NextNodeId = "yueru_reaction"
                },
                new DialogNode
                {
                    Id = "yueru_reaction",
                    Speaker = "赵灵儿",
                    Content = "逍遥哥哥...😢",
                    NextNodeId = "meet_end"
                },
                // 选择都喜欢分支
                new DialogNode
                {
                    Id = "choose_both",
                    Speaker = "赵灵儿",
                    Content = "逍遥哥哥真贪心...",
                    NextNodeId = "both_reaction1"
                },
                new DialogNode
                {
                    Id = "both_reaction1",
                    Speaker = "林月如",
                    Content = "花心大萝卜！",
                    NextNodeId = "both_reaction2"
                },
                new DialogNode
                {
                    Id = "both_reaction2",
                    Speaker = "李逍遥",
                    Content = "我...我只是觉得你们都很好啊...",
                    NextNodeId = "meet_end"
                },
                // 结束
                new DialogNode
                {
                    Id = "meet_end",
                    Speaker = "旁白",
                    Content = "无论如何，三人的缘分就此开始...",
                    NextNodeId = null
                }
            }
        };
    }

    /// <summary>
    /// 创建黑悟空剧本
    /// </summary>
    private static AVGScript CreateBlackWukongScript()
    {
        return new AVGScript
        {
            Id = "BlackWukong_Story",
            Name = "黑神话悟空 - 觉醒之路",
            Description = "一个关于黑悟空觉醒、成长与救赎的故事",
            StartNodeId = "wukong_001",
            Nodes = new List<AVGNode>
            {
                // 背景设置
                new BackgroundNode
                {
                    Id = "wukong_bg",
                    ImagePath = "image/AVGSystem/Resources/bg3.png",
                    NextNodeId = "wukong_001"
                },
                
                // 开场
                new DialogNode
                {
                    Id = "wukong_001",
                    Speaker = "旁白",
                    Content = "花果山水帘洞，一片祥和宁静。但在这宁静之下，却隐藏着不为人知的秘密...",
                    NextNodeId = "wukong_002"
                },
                
                new DialogNode
                {
                    Id = "wukong_002",
                    Speaker = "旁白",
                    Content = "传说中的齐天大圣孙悟空，在取经路上经历了无数磨难，但最大的敌人，却是他自己心中的黑暗...",
                    NextNodeId = "wukong_003"
                },
                
                // 黑悟空出现
                new DialogNode
                {
                    Id = "wukong_003",
                    Speaker = "黑悟空",
                    Content = "哈哈哈！你以为取经就能洗净你心中的戾气吗？我就是你内心最真实的一面！",
                    Characters = new List<CharacterDisplay>
                    {
                        new CharacterDisplay
                        {
                            Name = "黑悟空",
                            ImagePath = "image/AVGSystem/Resources/Characters/BlackWukong/avatar.png",
                            Position = CharacterPosition.Center,
                            Visible = true
                        }
                    },
                    NextNodeId = "wukong_004"
                },
                
                new DialogNode
                {
                    Id = "wukong_004",
                    Speaker = "孙悟空",
                    Content = "你...你是谁？为什么和我长得一模一样？",
                    Characters = new List<CharacterDisplay>
                    {
                        new CharacterDisplay
                        {
                            Name = "黑悟空",
                            ImagePath = "image/AVGSystem/Resources/Characters/BlackWukong/avatar.png",
                            Position = CharacterPosition.Left,
                            Visible = true
                        },
                        new CharacterDisplay
                        {
                            Name = "孙悟空",
                            ImagePath = "image/AVGSystem/Resources/Characters/Wukong/avatar.png",
                            Position = CharacterPosition.Right,
                            Visible = true
                        }
                    },
                    NextNodeId = "wukong_005"
                },
                
                new DialogNode
                {
                    Id = "wukong_005",
                    Speaker = "黑悟空",
                    Content = "我是你被压制的愤怒，是你被遗忘的野性，是你在五指山下五百年积累的怨恨！",
                    NextNodeId = "wukong_choice1"
                },
                
                // 第一个选择
                new ChoiceNode
                {
                    Id = "wukong_choice1",
                    Title = "面对内心的黑暗，孙悟空选择...",
                    Options = new List<ChoiceOption>
                    {
                        new ChoiceOption { Text = "拒绝承认，坚持正道", NextNodeId = "reject_darkness" },
                        new ChoiceOption { Text = "承认黑暗，寻求平衡", NextNodeId = "accept_darkness" },
                        new ChoiceOption { Text = "与黑暗对话，了解真相", NextNodeId = "talk_to_darkness" }
                    }
                },
                
                // 拒绝黑暗分支
                new DialogNode
                {
                    Id = "reject_darkness",
                    Speaker = "孙悟空",
                    Content = "我已经不是当年那个桀骜不驯的猴子了！我要走正道，保护师父！",
                    NextNodeId = "black_reaction1"
                },
                
                new DialogNode
                {
                    Id = "black_reaction1",
                    Speaker = "黑悟空",
                    Content = "哈！虚伪！你以为压抑就能让我消失吗？我会一直在你心中，等待爆发的那一刻！",
                    NextNodeId = "wukong_struggle"
                },
                
                // 承认黑暗分支
                new DialogNode
                {
                    Id = "accept_darkness",
                    Speaker = "孙悟空",
                    Content = "也许...也许你说得对。我确实有愤怒，有不甘，有被束缚的痛苦...",
                    NextNodeId = "black_reaction2"
                },
                
                new DialogNode
                {
                    Id = "black_reaction2",
                    Speaker = "黑悟空",
                    Content = "很好！承认自己的黑暗，才是真正的勇气。但是，你准备好面对真实的自己了吗？",
                    NextNodeId = "wukong_integration"
                },
                
                // 对话分支
                new DialogNode
                {
                    Id = "talk_to_darkness",
                    Speaker = "孙悟空",
                    Content = "告诉我，你为什么会出现？你想要什么？",
                    NextNodeId = "black_story"
                },
                
                new DialogNode
                {
                    Id = "black_story",
                    Speaker = "黑悟空",
                    Content = "我想要的，就是不再被压抑。五百年的囚禁，无数次的屈辱，你都选择了忍耐...",
                    NextNodeId = "black_story2"
                },
                
                new DialogNode
                {
                    Id = "black_story2",
                    Speaker = "黑悟空",
                    Content = "但愤怒和野性是你的本性！没有它们，你就不是真正的齐天大圣！",
                    NextNodeId = "wukong_understanding"
                },
                
                // 理解分支
                new DialogNode
                {
                    Id = "wukong_understanding",
                    Speaker = "孙悟空",
                    Content = "我明白了...你不是我的敌人，你是我的一部分。真正的力量，来自于接纳完整的自己。",
                    NextNodeId = "final_choice"
                },
                
                // 挣扎分支
                new DialogNode
                {
                    Id = "wukong_struggle",
                    Speaker = "孙悟空",
                    Content = "不...我不能让黑暗控制我！师父教导我要慈悲为怀...",
                    NextNodeId = "black_persistence"
                },
                
                new DialogNode
                {
                    Id = "black_persistence",
                    Speaker = "黑悟空",
                    Content = "慈悲？那些妖怪伤害无辜时，你的慈悲在哪里？你的愤怒才是保护他人的力量！",
                    NextNodeId = "wukong_realization"
                },
                
                new DialogNode
                {
                    Id = "wukong_realization",
                    Speaker = "孙悟空",
                    Content = "也许...也许愤怒和慈悲并不矛盾。为了正义而愤怒，这也是一种慈悲...",
                    NextNodeId = "final_choice"
                },
                
                // 融合分支
                new DialogNode
                {
                    Id = "wukong_integration",
                    Speaker = "孙悟空",
                    Content = "如果你是我的一部分，那么我们应该共存，而不是对抗。",
                    NextNodeId = "black_surprise"
                },
                
                new DialogNode
                {
                    Id = "black_surprise",
                    Speaker = "黑悟空",
                    Content = "什么？你...你竟然愿意接纳我？",
                    NextNodeId = "wukong_wisdom"
                },
                
                new DialogNode
                {
                    Id = "wukong_wisdom",
                    Speaker = "孙悟空",
                    Content = "真正的强者，不是消灭黑暗，而是与黑暗和谐共存。这样，我才是完整的孙悟空。",
                    NextNodeId = "final_choice"
                },
                
                // 最终选择
                new ChoiceNode
                {
                    Id = "final_choice",
                    Title = "经历了内心的对话，孙悟空决定...",
                    Options = new List<ChoiceOption>
                    {
                        new ChoiceOption { Text = "融合黑暗，成为完整的自己", NextNodeId = "ending_integration" },
                        new ChoiceOption { Text = "继续压抑，坚持现在的道路", NextNodeId = "ending_suppression" },
                        new ChoiceOption { Text = "寻找平衡，在光明与黑暗间行走", NextNodeId = "ending_balance" }
                    }
                },
                
                // 融合结局
                new DialogNode
                {
                    Id = "ending_integration",
                    Speaker = "孙悟空",
                    Content = "从今以后，我不再逃避自己的黑暗。愤怒和慈悲，野性和理智，都是我的力量！",
                    Characters = new List<CharacterDisplay>
                    {
                        new CharacterDisplay
                        {
                            Name = "完整悟空",
                            ImagePath = "image/AVGSystem/Resources/Characters/CompleteWukong/avatar.png",
                            Position = CharacterPosition.Center,
                            Visible = true
                        }
                    },
                    NextNodeId = "ending_final1"
                },
                
                new DialogNode
                {
                    Id = "ending_final1",
                    Speaker = "黑悟空",
                    Content = "哈哈！这才是真正的齐天大圣！我们终于合而为一了！",
                    NextNodeId = "story_end"
                },
                
                // 压抑结局
                new DialogNode
                {
                    Id = "ending_suppression",
                    Speaker = "孙悟空",
                    Content = "我选择继续现在的道路。黑暗终究是黑暗，不能让它影响我保护师父的决心。",
                    NextNodeId = "black_warning"
                },
                
                new DialogNode
                {
                    Id = "black_warning",
                    Speaker = "黑悟空",
                    Content = "愚蠢！你以为这样就结束了？我会在你最脆弱的时候回来！到时候，你会后悔今天的选择！",
                    NextNodeId = "story_end"
                },
                
                // 平衡结局
                new DialogNode
                {
                    Id = "ending_balance",
                    Speaker = "孙悟空",
                    Content = "我不会完全拥抱黑暗，也不会完全压抑它。我要学会在光明与黑暗之间找到平衡。",
                    NextNodeId = "black_respect"
                },
                
                new DialogNode
                {
                    Id = "black_respect",
                    Speaker = "黑悟空",
                    Content = "有趣...这倒是个聪明的选择。那么，我们就在这种微妙的平衡中共存吧。",
                    NextNodeId = "wisdom_gained"
                },
                
                new DialogNode
                {
                    Id = "wisdom_gained",
                    Speaker = "孙悟空",
                    Content = "真正的智慧，不是非黑即白，而是在灰色地带中找到属于自己的道路。",
                    NextNodeId = "story_end"
                },
                
                // 故事结束
                new DialogNode
                {
                    Id = "story_end",
                    Speaker = "旁白",
                    Content = "无论选择什么道路，孙悟空都在这次内心的对话中获得了成长。真正的英雄之路，从来不是一帆风顺的...",
                    NextNodeId = null // 故事结束
                }
            }
        };
    }

    /// <summary>
    /// 创建背景图片测试剧本 - 4句话使用4个背景
    /// </summary>
    private static AVGScript CreateBackgroundTestScript()
    {
        return new AVGScript
        {
            Id = "Background_Test",
            Name = "背景图片展示测试",
            Description = "展示4个背景图片的对话测试",
            StartNodeId = "bg_test_001",
            Nodes = new List<AVGNode>
            {
                // 第一句话 - 背景1
                new BackgroundNode
                {
                    Id = "set_bg1",
                    ImagePath = "image/AVGSystem/Resources/bg1.png",
                    NextNodeId = "bg_test_001"
                },
                
                new DialogNode
                {
                    Id = "bg_test_001",
                    Speaker = "导游",
                    Content = "🌸 欢迎来到第一个场景！这里是春天的花园，樱花飞舞，生机盎然。",
                    NextNodeId = "set_bg2"
                },
                
                // 第二句话 - 背景2
                new BackgroundNode
                {
                    Id = "set_bg2",
                    ImagePath = "image/AVGSystem/Resources/bg2.png",
                    NextNodeId = "bg_test_002"
                },
                
                new DialogNode
                {
                    Id = "bg_test_002",
                    Speaker = "导游",
                    Content = "🏔️ 现在我们来到了第二个场景！这里是雄伟的山峦，云雾缭绕，仙气飘飘。",
                    NextNodeId = "set_bg3"
                },
                
                // 第三句话 - 背景3
                new BackgroundNode
                {
                    Id = "set_bg3",
                    ImagePath = "image/AVGSystem/Resources/bg3.png",
                    NextNodeId = "bg_test_003"
                },
                
                new DialogNode
                {
                    Id = "bg_test_003",
                    Speaker = "导游",
                    Content = "🌊 接下来是第三个场景！这里是宁静的湖泊，水面如镜，倒映着天空的云彩。",
                    NextNodeId = "set_bg4"
                },
                
                // 第四句话 - 背景4
                new BackgroundNode
                {
                    Id = "set_bg4",
                    ImagePath = "image/AVGSystem/Resources/bg4.png",
                    NextNodeId = "bg_test_004"
                },
                
                new DialogNode
                {
                    Id = "bg_test_004",
                    Speaker = "导游",
                    Content = "🏮 最后是第四个场景！这里是古典的室内，温馨的灯光，仿佛回到了古代的雅致生活。",
                    NextNodeId = "bg_test_end"
                },
                
                // 结束
                new DialogNode
                {
                    Id = "bg_test_end",
                    Speaker = "系统",
                    Content = "✨ 背景图片展示测试完成！你已经体验了所有4个背景场景。",
                    NextNodeId = null // 测试结束
                }
            }
        };
    }
}
