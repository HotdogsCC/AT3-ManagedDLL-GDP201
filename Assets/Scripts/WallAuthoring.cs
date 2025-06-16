using UnityEngine;
using Unity.Entities;
using MyDLL;

public class WallAuthoring : MonoBehaviour
{
    // extents of the wall
    public Transform edge1;
    public Transform edge2;
}

class WallBaker : Baker<WallAuthoring>
{
    public override void Bake(WallAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new ECSWall
        {
            position = authoring.transform.position,
            edge1 = authoring.edge1.position,
            edge2 = authoring.edge2.position
        });

    }
}