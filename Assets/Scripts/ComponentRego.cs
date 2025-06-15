using Unity.Entities;
using MyDLL;

[assembly: RegisterGenericComponentType(typeof(MyDLL.Movement))]
[assembly: RegisterGenericComponentType(typeof(MyDLL.AgentDecrementer))]