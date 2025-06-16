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
                    DoRangedAttack(chunkIndex, thisEntity, ref transform);
                    break;
            }
            
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

            bool isThereAnEnemy = false;
            //if there are targets, set target as first one
            foreach (var enemy in hits)
            {
                //don't do anything is this is us
                if (enemy.Entity == thisEntity)
                {
                    continue;
                }
                
                //otherwise, this is an enemy. get ready for battle!
                isThereAnEnemy = true;
                
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

                    hits.Dispose();
                    return;

                }
                        
            }

            hits.Dispose();

            // if there isn't an enemy, check for walls in the way
            if (!isThereAnEnemy)
            {
                //sets up params for the raycast
                RaycastInput raycastInput = default;
                raycastInput.Start = transform.Position;
                raycastInput.End = GetRaycastEndPosition(thisEntity, ref transform);
                raycastInput.Filter = new CollisionFilter
                {
                    BelongsTo = (uint)CollisionLayer.AllEnemies,
                    CollidesWith = (uint)CollisionLayer.Wall

                };
                RaycastHit raycastHit = default;
                
                //is there a wall?
                if (physicsWorld.CastRay(raycastInput, out raycastHit))
                {
                    //figure out which edge is closer
                    float edge1DistanceSq = math.distancesq(jobWallLookup[raycastHit.Entity].edge1, transform.Position);
                    float edge2DistanceSq = math.distancesq(jobWallLookup[raycastHit.Entity].edge2, transform.Position);
                    
                    //is edge 1 closer?
                    if (edge1DistanceSq < edge2DistanceSq)
                    {
                        //set the target position to edge 1
                        Ecb.SetComponent(chunkIndex, thisEntity, Movement.NewTargetPosition(
                            jobMovementLookup[thisEntity],
                            jobWallLookup[raycastHit.Entity].edge1));
                    }
                    //edge 2 is closer
                    else
                    {
                        //set the target position to edge 1
                        Ecb.SetComponent(chunkIndex, thisEntity, Movement.NewTargetPosition(
                            jobMovementLookup[thisEntity],
                            
                            jobWallLookup[raycastHit.Entity].edge2));
                    }
                    
                    
                }
            }
            
            //move toward target
            
            //get the vector that points from its position to the target
            float3 direction = jobMovementLookup[thisEntity].TargetPosition - transform.Position;
            
            //normalize it
            float3 normalized = math.normalizesafe(direction);
            
            //move toward it
            transform.Position += normalized * jobMovementLookup[thisEntity].MoveSpeed * DeltaTime;
        }

        private float3 GetRaycastEndPosition(Entity thisEntity, ref LocalTransform transform)
        {
            float3 adder = new float3(0);
            switch (jobMovementLookup[thisEntity].team)
            {
                case Team.TEAM_1:
                    adder.z = jobMovementLookup[thisEntity].enemyRangeDetection;
                    break;
                case Team.TEAM_2:
                    adder.z = -jobMovementLookup[thisEntity].enemyRangeDetection;
                    break;
                case Team.TEAM_3:
                    adder.x = jobMovementLookup[thisEntity].enemyRangeDetection;
                    break;
                case Team.TEAM_4:
                    adder.x = -jobMovementLookup[thisEntity].enemyRangeDetection;
                    break;
            }

            return transform.Position + adder;
        }

        private void DoMeleeAttack([ChunkIndexInQuery] int chunkIndex, Entity thisEntity, ref LocalTransform transform)
        {
            float curCoolDown = jobMovementLookup[thisEntity].coolDownTimer - DeltaTime;
            if(curCoolDown <= 0)
            {
                //reset cooldown timer
                Ecb.SetComponent(chunkIndex, thisEntity, Movement.ResetCoolDown(
                    jobMovementLookup[thisEntity]));
            }
            else
            {
                //decrement the cooldown
                Ecb.SetComponent(chunkIndex, thisEntity, Movement.DecrementCoolDown(
                    jobMovementLookup[thisEntity],
                    DeltaTime));
            }
        }

        private void DoRangedAttack([ChunkIndexInQuery] int chunkIndex, Entity thisEntity, ref LocalTransform transform)
        {
            //sets up params for the raycast
            RaycastInput raycastInput = default;
            raycastInput.Start = transform.Position;
            raycastInput.End = jobMovementLookup[thisEntity].TargetPosition;
            raycastInput.Filter = new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.AllEnemies,
                CollidesWith = GetCollisionLayer.Please(jobMovementLookup[thisEntity].team)

            };
            RaycastHit raycastHit = default;
                
            //check there is an enemy at the target position
            if (physicsWorld.CastRay(raycastInput, out raycastHit))
            {
                //spawn the projectile
                Entity projectileInstance = Ecb.Instantiate(chunkIndex, jobMovementLookup[thisEntity].projectile);
                
                //set the projectile position to where we are
                Ecb.SetComponent(chunkIndex, projectileInstance, LocalTransform.FromPosition(transform.Position));

                Ecb.AddComponent(chunkIndex, projectileInstance, new Projectile
                {
                    targetPosition = raycastHit.Position,
                    speed = 10,
                    damage = 1,
                    aliveTime = 1,
                    team = jobMovementLookup[thisEntity].team
                });
            }
            
            //otherwise, go back to heading to the target
            else
            {
                Ecb.SetComponent(chunkIndex, thisEntity, Movement.SetCurrentState(
                    jobMovementLookup[thisEntity],
                    NPCState.HEADING_TO_TARGET));
            }
        }
    }
}
