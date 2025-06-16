using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using System;

[BurstCompile]
public partial struct ProjectileSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

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
        new ProcessProjectileJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            ecb = GetEntityCommandBuffer(ref state)

        }.ScheduleParallel();
    }

    public partial struct ProcessProjectileJob : IJobEntity
    {
        public float deltaTime;
        public EntityCommandBuffer.ParallelWriter ecb;

        private void Execute([ChunkIndexInQuery] int chunkIndex, ref LocalTransform transform, ref Projectile projectile, Entity thisEntity)
        {
            //decrement alive time
            projectile.aliveTime -= deltaTime;

            if(projectile.aliveTime <= 0)
            {
                //delete the projectile
                ecb.AddComponent(chunkIndex, thisEntity, typeof(Disabled));

                //and we're finished
                return;
            }

            //otherwise, move towards the target
            float3 currentPosition = transform.Position;
            float3 targetPosition = projectile.targetPosition;

            //get the vector from the current position to the target
            float3 currentToTarget = targetPosition - currentPosition;
            
            //normalise it
            float3 directionVector = math.normalizesafe(currentToTarget);

            //mulitply by move speed
            directionVector *= projectile.speed * deltaTime;

            //if the distance is smaller than the movement magnitude , then we will overshoot
            float distance = math.distancesq(currentPosition, targetPosition);
            float magnitude = math.pow(directionVector.x, 2) + math.pow(directionVector.y, 2) + math.pow(directionVector.z, 2);
            if(distance < magnitude)
            {
                //set the position to the target
                ecb.SetComponent(chunkIndex, thisEntity, LocalTransform.FromPosition(targetPosition));

                //then set its alive time to 0, so it is destroyed on the next frame
                projectile.aliveTime = 0;

                return;
            }

            //otherwise, move toward the target
            else
            {
                float3 newPos = currentPosition + directionVector;
                //set the position to the target
                ecb.SetComponent(chunkIndex, thisEntity, LocalTransform.FromPosition(newPos));

                return;
            }
        }
    }

}