using Unity.Entities;
using Unity.Mathematics;

public static class SpawnerLocations
{
    public static float3 team1BasePosition = new float3(0, 1, 0);
    public static float3 team2BasePosition = new float3(0, 1, 45);
    public static float3 team3BasePosition = new float3(-23, 1, 23);
    public static float3 team4BasePosition = new float3(23, 1, 23);

    //team 1 hates team 2, team 3 hates team 4
    //takes in the team and returns the position of its rival
    public static float3 GetMyEnemyBasePosition(Team myTeam)
    {
        switch (myTeam)
        {
            case Team.TEAM_1:
                return team2BasePosition;
            case Team.TEAM_2:
                return team1BasePosition;
            case Team.TEAM_3:
                return team4BasePosition;
            case Team.TEAM_4:
                return team3BasePosition;
        }
        
        //'null' float if the above fails
        return new float3(9999);
    }
}
public struct Spawner : IComponentData
{
    //the number of entities this spawner has spawned
    public uint spawnCount;
    
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
    //The material to apply to the team
    public float4 materialColour;
    //The amount of health this spawner has
    public int currentHealth;
}
