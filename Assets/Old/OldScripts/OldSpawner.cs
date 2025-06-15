using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyDLL;

public class OldSpawner : MonoBehaviour
{
    public Team team; //
    float currentHealth; //
    float spawnTimer; //
    float spawnInterval = .3f; //
    string teamName;
    public BaseNPCBehaviour tankPrefab;  //
    public BaseNPCBehaviour rangedPrefab; //
    public BaseNPCBehaviour meleePrefab; //
    public OldSpawner enemySpawner; 
    public Material teamMaterial;
    uint nextID;
    // Start is called before the first frame update
    void Start()
    {
        nextID = 0;
        currentHealth = 200;
        spawnTimer = spawnInterval;
        if (team == Team.TEAM_1)
        {
            teamName = "Team 1";
        }
        else
        {
            teamName = "Team 2";
        }
        findSpawns();
    }

    // find all the spawns and store a reference to the enemy spawn so we can easilyu find it later
    void findSpawns()
    {
        GameObject[] spawns;
        spawns = GameObject.FindGameObjectsWithTag("Spawner");
        foreach(var spawn in spawns)
        {
            if (spawn != this.gameObject)
            {
                enemySpawner = spawn.GetComponent<OldSpawner>();
                if(enemySpawner == null)
                {
                    Debug.LogError("Object tagged as spawner doesn't have the spawn script");
                }
            }
        }
    }

    // Spawn a NPC and set it's team and color
    void spawnNPC(GameObject go)
    {
        // spawn at edge of spawner on side nearest enemy
        Vector3 direction = (enemySpawner.transform.position - transform.position).normalized;
        Vector3 spawnpoint = transform.position + direction * 10;
        GameObject npc = GameObject.Instantiate(go, spawnpoint, Quaternion.identity);
        BaseNPCBehaviour baseScript = npc.GetComponent<BaseNPCBehaviour>();
        baseScript.setUp(enemySpawner.gameObject,team,nextID++);
        MeshRenderer mr = npc.GetComponent<MeshRenderer>();
        mr.material = teamMaterial;
    }

    // Choose what to spawn
    void pickSpawn()
    {
        // Pick thing to spawn
        // 4/7 chance of melee
        // 2/7 chance of ranged
        // 1/7 chance of tank
        int random = Random.Range(0, 7);
        if(random == 0)
        {
            spawnNPC(tankPrefab.gameObject);
            spawnTimer += spawnInterval; // takes even longer to spawn tanks
        }
        else if (random < 5)
        {
            spawnNPC(meleePrefab.gameObject);
        }
        else 
        {
           spawnNPC(rangedPrefab.gameObject);
            spawnTimer += spawnInterval * 0.5f; // takes longer to spawn ranged
        } 
    }

    // spawm at intervals of spawnInterval seconds
    void spawnControl()
    {
        spawnTimer -= Time.deltaTime;
        if (currentHealth > 0 && spawnTimer < 0)
        {
            pickSpawn();
            spawnTimer += spawnInterval;
        }

    }


    // Update is called once per frame
    void Update()
    {
        spawnControl();
    }

    // Take damage to spawn when attacked and print a message to the console
    public void doDamage(float damageValue)
    {
        currentHealth -= damageValue;

        if (currentHealth < 0)
        {
            string winningString = (team == Team.TEAM_1) ? "Green" : "Red";
            winningString += " Wins";
            Debug.Log(winningString);
            currentHealth = 0;
        }
        else
        {
            string damageString = (team == Team.TEAM_1) ? "Red" : "Green";
            damageString += " taking damage: ";
            damageString += damageValue;
            Debug.Log(damageString);
        }
    }
}
