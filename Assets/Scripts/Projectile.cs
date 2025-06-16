using MyDLL;
using Unity.Entities;
using Unity.Mathematics;

public struct Projectile : IComponentData
{
    public float3 targetPosition;
    public float speed;
    public float damage;
    public float aliveTime;
    public Team team;
}
