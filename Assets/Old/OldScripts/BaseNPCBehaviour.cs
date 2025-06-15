using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;
using MyDLL;


public class BaseNPCBehaviour : MonoBehaviour
{
    public NPCType nPCType; // type of enemy (ranged, melee or tank)
    GameObject enemyBase; // reference to the enemy base we are attacking
    public NPCState currentState;  // current state of state machine (heading towards target, melee attack, ranged attack)
    public float speed = 4.0f; // speed to move towards target
    public float damage; // how much damage this does when it attacks
    public float enemyRangeDetection = 30; // range of enemy detection
    float searchYAngle; // how far we sweep our detection ray per update
    float searchIntervalTimer = 0; // how oftern we search
    public float searchInterval = 0.04f; // seconds between searches
    Team team; // the team we are on
    public float maximumHealth; // starting health
    public float coolDownTime; // time between attacks
    public float coolDownTimer; // timer for attacks
    float currentHealth; // our current health
    public GameObject currentTarget; // object we are currently targetting
    uint ID; // UID
    bool wallAvoidanceLeft; // Are we avoiding the wall to the left or right
    public GameObject projectile; // prefab for the projectile we spawn if this is a ranged NPC
    public LayerMask wallLayer; // Layer mask set to wall layer in editor
    // Start is called before the first frame update
    void Start()
    {
        currentState  = NPCState.HEADING_TO_TARGET;
        currentHealth = maximumHealth;
        coolDownTimer = 0;
    }

    // ray cast towards the current target to check if a wall is in the way 
    //Use layer mask so we only check for walls
    bool CheckForWall(out GameObject objectFound)
    {
        const float wallDetectionRange = 50;
        if ((currentTarget != null))
        {
            RaycastHit hit;
            Vector3 searchDirection = currentTarget.transform.position - transform.position;
            if (Physics.Raycast(transform.position, searchDirection, out hit, wallDetectionRange,wallLayer))
            {
                objectFound = hit.collider.gameObject;
                return true;
            }
        }
        objectFound = null;
        return false;
    }

    // at periodic intervals we cast a ray to look for enemies
    // Over time the ray sweeps around 360 degrees.
    // This is spread over time.
    NPCType SearchForObject(out GameObject objectFound)
    {
        searchIntervalTimer -= Time.deltaTime;
        if (searchIntervalTimer < 0)
        {
            searchIntervalTimer = searchInterval;
            RaycastHit hit;
            Vector3 searchDirection;
            // construct direction for search
            searchDirection = new Vector3(Mathf.Cos(searchYAngle), 0, Mathf.Sin(searchYAngle));
            searchYAngle += .5f; // Move the search angle round so we probe in a different direction next time

            //cast a ray in the current search direction and check if it hits something
            if (Physics.Raycast(transform.position, searchDirection, out hit, enemyRangeDetection))
            {
                // draw the ray in debug
                Debug.DrawRay(transform.position, searchDirection * hit.distance, Color.yellow, searchInterval);
                // check what we hit
                objectFound = hit.collider.gameObject;
                // if it has our NPC script on it then it's a possible target
                BaseNPCBehaviour npcScript = objectFound.GetComponent<BaseNPCBehaviour>();
                if (npcScript == null)
                {
                    // If it has no script then it can't be an enemy
                    return NPCType.NONE;
                }
                //If the NPC we found is on our team then we ignore it
                if(npcScript.getTeam() == team)
                {
                    return NPCType.NONE;
                }
                if (hit.collider.tag == "Melee")
                {
                    return NPCType.MELEE;
                }
                if (hit.collider.tag == "Ranged")
                {
                    return NPCType.RANGED;
                }
                if (hit.collider.tag == "Tank")
                {
                    return NPCType.TANK;
                }
            }
            else
            {
                // draw the ray in debug
                Debug.DrawRay(transform.position, searchDirection * enemyRangeDetection, Color.white, searchInterval);
            }
        }
        // If we haven't hit anything or it wasn't an NPC then return that nothing was foiund
        objectFound = null;
        return NPCType.NONE;
    }

    // Setup called when NPC is spawned
    public void setUp(GameObject enemyBase, Team team,uint ID)
    {
        this.enemyBase = enemyBase;
        this.team = team;
        this.ID = ID;
        currentTarget = enemyBase;
        wallAvoidanceLeft = (ID & 1) == 0;
    }

