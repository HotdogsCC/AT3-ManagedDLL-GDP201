using Unity.Entities;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public partial struct UISystem : ISystem
{
    public void OnCreate(ref SystemState state) {}
    
    public void OnDestroy(ref SystemState state) {}

    public void OnUpdate(ref SystemState state)
    {
        uint spawned = 0;
        //get spawned
        foreach (var spawner in SystemAPI.Query<RefRO<Spawner>>())
        {
            spawned += spawner.ValueRO.spawnCount;
        }

        UI.agentsSpawned = spawned;
    }

}