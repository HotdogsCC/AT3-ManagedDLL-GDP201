using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

class SpawnerAuthoring : MonoBehaviour
{
    public GameObject TankPrefab;
    public GameObject RangedPrefab;
    public GameObject MeleePrefab;
    public Team team;
    public float SpawnRate;
    public Color materialColour;
    public int startingHealth;
}

class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);
        float4 tempColour;
        
        //create a float3 of its pos
        float3 position = new float3(
            authoring.transform.position.x,
            authoring.transform.position.y,
            authoring.transform.position.z
        );
        
        //set it in a static class for easy access
        switch (authoring.team)
        {
            case Team.TEAM_1:
                SpawnerLocations.team1BasePosition = position;
                break;
            case Team.TEAM_2:
                SpawnerLocations.team2BasePosition = position;
                break;
            case Team.TEAM_3:
                SpawnerLocations.team3BasePosition = position;
                break;
            case Team.TEAM_4:
                SpawnerLocations.team4BasePosition = position;
                break;
        }
        
        AddComponent(entity, new Spawner
        {
            // By default, each authoring GameObject turns into an Entity.
            // Given a GameObject (or authoring component), GetEntity looks up the resulting Entity.
            SpawnPosition = authoring.transform.position,
            NextSpawnTime = 0.0f,
            SpawnRate = authoring.SpawnRate,
            team = authoring.team,
            tankPrefab = GetEntity(authoring.TankPrefab, TransformUsageFlags.Dynamic),
            rangedPrefab = GetEntity(authoring.RangedPrefab, TransformUsageFlags.Dynamic),
            meleePrefab = GetEntity(authoring.MeleePrefab, TransformUsageFlags.Dynamic),
            materialColour = new float4(
                authoring.materialColour.r,
                authoring.materialColour.g,
                authoring.materialColour.b,
                authoring.materialColour.a
                ),
            currentHealth = authoring.startingHealth
        });
    }
}
