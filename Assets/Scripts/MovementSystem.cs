using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using MyDLL;

using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct MovementSystem : ISystem
{
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        
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
        //get the entity command buffer for main thread commands
        EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
        
        //create a new instance of the job, assigns data, schedules in parallel
        new ProcessMovementJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>(),
            Ecb = ecb,
            jobMovementLookup = SystemAPI.GetComponentLookup<Movement>(true),
            jobWallLookup = SystemAPI.GetComponentLookup<ECSWall>(true)
            
        }.ScheduleParallel();
        
        
    }
    
    [WithAll(typeof(Movement))]
    public partial struct ProcessMovementJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public PhysicsWorldSingleton physicsWorld;
        [ReadOnly] public ComponentLookup<Movement> jobMovementLookup;
        [ReadOnly] public ComponentLookup<ECSWall> jobWallLookup;
        
        // IJobEntity generates a component data query based on the parameters of its `Execute` method.
        // 'ref' specifies read and write access

        
        private void Execute([ChunkIndexInQuery] int chunkIndex, Entity thisEntity, ref LocalTransform transform)
        {
            //collision resolution written in MyDLL
            MyPhysics myPhysics = new MyPhysics
            {
                Ecb = Ecb,
                physicsWorld = physicsWorld,
                jobMovementLookup = jobMovementLookup
            };

            myPhysics.ResolveCollisions(chunkIndex, thisEntity, ref transform);

            MyStateMachine myStateMachine = new MyStateMachine
            {
                Ecb = Ecb,
                physicsWorld = physicsWorld,
                jobMovementLookup = jobMovementLookup,
                jobWallLookup = jobWallLookup
            };
            
            //do the behaviour based on its current state
            switch (jobMovementLookup[thisEntity].currentState)
            {
                case NPCState.HEADING_TO_TARGET:
                    myStateMachine.HeadingToTarget(chunkIndex, thisEntity, ref transform, DeltaTime);
                    break;
                case NPCState.RANGE_ATTACK:
                    myStateMachine.DoRangedAttack(chunkIndex, thisEntity, ref transform, DeltaTime);
                    break;
            }
            
        }
    }
}
