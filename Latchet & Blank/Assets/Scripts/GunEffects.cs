using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunEffects : MonoBehaviour {
    bool firing;
    PlayerController playerController;

    public AudioSource source;
    public GameObject rifleShotPrefab;
    public int playerDamage;
    public ParticleSystem muzzleFlash;
    public ParticleSystem shells;
    public Transform rifleBarrelPoint;

    // Use this for initialization
    void Start () {
        Messenger.AddListener(GameEvent.LEVEL_COMPLETE, stopShooting);
        Messenger.AddListener(GameEvent.PLAYER_DEATH, stopShooting);
        playerController = FindObjectOfType<PlayerController>();
        firing = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.timeScale < 0.01f) return;
        if (playerController.isShooting && !firing)
        {
            muzzleFlash.Play();
            shells.Play();
            source.Play();
            firing = true;
        }
        else if (!playerController.isShooting && firing) {
            muzzleFlash.Stop();
            shells.Stop();
			source.Stop();
            firing = false;
        }
	}
    public void OnDestroy()
    {
        Messenger.RemoveListener(GameEvent.LEVEL_COMPLETE, stopShooting);
        Messenger.RemoveListener(GameEvent.PLAYER_DEATH, stopShooting);
    }
    public void stopShooting()
    {
        muzzleFlash.Stop();
        shells.Stop();
        source.Stop();
        firing = false;
    }
    public void fireBullet()
    {
        GameObject shot = Instantiate(rifleShotPrefab, rifleBarrelPoint.transform.position, rifleBarrelPoint.transform.rotation);
        shot.GetComponent<ShotController>().damage = playerDamage;
        shot.transform.Rotate(new Vector3(0, 180, 0));
        Destroy(shot, 1.5f);
    }
}

