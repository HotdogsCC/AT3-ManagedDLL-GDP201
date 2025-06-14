using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public struct UI : IComponentData
{
    public static uint agentsSpawned;

    public static uint agentsKilled = 0;
}