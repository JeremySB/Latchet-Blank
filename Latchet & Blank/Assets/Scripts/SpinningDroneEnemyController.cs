using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using UnityEngine;

public class SpinningDroneEnemyController : MonoBehaviour, IEnemy
{

    bool dead = false;
    float deathTimer = 0;
    private float health;

    // charging vars
    enum ChargingState {Charging, Charged, Discharging, Discharged};
    ChargingState chargingState = ChargingState.Discharged;
    float percentageCharged = 0;
    float currentSpin = 0;
    float currentIllumination = 1f;
    public float maxSpin = 50f;
    public float maxIllumination = 5f;
    public float minSpin = 0f;
    public float minIllumination = 1f;
    public float chargingRate = 0.5f;
    public float dischargingRate = 0.5f;

    Transform playerTarget;
    readonly Quaternion endingRotation = Quaternion.Euler(new Vector3(60, 0, 60));
    Quaternion startingRotation;
    float builtUpDamage = 0f;
    AudioSource audio;

    public int damagePerSecond = 25;
    public Transform body;
    public Light firingLight;
    public float range = 20f;
    public int startingHealth = 250;
    public GameObject shotPrefab;
    public GameObject lightningPrefab;

    // Use this for initialization
    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("PlayerTarget").transform;
        GetComponent<AIRig>().AIStart();
        health = startingHealth;
        StartCoroutine(Cooldown());
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dead)
        {
            transform.rotation = Quaternion.Lerp(startingRotation, endingRotation, deathTimer);
            deathTimer += Time.deltaTime * 1;
            audio.Stop();
            return;
        };

        if (percentageCharged > .75f)
        {
            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }
        else if(audio.isPlaying)
        {
            audio.Stop();
        }

        if(builtUpDamage < damagePerSecond/4f) builtUpDamage += damagePerSecond * Time.deltaTime;

        //transform.LookAt(playerTarget);

        // update current spin and charge illumination

        if (chargingState == ChargingState.Charging)
        {
            percentageCharged += Time.deltaTime * chargingRate;
            currentSpin = Mathf.Lerp(minSpin, maxSpin, percentageCharged);
            currentIllumination = Mathf.Lerp(minIllumination, maxIllumination, percentageCharged);
            firingLight.intensity = currentIllumination;
            if (percentageCharged >= 1f)
            {
                percentageCharged = 1f;
                chargingState = ChargingState.Charged;
            }
        }
        else if (chargingState == ChargingState.Discharging)
        {
            percentageCharged -= Time.deltaTime * dischargingRate;
            currentSpin = Mathf.Lerp(minSpin, maxSpin, percentageCharged);
            currentIllumination = Mathf.Lerp(minIllumination, maxIllumination, percentageCharged);
            firingLight.intensity = currentIllumination;
            if (percentageCharged <= 0f)
            {
                percentageCharged = 0f;
                chargingState = ChargingState.Discharged;
            }
        }
        
    }

    void FixedUpdate()
    {
        body.Rotate(0, 0, currentSpin * Time.deltaTime);
    }

    public bool FireAtPlayer()
    {
        if (Vector3.Distance(playerTarget.position, transform.position) > range)
        {
            return false;
        }
        GameObject shot = Instantiate(shotPrefab, transform.position, transform.rotation);
        shot.transform.position += transform.forward * 1;
        int damage =(int)builtUpDamage;
        builtUpDamage -= damage;
        shot.GetComponent<ShotController>().damage = damage;
        Destroy(shot, 3f);
        
        return true;
    }
    
    public void Damage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Death();
        }

        SpawnLightning(0.2f);
    }


    public bool ChargeUp()
    {
        if (chargingState == ChargingState.Charged)
        {
            return true;
        }
        chargingState = ChargingState.Charging;
        return false;
    }

    IEnumerator Cooldown()
    {
        while (true)
        {
            if (chargingState == ChargingState.Charged || chargingState == ChargingState.Charging)
            {
                yield return new WaitForSeconds(0.6f);
                chargingState = ChargingState.Discharging;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Death()
    {
        dead = true;
        startingRotation = transform.rotation;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<AIRig>().AI.IsActive = false;
        SpawnLightning(2.5f);
        Destroy(this.gameObject, 2.5f);
    }

    void SpawnLightning(float duration)
    {
        GameObject lightning = Instantiate(lightningPrefab, this.gameObject.transform);
        lightning.transform.localScale = new Vector3(2f, 2f, 2f);
        lightning.transform.localPosition = new Vector3(0, 0, 1.0f);
        lightning.transform.localEulerAngles = new Vector3(0, 90, 0);
        Destroy(lightning, duration);
    }
}