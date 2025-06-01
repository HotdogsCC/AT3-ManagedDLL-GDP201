using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct MovementSystem : ISystem
{
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
        new ProcessMovementJob
        {
            Ecb = ecb,
            DeltaTime = SystemAPI.Time.DeltaTime
            
        }.ScheduleParallel();
    }

    public partial struct ProcessMovementJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float DeltaTime;
        
        // IJobEntity generates a component data query based on the parameters of its `Execute` method.
        // 'ref' specifies read and write access

        private void Execute([ChunkIndexInQuery] int chunkIndex, ref LocalTransform transform, Movement movement)
        {
            
            transform.Position += movement.TargetPosition * movement.MoveSpeed * DeltaTime;
        }
    }
}
