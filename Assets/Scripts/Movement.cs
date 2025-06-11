using System;
using Unity.Entities;
using Unity.Mathematics;

public struct Movement : IComponentData
{
    // type of enemy (ranged, melee, tank)
    public NPCType npcType;
    // position of the enemy base
    public float3 enemyBasePosition;
    //current state of state machine
    public NPCState currentState;
    //speed to move towards the target
    public float MoveSpeed;
    //how much damage this does when it attacks
    public float damage;
    //radius of the circle collision
    public float enemyRangeDetection;
    // the team this agent is on
    public Team team;
    // starting health
    public float maximumHealth;
    // current health
    public float currentHealth;
    // time between attacks
    public float coolDownTime; 
    // timer for attacks
    public float coolDownTimer; 
    // whether to avoid to the left or right
    public bool bShouldAvoidToLeft;
    // projectile prefab
    public Entity projectile;
    
    //the position of where the entity should travel toward
    public float3 TargetPosition;

    public static Movement NewTargetPosition(Movement currentMovement, float3 newTargetPosition)
    {
        Movement tempMovement = currentMovement;
        tempMovement.TargetPosition = newTargetPosition;
        return tempMovement;
    }
    
    public static Movement SetCurrentState(Movement currentMovement, NPCState newState)
    {
        Movement tempMovement = currentMovement;
        tempMovement.currentState = newState;
        return tempMovement;
    }


}
