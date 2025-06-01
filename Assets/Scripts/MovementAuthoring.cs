using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

class MovementAuthoring : MonoBehaviour
{
    public float3 targetPosition;
    public float moveSpeed;
}

class MovementBaker : Baker<MovementAuthoring>
{
    public override void Bake(MovementAuthoring authoring)
    {
        // By default, each authoring GameObject turns into an Entity.
        // Given a GameObject (or authoring component), GetEntity looks up the resulting Entity.
        
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Movement
        {
            MoveSpeed = authoring.moveSpeed,
            TargetPosition = authoring.targetPosition
            
        });
    }
}
