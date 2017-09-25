using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHit : MonoBehaviour, IEnemy {
    public GameObject[] toShow;
	// Use this for initialization
	void Start () {
        foreach (GameObject hide in toShow) {
            hide.GetComponent<Renderer>().enabled = false;
            hide.GetComponent<Collider>().enabled = false;
        }
	}

    public void Damage(int amount) {
        foreach (GameObject hide in toShow)
        {
            hide.GetComponent<Renderer>().enabled = true;
            hide.GetComponent<Collider>().enabled = true;
        }
        Destroy(this.gameObject);
    }

    bool IEnemy.FireAtPlayer()
    {
        throw new NotImplementedException();
    }
}
