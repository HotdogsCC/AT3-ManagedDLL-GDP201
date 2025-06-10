using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct MovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }
    
    public void OnDestroy(ref SystemState state) {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //create a new instance of the job, assigns data, schedules in parallel
        new ProcessMovementJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>()
            
        }.ScheduleParallel();
        
        
    }
    
    
    public partial struct ProcessMovementJob : IJobEntity
    {
        public float DeltaTime;
        [ReadOnly] public PhysicsWorldSingleton physicsWorld;
        
        // IJobEntity generates a component data query based on the parameters of its `Execute` method.
        // 'ref' specifies read and write access

        private void Execute([ChunkIndexInQuery] int chunkIndex, Entity thisEntity, ref LocalTransform transform, ref Movement movement)
        {
            //used for storing all agents in collision zone
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
            
            //runs a sphere collider to see what is in range
            physicsWorld.OverlapSphere(transform.Position, 0.25f, ref hits, new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.Enemy,
                CollidesWith = (uint)CollisionLayer.Enemy

            });
            
            //resolve collisions
            foreach (var enemy in hits)
            {
                //don't do anything is this is us
                if (enemy.Entity == thisEntity)
                {
                    continue;
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
            
            
                
                
            //do the behaviour based on its current state
            switch (movement.currentState)
            {
                case NPCState.HEADING_TO_TARGET:
                    //set the current target to be the position of the enemy base
                    movement.TargetPosition = SpawnerLocations.GetMyEnemyBasePosition(movement.team);
                    
                    //reuse the hits list from before (saving data!)
                    hits.Clear();
                    
                    //see if a target is in range
                    physicsWorld.OverlapSphere(transform.Position, movement.enemyRangeDetection, ref hits, new CollisionFilter
                    {
                        BelongsTo = (uint)CollisionLayer.Enemy,
                        CollidesWith = (uint)CollisionLayer.Enemy

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
                        movement.TargetPosition = enemy.Position;
                        
                        //if we are an archer
                        if (movement.npcType == NPCType.RANGED)
                        {
                            //then start firing
                            movement.currentState = NPCState.RANGE_ATTACK;
                            
                            //we're done with this switch 
                            break;
                        }
                        
                        //if we are not an archer, we need to 
                    }
                    break;
                case NPCState.MELEE_ATTACK:
                    break;
                case NPCState.RANGE_ATTACK:
                    break;
            }

            //movement.TargetPosition = new float3(0, 1, 20); 
            
            //get the vector that points from its position to the target
            float3 direction = movement.TargetPosition - transform.Position;
            
            //normalize it
            float3 normalized = math.normalizesafe(direction);
            
            //move toward it
            transform.Position += normalized * movement.MoveSpeed * DeltaTime;
            
        }
    }
}
