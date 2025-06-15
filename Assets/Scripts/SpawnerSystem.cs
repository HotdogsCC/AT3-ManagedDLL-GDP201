using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System;
using MyDLL;

[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    //broadcasts an event for the UIMonoBehaviour to recieve 
    public static Action OnSpawnEntity;

    public void OnCreate(ref SystemState state) {}
    
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
            Ecb = ecb
        }.ScheduleParallel();




        //single thread

        // Queries for all Spawner components. Uses RefRW because this system wants
        // to read from and write to the component. If the system only needed read-only
        // access, it would use RefRO instead.

        //foreach (RefRW<Spawner> spawner in SystemAPI.Query<RefRW<Spawner>>())
        //{
        //    ProcessSpawner(ref state, spawner);
        //}
    }

    private void ProcessSpawner(ref SystemState state, RefRW<Spawner> spawner)
    {
        //if the next spawn time has passed
        if (spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime)
        {
            //spawns a new entity and positions it at the spawner
            Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.meleePrefab);
            
            //LocalTransform.FromPosition() returns a Transform inited with the given pos
            state.EntityManager.SetComponentData(
                newEntity, 
                LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition));
            
            //resets the next spawn time
            spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
        }
    }

    public partial struct ProcessSpawnerJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double ElapsedTime;
        
        // IJobEntity generates a component data query based on the parameters of its `Execute` method.
        // This example queries for all Spawner components and uses `ref` to specify that the operation
        // requires read and write access. Unity processes `Execute` for each entity that matches the
        // component data query.
        private void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
        {
            // if the next spawn time has passed
            if (spawner.NextSpawnTime < ElapsedTime)
            {
                // spawns a new entity
                Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.meleePrefab);
                
                // sets its position to be at the spawner
                Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(spawner.SpawnPosition));
                
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
