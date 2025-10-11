# AI开发文档 - 游戏物理系统

## 系统概述

这是一个基于物理引擎的游戏开发框架，支持创建物理对象、碰撞检测和脚本组件系统。框架使用Unreal Engine风格的坐标系。
此框架已经处理了``主控单位``的移动、跳跃等角色操作

> 📚 **API参考文档**: 查看 [客户端物理系统API](../api-client-reference/GameCore.PhysicsSystem.xml) 和 [服务端物理系统API](../api-server-reference/GameCore.PhysicsSystem.xml) 了解物理属性的详细定义。

## 坐标系统

### Unreal Engine左手坐标系
- **X轴**: 向前（Forward）- 正值向前
- **Y轴**: 向右（Right）- 正值向右  
- **Z轴**: 向上（Up）- 正值向上

### 示例坐标使用
```csharp
// 地面平台 - X轴延伸，Z=0为地面
new Vector3(1000, 0, 0)  // 前方地面

// 悬浮平台 - Z值表示高度
new Vector3(1000, 0, 400)  // 前方400单位高的平台

// 侧面物体 - Y值偏移
new Vector3(1000, 200, 0)  // 右侧200单位的地面物体
```

注意：Vector3使用的是C#系统自带的结构：System.Numerics.Vector3

## 核心API

### 1. 场景管理
```csharp
using GameCorePhysics.Actor;
GameCore.SceneSystem.Scene scene = GameCore.SceneSystem.Scene.Get(GameEntry.PhysicsGameData.Scene.PhysicsScene);
```

### 2. 物理对象创建、销毁
```csharp
// 基础形状创建
var actor = new PhysicsActor(
    Player.LocalPlayer,                                    // 玩家引用
    PhysicsActor.GetPrimitiveLink(PrimitiveShape.Cube),   // 形状
    scene,                                                 // 场景
    new Vector3(x, y, z),                                 // 位置
    Vector3.Zero                                          // 旋转
);

// 销毁物理对象
PhysicsActor.DestroyImmediately(actor);
```

### 3.修改物体位置和缩放
```csharp
// 获取物体节点
Node node = actor.GetNode();

// 获取物体位置
Vector3 position = node.position;

// 修改物体位置
node.position = new Vector3(0, 0, 100);

// 获取物体本地位置
Vector3 localPosition = node.localPosition;

// 修改物体本地位置
node.localPosition = new Vector3(0, 0, 100);

// 获取物体本地缩放
Vector3 localScale = node.localScale;

// 设置物体本地缩放
node.localScale = new Vector3(10, 10, 10);
```

### 4.获取主控单位，并且为它创建物理对象
```
// 获取主控单位
var mainUnit = Player.LocalPlayer.MainUnit;
// 为主控单位创建物理对象
var physicsActor = new PhysicsActor(mainUnit);
```

主控物理对象默认``碰撞Layer``等于1，默认``碰撞Mask``等于unsigned最大值

#### 支持的形状类型
- `PrimitiveShape.Cube` - 立方体/矩形（半径50）
- `PrimitiveShape.Sphere` - 球体（半径50）
- `PrimitiveShape.Cylinder` - 圆柱体（半径50，高度100）
- `PrimitiveShape.Cone` - 圆锥体（半径50，高度100）
- `PrimitiveShape.Capsule` - 胶囊体（半径50，高度200）

基础形状类型默认碰撞Layer等于2，默认Mask等于unsigned最大值

### 5. 脚本组件系统

#### 创建脚本组件
``ScriptComponent``继承自``Component``

```csharp
using EngineInterface.Urho3DInterface;

public class MyGameComponent : ScriptComponent
{
    public override void OnStart()
    {
        // 组件启动时调用
    }

    public override void OnDelayedStart()
    {
        
    }

    public override void OnStop()
    {
        // 组件结束时调用
    }

    public override void OnUpdate(float timeStep)
    {
        // 每个渲染帧更新
    }

    public override void OnPostUpdate(float timeStep)
    {
        // 每个渲染帧更新
    }

    public override void OnFixedUpdate(float timeStep)
    {
        // 每个物理帧更新
    }

    public override void OnFixedPostUpdate(float timeStep)
    {
        // 每个物理帧更新
    }

    public override void OnTriggerEnter(Node node)
    {
        // 碰撞开始时调用
        Console.WriteLine("检测到碰撞!");
    }

    public override void OnTriggerStay(Node node)
    {
        // 碰撞持续时调用
    }

    public override void OnTriggerExit(Node node)
    {
        // 碰撞结束时调用
    }
}
```

### 6. 刚体组件
``RigidBody``继承自``Component``

