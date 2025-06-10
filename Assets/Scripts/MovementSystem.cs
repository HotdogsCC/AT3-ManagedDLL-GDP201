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
            //Debug.Log(transform.Position);
            //get colliders
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
            
            physicsWorld.OverlapSphere(transform.Position, 0.25f, ref hits, new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.Enemy,
                CollidesWith = (uint)CollisionLayer.Enemy

            });
            
            //resolve collisions
            foreach (var enemy in hits)
            {
                //dont do anything is this is us
                if (enemy.Entity == thisEntity)
                {
                    continue;
                }

                //resolve the collision
                float pushDistance = 0.25f - math.distance(transform.Position, enemy.Position);
                float3 pushVector = math.normalizesafe(transform.Position - enemy.Position) * pushDistance;
                
                //add the push onto the agent
                transform.Position += pushVector;


            }
            
            
                
                
            //do the behaviour based on its current state
            switch (movement.currentState)
            {
                
                case NPCState.HEADING_TO_TARGET:
                    //check if current target is 'null'
                    if (movement.TargetPosition.x >= 9998.0f)
                    {
                        //set the current target to be the position of the enemy base
                        movement.TargetPosition = SpawnerLocations.GetMyEnemyBasePosition(movement.team);
                        
                    }
                    
                    //see if a target is in range
                    foreach (var pair in UI.agentPositions)
                    {
                        //if it is within range
                        float3 otherPosition = pair.Value;
                        if (math.distance(transform.Position, otherPosition) < movement.enemyRangeDetection)
                        {
                            movement.TargetPosition = pair.Value;
                            break;
                        }
                    }
                    break;
                case NPCState.MELEE_ATTACK:
                    break;
                case NPCState.RANGE_ATTACK:
                    break;
            }

            movement.TargetPosition = new float3(0, 1, 20); 
            
            //get the vector that points from its position to the target
            float3 direction = movement.TargetPosition - transform.Position;
            
            //normalize it
            float3 normalized = math.normalizesafe(direction);
            
            //move toward it
            transform.Position += normalized * movement.MoveSpeed * DeltaTime;
            
        }
    }
}
