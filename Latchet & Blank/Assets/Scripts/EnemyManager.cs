using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RAIN.Core;

public class EnemyManager : MonoBehaviour
{
    List<GameObject> spawnpoints;
    List<GameObject> droneEnemies = new List<GameObject>();

    public GameObject droneEnemyPrefab;
    public float spawnInterval = 2f;
    public int maxDroneEnemies = 6;
    public int numToSpawn = 15;
    private int spawned = 0;
    private bool doneSpawning = false;

    // Use this for initialization
    void Start ()
	{
	    spawnpoints = GameObject.FindGameObjectsWithTag("Spawnpoint").ToList();
        if(numToSpawn == 0)
            StartCoroutine(Spawn());
        else
            StartCoroutine(SpawnCount());
    }
	
	// Update is called once per frame
	void Update ()
	{
        droneEnemies.RemoveAll(i => i == null);
        if (doneSpawning && droneEnemies.Count == 0)
        {
            //FindObjectOfType<GameManager>().LoadScene("Main Menu");
            Messenger.Broadcast(GameEvent.LEVEL_COMPLETE);
            Destroy(this.gameObject);
        }
    }

    IEnumerator Spawn()
    {
        while (true)
        {
            if (droneEnemies.Count < maxDroneEnemies)
            {
                int r = Random.Range(0, spawnpoints.Count);
                GameObject newEnemy = Instantiate(droneEnemyPrefab, spawnpoints[r].transform.position,
                    spawnpoints[r].transform.rotation);
                droneEnemies.Add(newEnemy);
                newEnemy.GetComponent<AIRig>().AIStart();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    IEnumerator SpawnCount() {
        while (true)
        {
            droneEnemies.RemoveAll(i => i == null);
            if (spawned < numToSpawn)
            {
                spawned++;
                int r = Random.Range(0, spawnpoints.Count);
                GameObject newEnemy = Instantiate(droneEnemyPrefab, spawnpoints[r].transform.position,
                    spawnpoints[r].transform.rotation);
                droneEnemies.Add(newEnemy);
            }
            else {
                doneSpawning = true;
                break;
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}