```csharp
// 通过node获取刚体组件
RigidBody rigidBody = node.GetComponent<RigidBody>();

// 设置力
rigidBody.ApplyForce(new Vector3(0, 0, 100));

// 设置速度
rigidBody.SetLinearVelocity(new Vector3(0, 0, 100));

// 设置重力开关
rigidBody.SetUseGravity(true);

// 获取重力开关
rigidBody.GetUseGravity();

// 设置碰撞Layer
rigidBody.SetCollisionLayer(1u);

// 获取碰撞Layer
uint layer = rigidBody.GetCollisionLayer();

// 设置碰撞Mask
rigidBody.SetCollisionMask(1u);
```

``碰撞Layer``和``碰撞Mask``的工作原理：
当物理引擎检查两个物体（A 和 B）是否应该碰撞时，它会执行一个位运算：
if ( (A.Layer & B.Mask) != 0 && (B.Layer & A.Mask) != 0 )
只有同时满足这两个条件，碰撞才会发生。

物体A的层 必须在 物体B的遮罩 中

物体B的层 必须在 物体A的遮罩 中

注意：不能主动创建RigidBody组件，通过PhysicsActor获取的Node里面已经自动创建好RigidBody了


### 7.碰撞过滤器
```csharp
RigidBody rigidBody = node.GetComponent<RigidBody>();
// 设置碰撞过滤器
rigidBody.SetCollisionFilter((RigidBody otherRigidBody, Vector3 contactPoint) =>
{
    // 返回true表示忽略碰撞
    return true;
});
```

备注：不允许在过滤函数里面修改物理属性！！！

#### 添加组件到物理对象
```csharp
// 方式1: 直接创建
Node node = physicsActor.GetNode();
node.CreateComponent<MyGameComponent>();

// 方式2: 实例化后添加（推荐用于需要传参的组件）
Node node = physicsActor.GetNode();
MyGameComponent component = new MyGameComponent();
node.AddComponent<MyGameComponent>(component);
```

#### 获取组件节点
```csharp
// 组件定义
Component component;

// 通过组件获取节点
Node node = component.node;
```

### 7. 图形相关类型定义
```csharp
namespace EngineInterface.Urho3DInterface.Graphics;

/// <summary>
/// Primitive type.
/// </summary>
public enum PrimitiveType
{
    TriangleList = 0,
    LineList,
    PointList,
    TriangleStrip,
    LineStrip,
    TriangleFan,
};

/// <summary>
/// Blending mode.
/// </summary>
public enum BlendMode
{
    Replace = 0,
    Add,
    Multiply,
    Alpha,
    AddAlpha,
    PremulAlpha,
    InvdestAlpha,
    Subtract,
    SubtractAlpha,
};

/// <summary>
/// Depth or stencil compare mode.
/// </summary>
public enum CompareMode
{
    Always = 0,
    Equal,
    NotEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    Max,
};

/// <summary>
/// Culling mode.
/// </summary>
public enum CullMode
{
    None = 0,
    CCW,
    CW,
    Max,
};

/// <summary>
/// Fill mode.
/// </summary>
public enum FillMode
{
    Solid = 0,
    Wireframe,
    Point,
};

/// <summary>
/// Stencil operation.
/// </summary>
public enum StencilOp
{
    Keep = 0,
    Zero,
    Ref,
    Incr,
    Decr,
};

/// <summary>
/// Stencil state
/// </summary>
public struct StencilState
{
    public StencilState()
    {

    }

    public bool StencilTest { get; set; } = false;

    public CompareMode StencilCompare { get; set; }  = CompareMode.Always;

    public int StencilRef { get; set; } = 0;

    public StencilOp PassOp { get; set; } = StencilOp.Keep;

    public uint StencilReadMask { get; set; } = 0;

    public uint StencilWriteMask {  get; set; } = 0;

    public static readonly StencilState Default = new();
};
```

### 8. 材质系统

```csharp
// 获取地形材质
List<Material> materials = Terrain.GetMaterials();
```

```csharp
// 设置材质属性
material.SetFloat("TintColor", 1.0f);
material.SetVector("TintColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
material.SetColor("TintColor", new Color(1.0f, 1.0f, 1.0f, 1.0f));

// 获取材质属性
float floatValue = material.GetFloat("TintColor");
Vector4 vectorValue = material.GetVector("TintColor");
Color colorValue = material.GetColor("TintColor");
```

```csharp
// 设置材质模板
StencilState stencilState = new();
material.SetStencilState(stencilState);

stencilState = material.GetStencilState();
```

```csharp
// 设置材质裁剪模式
material.SetCullMode(cullMode);
// 设置材质填充模式
material.SetFillMode(fillMode);
// 设置材质渲染优先级（范围0~256，数字越小渲染越靠前）
material.SetRenderOrder(renderOrder);

// 获取材质裁剪模式
CullMode cullMode = material.GetCullMode();
// 获取材质填充模式
FillMode fillMode = material.GetFillMode();
// 获取材质渲染优先级
uint renderOrder = material.GetRenderOrder();
```

