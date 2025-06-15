using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using MyDLL;

class UIAuthoring : MonoBehaviour
{
    
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