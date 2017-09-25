using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RAIN.Core;

public class BlankController : MonoBehaviour {
	// Use this for initialization
	void Start () {
        GetComponent<AIRig>().AIStart();
        GetComponent<AIRig>().AI.WorkingMemory.SetItem("blankMoveSpeed", 3.5);
    }
	
	// Update is called once per frame
	void Update () {
		if(Vector3.Distance(GameObject.FindGameObjectWithTag("PlayerTarget").transform.position, this.gameObject.transform.position) >= 5)
        {
            GetComponent<AIRig>().AI.WorkingMemory.SetItem("blankMoveSpeed", 15);
        }
        else
        {
            GetComponent<AIRig>().AI.WorkingMemory.SetItem("blankMoveSpeed", 3.5);
        }
	}
}
