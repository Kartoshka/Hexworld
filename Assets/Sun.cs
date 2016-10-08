using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {

	public GameObject flashLight;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		RaycastHit hit;
		if (Physics.Raycast (transform.position, flashLight.transform.position - transform.position, out hit,float.MaxValue)) {
			if (hit.collider.gameObject != flashLight) {
			}
		}
	}
}
