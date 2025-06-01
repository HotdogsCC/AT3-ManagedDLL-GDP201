using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


// Trivial script to move projectile towards it's target
// Projectile is destroyed if it travels the full distance or 
// It's target is destroyed before it reaches it
// When projectile travels it's full distance it applies damage to the target
// Then destroys itself
public class ProjectileScript : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject target;
    float distanceToGo = 100;
    public float speed = 10;
    float damage;
    BaseNPCBehaviour enemyScript;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If the target has been destroyed whilst the projectile is in flight then destroy the projectile
        if (target == null)
        {
            Object.Destroy(gameObject);
            return;
        }
        // update distance stll to go based on speed and deltatime
        distanceToGo -= speed * Time.deltaTime;
        // If projectile as reaches the target then apply damage and destroy projectile
        if (distanceToGo < 0)
        {
            GameObject.Destroy(gameObject);
            if(enemyScript != null)
            {
                enemyScript.takeDamage(damage);
            }
        }
        else
        {
            // Else we move projectile towards the target
            Vector3 heading = (target.transform.position - transform.position).normalized;
            Vector3 pos = transform.position;
            pos += heading * Time.deltaTime * speed;
            pos.y = 1.5f; // fix the y component
            transform.position = pos;
        }
    }

    //public method to allow spawning code to set projectile up
    public void SetUp(BaseNPCBehaviour enemyScript,float damage)
    {
        // set the projectiles target
        this.enemyScript = enemyScript;
        this.target = enemyScript.gameObject;    
        this.damage = damage; // how much damage the projectile does
        // To keep things simple we work out distance between projectile and target at spawn
        // Note that the projectile moves towards the target so this distance tavelled wont be this exactly
        // However, given how fast the projectile moves relative to the target the player will not see the slight inconsistency
        distanceToGo = (target.transform.position - transform.position).magnitude;
    }
}
