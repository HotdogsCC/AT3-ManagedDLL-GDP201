using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

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
            jobMovementLookup = SystemAPI.GetComponentLookup<Movement>(true)
            
        }.ScheduleParallel();
        
        
    }
    
    [WithAll(typeof(Movement))]
    public partial struct ProcessMovementJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public PhysicsWorldSingleton physicsWorld;
        [ReadOnly] public ComponentLookup<Movement> jobMovementLookup;
        
        // IJobEntity generates a component data query based on the parameters of its `Execute` method.
        // 'ref' specifies read and write access

        
        private void Execute([ChunkIndexInQuery] int chunkIndex, Entity thisEntity, ref LocalTransform transform)
        {
            ResolveCollisions(chunkIndex, thisEntity, ref transform);
            
            //do the behaviour based on its current state
            switch (jobMovementLookup[thisEntity].currentState)
            {
                case NPCState.HEADING_TO_TARGET:
                    HeadingToTarget(chunkIndex, thisEntity, ref transform);
                    break;
                case NPCState.MELEE_ATTACK:
                    DoMeleeAttack(chunkIndex, thisEntity, ref transform);
                    break;
                case NPCState.RANGE_ATTACK:
                    break;
            }
            
        }

        private void ResolveCollisions([ChunkIndexInQuery] int chunkIndex, Entity thisEntity, ref LocalTransform transform)
        {
            //used for storing all agents in collision zone
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
            
            //runs a sphere collider to see what is being collided with
            physicsWorld.OverlapSphere(transform.Position, 0.25f, ref hits, new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.AllEnemies,
                CollidesWith = (uint)CollisionLayer.AllEnemies

            });
            
            //resolve collisions
            foreach (var enemy in hits)
            {
                //don't do anything is this is us
                if (enemy.Entity == thisEntity)
                {
                    continue;
                }
                
                //if this enemy is on a different team, engage in warfare
                if (jobMovementLookup[thisEntity].team != jobMovementLookup.GetRefRO(enemy.Entity).ValueRO.team)
                {
                    Ecb.SetComponent(chunkIndex, thisEntity, Movement.SetCurrentState(
                        jobMovementLookup[thisEntity],
                        NPCState.MELEE_ATTACK));
                }

                //resolve the collision
                float pushDistance = 0.25f - math.distance(transform.Position, enemy.Position);
                float3 pushVector = math.normalizesafe(transform.Position - enemy.Position) * pushDistance;
                
                //add the push onto the agent
                transform.Position += pushVector;
                
                //after being pushed out of one, don't deal with the others
                break;

                //the scope of this project is too small to deal with multi collisions
            }

            //we're done with the list, so free up the memory
            hits.Dispose();

        }

        private void HeadingToTarget([ChunkIndexInQuery] int chunkIndex, Entity thisEntity, ref LocalTransform transform)
        {
            //set the current target to be the position of the enemy base
            Ecb.SetComponent(chunkIndex, thisEntity, Movement.NewTargetPosition(
                jobMovementLookup[thisEntity],
                SpawnerLocations.GetMyEnemyBasePosition(jobMovementLookup[thisEntity].team)));
                    
            //used for storing all agents in collision zone
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
                    
            //see if a target is in range
            physicsWorld.OverlapSphere(transform.Position, jobMovementLookup[thisEntity].enemyRangeDetection, ref hits, new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.AllEnemies,
                CollidesWith = GetCollisionLayer.Please(jobMovementLookup[thisEntity].team)

            });
                    
            //pick the first target in range
            foreach (var enemy in hits)
            {
                //don't do anything is this is us
                if (enemy.Entity == thisEntity)
                {
                    continue;
                }
                
                //otherwise, this is an enemy. get ready for battle!
                Ecb.SetComponent(chunkIndex, thisEntity, Movement.NewTargetPosition(
                    jobMovementLookup[thisEntity],
                    enemy.Position));
                        
                //if we are an archer
                if (jobMovementLookup[thisEntity].npcType == NPCType.RANGED)
                {
                    //then start firing
                    Ecb.SetComponent(chunkIndex, thisEntity, Movement.SetCurrentState(
                        jobMovementLookup[thisEntity],
                        NPCState.RANGE_ATTACK));

                }
                        
            }
            
            //get the vector that points from its position to the target
            float3 direction = jobMovementLookup[thisEntity].TargetPosition - transform.Position;
            
            //normalize it
            float3 normalized = math.normalizesafe(direction);
            
            //move toward it
            transform.Position += normalized * jobMovementLookup[thisEntity].MoveSpeed * DeltaTime;
        }

        private void DoMeleeAttack([ChunkIndexInQuery] int chunkIndex, Entity thisEntity, ref LocalTransform transform)
        {
            float curCoolDown = jobMovementLookup[thisEntity].coolDownTimer - DeltaTime;
            if(curCoolDown <= 0)
            {

            }
        }
    }
}
