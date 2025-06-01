using Unity.Entities;
using Unity.Mathematics;

public struct Movement : IComponentData
{
    public float3 TargetPosition;
    public float MoveSpeed;
}
