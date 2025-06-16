using Unity.Entities;
using MyDLL;

[assembly: RegisterGenericComponentType(typeof(MyDLL.Movement))]
[assembly: RegisterGenericComponentType(typeof(MyDLL.AgentDecrementer))]
[assembly: RegisterGenericComponentType(typeof(MyDLL.ECSWall))]
[assembly: RegisterGenericComponentType(typeof(MyDLL.Projectile))]