```csharp
// 获取Shader
Shader shader = Shader.Find("PBR_PackedNormal/DefaultMetallicRoughness");

// 设置shader
material.shader = shader;
```

```csharp
// 设置材质pass enabled
material.SetShaderPassEnabled("base", false);
// 设置材质pass深度写
material.SetShaderPassDepthWrite("base", false);
// 设置材质pass颜色写
material.SetShaderPassColorWrite("base", false);
```

内置了如下这些Pass：
- base：基础pass
- alpha：半透pass
- litbase：光照pass（包含平行光、ClusterLight等计算）
- litalpha：班头光照pass（包含平行光、ClusterLight等计算
- shadow：实时投影pass
- planershadow：平面阴影pass
- xray：XRay
- outstroke：外描边pass
- innerstroke：内描边pass
- depth：深度pass


### 9. 创建自定义Mesh

```csharp
/// <summary>
/// 创建自定义Mesh
/// </summary>
/// <param name="vertexArray">顶点数组(Vector3)</param>
/// <param name="indexArray">索引数组(uint)</param>
/// <param name="primitiveType" type="PrimitiveType">渲染图元类型</param>
/// <returns></returns>
Mesh mesh = Mesh.CreateCustomMesh(verts, indies, primitiveType);
```

### 10. 网格组件系统

```csharp
Mesh mesh;
Material material;

Node node = actor.GetNode();
StaticMeshComponent comp = node.GetComponent<StaticMeshComponent>();
// 设置Mesh
comp.SetMesh(mesh);
// 设置材质
comp.SetMaterial(material);
```

## 游戏开发模式

### 1. 基础游戏类结构
```csharp
#if CLIENT
using System;
using System.Collections.Generic;
using GameCorePhysics.Actor;
using EngineInterface.Urho3DInterface;

public class MyGame : IGameClass
{
    private GameCore.SceneSystem.Scene scene;
    private List<PhysicsActor> gameObjects;
    
    public MyGame()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        scene = GameCore.SceneSystem.Scene.Get(GameEntry.PhysicsGameData.Scene.PhysicsScene);
        gameObjects = new List<PhysicsActor>();
    }

    static MyGame myGame = null;

    // 游戏开始时调用
    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += () =>
        {
            myGame = new MyGame();
        };
    }
}
#endif
```

### 2. 碰撞检测最佳实践

#### 为不同游戏对象创建专门的碰撞组件
```csharp
// 收集物品组件
public class CollectibleComponent : ScriptComponent
{
    private MyGame gameRef;
    
    public CollectibleComponent(MyGame game)
    {
        gameRef = game;
    }
    
    public override void OnTriggerEnter(Node node)
    {
        gameRef.CollectItem();
        Console.WriteLine("🪙 收集到道具!");
    }
}

// 敌人碰撞组件  
public class EnemyComponent : ScriptComponent
{
    public override void OnTriggerEnter(Node node)
    {
        Console.WriteLine("💥 敌人被击败!");
    }
}

// 胜利触发器
public class VictoryTrigger : ScriptComponent
{
    private MyGame gameRef;
    
    public VictoryTrigger(MyGame game)
    {
        gameRef = game;
    }
    
    public override void OnTriggerEnter(Node node)
    {
        gameRef.CompleteLevel();
        Console.WriteLine("🏁 关卡完成!");
    }
}
```

## 注意事项

### 1. 命名空间导入
必须导入的命名空间：
```csharp
using System;
using System.Collections.Generic;
using GameCorePhysics.Actor;
using EngineInterface.Urho3DInterface;
```

### 2. 物理对象生命周期
- 创建物理对象后，通过 `GetNode()` 获取引擎节点
- 脚本组件通过 `AddComponent` 方法添加到节点
- 避免在组件构造函数中进行复杂初始化，使用 `OnStart()` 方法

### 3. 性能考虑
- 避免在 `OnUpdate()` 中进行耗时操作
- 使用碰撞检测而非距离计算来检测交互
- 合理管理游戏对象列表，及时移除不需要的对象


## 常见错误避免

### ❌ 错误做法
```csharp
Vector3 direction = (target - source).normalized; // normalized不存在！
RigidBody rb = node.CreateComponent<RigidBody>(); // 不要手动创建！
Vector3 pos = GetNode().position; // 组件中用node属性！
```

### ✅ 正确做法
```csharp
Vector3 direction = Vector3.Normalize(target - source);
RigidBody rb = node.GetComponent<RigidBody>();
Vector3 pos = node.position;
```
