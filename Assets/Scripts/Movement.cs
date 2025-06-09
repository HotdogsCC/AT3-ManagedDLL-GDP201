using System;
using Unity.Entities;
using Unity.Mathematics;

public struct Movement : IComponentData
{
    // type of enemy (ranged, melee, tank)
    public NPCType npcType;
    // reference to the enemy base this agent is trying to reach
    private Entity enemyBase;
    //current state of state machine
    public NPCState currentState;
    //speed to move towards the target
    public float MoveSpeed;
    //how much damage this does when it attacks
    public float damage;
    //radius of the circle collision
    public float enemyRangeDetection;
    // the team this agent is on
    private Team team;
    // starting health
    public float maximumHealth;
    // current health
    private float currentHealth;
    // entity currently being targeted
    public Entity currentTarget;
    
    
    
    
    public float3 TargetPosition;
    

}
