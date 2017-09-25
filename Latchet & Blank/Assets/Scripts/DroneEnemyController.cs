using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using UnityEngine;

public class DroneEnemyController : MonoBehaviour,IEnemy
{

    bool dead = false;
    float deathTimer = 0;
    private float health;
    
    Transform playerTarget;
    readonly Quaternion endingRotation = Quaternion.Euler(new Vector3(45, 0, 45));
    Quaternion startingRotation;
    AudioSource audio;

    public int startingHealth = 50;
    public float range = 20f;
    public int damage = 5;
    public GameObject shotPrefab;
    public GameObject lightningPrefab;

    // Use this for initialization
    void Start ()
	{
	    playerTarget = GameObject.FindGameObjectWithTag("PlayerTarget").transform;
        GetComponent<AIRig>().AIStart();
        health = startingHealth;
	    audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (dead)
	    {
            transform.rotation = Quaternion.Lerp(startingRotation, endingRotation, deathTimer);
	        deathTimer += Time.deltaTime * 2;
	        return;
	    };
        transform.LookAt(playerTarget);

        // kill if glitched out
	    if (playerTarget && Vector3.Distance(playerTarget.position, transform.position) > 500)
	    {
	        Destroy(gameObject);
            Debug.Log("Drone out of bounds - destroyed");
	    }
    }

    public bool FireAtPlayer()
    {
        if (!playerTarget && Vector3.Distance(playerTarget.position, transform.position) > range)
        {
            return false;
        }
        GameObject shot = Instantiate(shotPrefab, transform.position, transform.rotation);
        shot.transform.position += transform.forward * 1;
        shot.GetComponent<ShotController>().damage = damage;
        Destroy(shot, 3f);
        audio.Play();
        return true;
    }
    
    public void Damage(int amount)
    {
        health -= amount;
        if(health <= 0)
        {
            Death();
        }
        SpawnLightning(0.2f);
    }

    void Death()
    {
        dead = true;
        startingRotation = transform.rotation;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<AIRig>().AI.IsActive = false;
        SpawnLightning(2f);
        Destroy(this.gameObject, 2f);
    }

    void SpawnLightning(float duration)
    {
        GameObject lightning = Instantiate(lightningPrefab, this.gameObject.transform);
        Destroy(lightning, duration);
    }
}