#if CLIENT

using System.Numerics;

namespace GameEntry;

// 玩家类
public class MyPlayer
{
    public Vector2 Position { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public int Level { get; set; }
    public float Experience { get; set; }
    public float ExperienceToNextLevel { get; set; }
    public float MoveSpeed { get; set; }
    public float AttackDamage { get; set; }
    public float AttackSpeed { get; set; }
    public float ExperienceMultiplier { get; set; }
    public bool ShouldLevelUp { get; private set; }

    private float attackTimer;
    private VampireSurvivors? gameInstance;

    public MyPlayer(float x, float y)
    {
        Position = new Vector2(x, y);
        Reset();
    }

    public void Reset()
    {
        Health = 100f;
        MaxHealth = 100f;
        Level = 1;
        Experience = 0f;
        ExperienceToNextLevel = 100f;
        MoveSpeed = 200f;
        AttackDamage = 25f;
        AttackSpeed = 1f;
        ExperienceMultiplier = 1f;
        attackTimer = 0f;
        ShouldLevelUp = false;
    }

    public void SetGameInstance(VampireSurvivors game)
    {
        gameInstance = game;
    }

    public void Move(float inputX, float inputY, float deltaTime)
    {
        if (inputX != 0 || inputY != 0)
        {
            // 标准化移动方向
            float length = MathF.Sqrt(inputX * inputX + inputY * inputY);
            if (length > 0)
            {
                inputX /= length;
                inputY /= length;
            }

            Position = new Vector2(
                Position.X + inputX * MoveSpeed * deltaTime,
                Position.Y + inputY * MoveSpeed * deltaTime
            );
        }
    }

    public void Update(float deltaTime)
    {
        // 更新攻击计时器
        attackTimer += deltaTime;

        // 自动攻击
        if (attackTimer >= 1f / AttackSpeed)
        {
            attackTimer = 0f;
            Attack();
        }
    }

    private void Attack()
    {
        // 发射投射物攻击最近的敌人
        // 暂时向四个方向发射
        var directions = new Vector2[]
        {
            new(1, 0),   // 右
            new(-1, 0),  // 左
            new(0, 1),   // 下
            new(0, -1)   // 上
        };

        foreach (var direction in directions)
        {
            var projectile = new Projectile(Position, direction, AttackDamage, 300f, 2f);
            gameInstance?.AddProjectile(projectile);
        }
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
    }

    public void AddExperience(float exp)
    {
        Experience += exp * ExperienceMultiplier;

        while (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel = Level * 100f; // 每级需要更多经验
            ShouldLevelUp = true;
        }
    }

    public void CompleteUpgrade()
    {
        ShouldLevelUp = false;
    }
}

// 敌人基类
public abstract class Enemy
{
    public Vector2 Position { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public float MoveSpeed { get; set; }
    public float Damage { get; set; }
    public int ExperienceValue { get; set; }

    protected Enemy(float x, float y, float health, float moveSpeed, float damage, int expValue)
    {
        Position = new Vector2(x, y);
        Health = health;
        MaxHealth = health;
        MoveSpeed = moveSpeed;
        Damage = damage;
        ExperienceValue = expValue;
    }

    public virtual void Update(float deltaTime, Vector2 playerPosition)
    {
        // 向玩家移动
        var direction = (playerPosition - Position).GetNormalized();
        Position = Position + direction * MoveSpeed * deltaTime;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
    }
}

// 基础敌人
public class BasicEnemy : Enemy
{
    public BasicEnemy(float x, float y) 
        : base(x, y, health: 50f, moveSpeed: 80f, damage: 15f, expValue: 10)
    {
    }
}

// 快速敌人
public class FastEnemy : Enemy
{
    public FastEnemy(float x, float y) 
        : base(x, y, health: 30f, moveSpeed: 150f, damage: 10f, expValue: 15)
    {
    }
}

// 强壮敌人
public class TankEnemy : Enemy
{
    public TankEnemy(float x, float y) 
        : base(x, y, health: 150f, moveSpeed: 40f, damage: 30f, expValue: 25)
    {
    }
}

// 投射物类
public class Projectile
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Damage { get; set; }
    public float LifeTime { get; set; }

    public Projectile(Vector2 startPosition, Vector2 direction, float damage, float speed, float lifeTime)
    {
        Position = startPosition;
        Velocity = direction.GetNormalized() * speed;
        Damage = damage;
        LifeTime = lifeTime;
    }

    public void Update(float deltaTime)
    {
        Position = Position + Velocity * deltaTime;
        LifeTime -= deltaTime;
    }
}

// 经验球类
public class ExperienceOrb
{
    public Vector2 Position { get; set; }
    public int ExperienceValue { get; set; }
    public float LifeTime { get; set; }
    public float CollectionSpeed { get; set; }

    public ExperienceOrb(Vector2 position, int expValue)
    {
        Position = position;
        ExperienceValue = expValue;
        LifeTime = 30f; // 30秒后消失
        CollectionSpeed = 100f;
    }

    public void Update(float deltaTime, Vector2 playerPosition)
    {
        LifeTime -= deltaTime;

        // 如果玩家靠近，吸引经验球
        float distanceToPlayer = Vector2.Distance(Position, playerPosition);
        if (distanceToPlayer < 100f)
        {
            var direction = (playerPosition - Position).GetNormalized();
            Position = Position + direction * CollectionSpeed * deltaTime;
        }
    }
}

// 武器基类
public abstract class Weapon
{
    public string Name { get; set; }
    public float Damage { get; set; }
    public float AttackSpeed { get; set; }
    public float Range { get; set; }
    public int Level { get; set; }

    protected Weapon(string name, float damage, float attackSpeed, float range)
    {
        Name = name;
        Damage = damage;
        AttackSpeed = attackSpeed;
        Range = range;
        Level = 1;
    }

    public abstract void Attack(Vector2 playerPosition, VampireSurvivors gameInstance);
    
    public virtual void Upgrade()
    {
        Level++;
        Damage *= 1.2f;
        AttackSpeed *= 1.1f;
    }
}

// 基础武器 - 魔法导弹
public class MagicMissile : Weapon
{
    public MagicMissile() : base("Magic Missile", 25f, 2f, 400f)
    {
    }

    public override void Attack(Vector2 playerPosition, VampireSurvivors gameInstance)
    {
        // 向最近的敌人发射导弹
        var directions = new Vector2[]
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
            new(0.707f, 0.707f), new(-0.707f, 0.707f), 
            new(0.707f, -0.707f), new(-0.707f, -0.707f)
        };

        int projectileCount = Math.Min(Level + 1, directions.Length);
        for (int i = 0; i < projectileCount; i++)
        {
            var projectile = new Projectile(playerPosition, directions[i], Damage, 300f, 3f);
            gameInstance.AddProjectile(projectile);
        }
    }
}

#else
namespace GameEntry.VampireSurvivor;

// 空的类定义用于服务器端编译
public struct Vector2(float x, float y)
{
    public float X { get; set; } = x; public float Y { get; set; } = y;
}
public class MyPlayer { }
public class Enemy { }
public class BasicEnemy : Enemy { }
public class Projectile { }
public class ExperienceOrb { }
public class Weapon { }
public class MagicMissile : Weapon { }

#endif