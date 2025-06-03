using Unity.Entities;
using Unity.Mathematics;

public struct Spawner : IComponentData
{
    //The prefab of an agent to spawn 
    //TEMP FOR TESTING
    public Entity Prefab; 
    //Where to spawn the agent (this is set in the Baker)
    public float3 SpawnPosition;
    //How much time (in seconds) until it can spawn again
    public float NextSpawnTime;
    //The amount of seconds between spawns
    public float SpawnRate;
    //The team type of this spawner
    public Team team;
    //The prefab for the tank
    public Entity tankPrefab;
    //The prefab for the ranged agent
    public Entity rangedPrefab;
    //The prefab for the melee agent
    public Entity meleePrefab;
}
