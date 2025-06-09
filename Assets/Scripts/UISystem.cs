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
        //get spawners
        foreach (var spawner in SystemAPI.Query<RefRO<Spawner>>())
        {
            spawned += spawner.ValueRO.spawnCount;
        }
        
        UI.agentsSpawned = spawned;
        
        //get agent positions
        foreach (var (agent, entity) in SystemAPI.Query<RefRO<Movement>>().WithEntityAccess())
        {
            UI.agentPositions[entity.Index] = agent.ValueRO.currentPosition;
        }
        
    }

}