    // Take damage and destroy NPC when health gets to 0
    public void takeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            GameObject.Destroy(gameObject);
        }
    }

    // Do the Melee attack
    // Called from the state machine
    void doMeleeAttack()
    {
        coolDownTimer -= Time.deltaTime;
        if(coolDownTimer <= 0)
        {
            coolDownTimer = coolDownTime;
            // If the target has been destroyed then change and head towards enemy base
            if (currentTarget == null)
            {
                currentState = NPCState.HEADING_TO_TARGET;
                currentTarget = enemyBase;
            }
            else
            {
                // Apply damage to the enemy
                BaseNPCBehaviour npcCurretScript = currentTarget.GetComponent<BaseNPCBehaviour>();
                if (npcCurretScript != null)
                {
                    npcCurretScript.takeDamage(damage);

                }
            }
        }
    }

    void doRangeAttack()
    {
        coolDownTimer -= Time.deltaTime;
        if (coolDownTimer <= 0)
        {
            coolDownTimer = coolDownTime;
            //if we have a target then check it's still visible and in range
            // If it's not then we set the current target to Null
            if ((currentTarget != null))
            {
                RaycastHit hit;
                Vector3 searchDirection = currentTarget.transform.position - transform.position;
                if (Physics.Raycast(transform.position, searchDirection, out hit, enemyRangeDetection))
                {
                    // If the thing we've hit isn't our current target then assume we've lost the target
                    if (hit.collider.gameObject != currentTarget)
                    {
                        currentTarget = null;
                    }
                } 
            }

            // If the current target is null then start heading to base 
            if (currentTarget == null)
            {
                currentState = NPCState.HEADING_TO_TARGET;
                currentTarget = enemyBase;
            }
            else
            {
                // If we have a target then launch a projectile at it
                var enemyScript =  currentTarget.GetComponent<BaseNPCBehaviour>();
                // instantiate projectile
                GameObject newpProjectile = GameObject.Instantiate(projectile, transform.position, Quaternion.identity);
                ProjectileScript projScript = newpProjectile.GetComponent<ProjectileScript>();
                projScript.SetUp(enemyScript,damage);
            }
        }
    }

    // State machine which handles basic behavour for our NPC
    void runStateMachine()
    {
        switch (currentState)
        {
            case NPCState.HEADING_TO_TARGET:
                // if we don't have a target then make the enemy base the target
                if (currentTarget == null)
                {
                    currentTarget = enemyBase;
                }
                // check if we can see an enemy
                GameObject objectFound;
                NPCType typeFound = SearchForObject(out objectFound);
                // if we can see an enemy then make it the target
                if ( typeFound != NPCType.NONE)
                {
                    BaseNPCBehaviour npcScript = objectFound.GetComponent<BaseNPCBehaviour>();
                    if ((npcScript != null))
                    {
                        if (nPCType == NPCType.RANGED)
                        {
                            currentState = NPCState.RANGE_ATTACK;
                            currentTarget = objectFound;
                            break;
                        }
                        // if we don't currently have an NPC as a target then make this one the target
                        BaseNPCBehaviour npcCurretScript = currentTarget.GetComponent<BaseNPCBehaviour>();
                        if (npcCurretScript == null)
                        {
                            currentTarget = objectFound;
                        }
                        else
                        {
                            // If we already have a target then if we have now found a tank, target that instead
                            // Note this is a bit simplistic, we should really check if we alreayd have a tank but this works
                            if (npcScript.tag == "Tank")
                            {
                                // always target tanks
                                currentTarget = objectFound;
                            }
                        }
                    }
                }
                // If we haven't found a target (maybe it was destroyed) then now target the enemy base
                if(currentTarget == null)
                {
                    currentTarget = enemyBase;
                }
                Vector3 targetPos = currentTarget.transform.position;
                Vector3 heading = targetPos - transform.position;
                GameObject wallFound;
                //Basic Wall avoidance, only works with walls perpendicular to direction of enemy spawn
                if (CheckForWall(out wallFound))
                {
                    // if there is a wall in the way then it becomes the target and we head towards one of its edges
                    currentTarget = wallFound;
                    targetPos = wallFound.transform.position;
                    if (wallAvoidanceLeft)
                    {
                        targetPos.x -= 10 + (wallFound.transform.localScale.x * 0.5f);
                    }
                    else
                    {
                        targetPos.x += 10 + (wallFound.transform.localScale.x * 0.5f);
                    }
                    heading = targetPos - transform.position;
                    heading.y = 0;
                    // Check if we have avoided the wall and if we have then start to move towards enemy base again
                    if (heading.magnitude < 5)
                    {
                        wallAvoidanceLeft = (Random.Range(0, 2) == 0);
                        currentState = NPCState.HEADING_TO_TARGET;
                        currentTarget = enemyBase;
                        break;
                    }
                }
                heading.y = 0;
                heading.Normalize();
                Vector3 position = transform.position;
                position += heading * speed * Time.deltaTime;
                transform.position = position;
                break;

            case NPCState.MELEE_ATTACK:
                doMeleeAttack();
                break;

            case NPCState.RANGE_ATTACK:
                doRangeAttack();
                break;
        }
    }

    void Update()
    {
        runStateMachine();
    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            // if we have hit a wall then we don't do anything
        }
        else if (other.tag == "Spawner")
        {
            // If we have reached the spawner then self destruct and do a lot of damage
            OldSpawner spawnerScript = other.gameObject.GetComponent<OldSpawner>();
            if (spawnerScript != null)
            {
                if(spawnerScript.team != team)
                {
                    spawnerScript.doDamage(damage * 3);
                    GameObject.Destroy(gameObject);
                }
            }
        }
        else
        {
            // if we have hit an enemy then start a melee attack
            BaseNPCBehaviour enemyScript = other.gameObject.GetComponent<BaseNPCBehaviour>();
            if (enemyScript != null) 
            {
                if(enemyScript.team != team)
                {
                    currentState = NPCState.MELEE_ATTACK;
                    currentTarget = other.gameObject;
                }
            }
        }
    }

    public Team getTeam()
    {
        return team;
    }
}
