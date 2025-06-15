using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using MyDLL;

class MovementAuthoring : MonoBehaviour
{
    // type of enemy (ranged, melee, tank)
    public NPCType npcType;
    //speed to move towards the target
    public float moveSpeed;
    //how much damage this does when it attacks
    public float damage;
    //radius of the circle collision
    public float enemyRangeDetection;
    // the team this agent is on
    public Team team;
    // starting health
    public float maximumHealth;
    // time between attacks
    public float coolDownTime; 
    // projectile prefab
    public GameObject projectile;
    
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
            npcType = authoring.npcType,
            currentState = NPCState.HEADING_TO_TARGET,
            MoveSpeed = authoring.moveSpeed,
            damage = authoring.damage,
            enemyRangeDetection = authoring.enemyRangeDetection,
            team = authoring.team,
            maximumHealth = authoring.maximumHealth,
            currentHealth = authoring.maximumHealth,
            coolDownTime = authoring.coolDownTime,
            coolDownTimer = 0.0f,
            projectile = GetEntity(authoring.projectile, TransformUsageFlags.None),
            TargetPosition = new float3(9999) // 9999 represents null
            
        });
    }
}
