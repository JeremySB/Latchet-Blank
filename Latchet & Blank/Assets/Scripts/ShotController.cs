using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotController : MonoBehaviour
{

    public float speed = 10f;
    public int damage = 5;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        IEnemy enemy = other.gameObject.GetComponent<IEnemy>();
        //Debug.Log("Bullet hit: " + other.tag);
        if(tag == "PlayerBullet" && enemy != null)
        {
            Debug.Log("Enemy Hit!");
            enemy.Damage(damage);
            Destroy(this.gameObject);
        }
        else if (tag == "EnemyBullet" && other.tag == "Player")
        {
            Debug.Log("Player Hit!");
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.Damage(damage);
            Destroy(this.gameObject);
        }
        if (enemy == null && other.tag != "EnemyBullet" ) //other.tag != "Enemy")
        {
            Destroy(this.gameObject);
        }
    }
}
