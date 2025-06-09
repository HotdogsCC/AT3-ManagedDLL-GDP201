using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public struct UI : IComponentData
{
    public static uint agentsSpawned;
    public static Dictionary<int, float3> agentPositions = new Dictionary<int, float3>();

}