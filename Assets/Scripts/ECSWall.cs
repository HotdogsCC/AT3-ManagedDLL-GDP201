using Unity.Entities;
using Unity.Mathematics;

public struct ECSWall : IComponentData
{
    //where the wall is
    public float3 position;
    
    //the target positions for agents to go
    //the left and right of the wall
    public float3 edge1;
    public float3 edge2;
    
}