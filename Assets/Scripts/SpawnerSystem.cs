using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System;
using MyDLL;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    //broadcasts an event for the UIMonoBehaviour to recieve 
    public static Action OnSpawnEntity;

    public Random randomNumber;

    public void OnCreate(ref SystemState state) 
    {
        randomNumber = new Random(1);
    }
    
    public void OnDestroy(ref SystemState state) {}

    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = 
            SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      
        EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
        
        //create a new instance of the job, assigns data, schedules in parallel
        new ProcessSpawnerJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            Ecb = ecb,
            randNumber = randomNumber.NextInt(0, 3)
        }.ScheduleParallel();

    }


    public partial struct ProcessSpawnerJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double ElapsedTime;
        public int randNumber;
        
        // IJobEntity generates a component data query based on the parameters of its `Execute` method.
        // This example queries for all Spawner components and uses `ref` to specify that the operation
        // requires read and write access. Unity processes `Execute` for each entity that matches the
        // component data query.
        private void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
        {
            // if the next spawn time has passed
            if (spawner.NextSpawnTime < ElapsedTime)
            {
                Entity newEntity;
                //pick a random enemy to spawn
                switch (randNumber)
                {
                    case 1:
                        // spawns a new entity
                        newEntity = Ecb.Instantiate(chunkIndex, spawner.tankPrefab);
                        break;

                    case 2:
                        // spawns a new entity
                        newEntity = Ecb.Instantiate(chunkIndex, spawner.rangedPrefab);
                        break;

                    default:
                        // spawns a new entity
                        newEntity = Ecb.Instantiate(chunkIndex, spawner.meleePrefab);
                        break;
                }
                
                // sets its position to be at the spawner
                Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPositionRotationScale(spawner.SpawnPosition, quaternion.identity, 0.25f));
                
                
                // add a dummy decrementer to it so it can call it later
                Ecb.AddComponent(chunkIndex, newEntity, typeof(AgentDecrementer));
                
                // increment this spawner's count
                spawner.spawnCount++;
                
                // set the colour to be of the spawner
                
                //Ecb.SetComponent(chunkIndex, newEntity, Movement.SetColour(spawner.materialColour));
                
                //reset the spawn time
                spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
                
                // invoke the spawn event
                OnSpawnEntity?.Invoke();
            }
        }
    }
}
