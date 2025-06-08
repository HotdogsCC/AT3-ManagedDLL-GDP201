using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

class UIAuthoring : MonoBehaviour
{
    //public GameObject[] spawners;
}

class UIBaker : Baker<UIAuthoring>
{
    public override void Bake(UIAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new UI
        {
            
        });
    }
}