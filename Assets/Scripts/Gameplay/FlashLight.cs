using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : MonoBehaviour {

	bool on = false;
	bool auto = true;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Autolight")) {
			auto = !auto;
			on = false;
		}

		if (!auto) {
			if (Input.GetButtonDown ("Flashlight")) {
				on = !on;
			}
		} else {
			Vector3 up = transform.TransformDirection (Vector3.up);

			Vector3 upRight = transform.TransformDirection (new Vector3 (0f, 0.7f, 0.7f));
			Vector3 upLeft = transform.TransformDirection (new Vector3 (0f, 0.7f, -0.7f));

			Vector3 upBack = transform.TransformDirection (new Vector3 (-0.7f, 0.7f, 0f));
			Vector3 upFront = transform.TransformDirection (new Vector3 (0.7f, 0.7f, 0f));

			if (Physics.Raycast (transform.position, up, float.MaxValue) && Physics.Raycast (transform.position, upRight, float.MaxValue)
			   && Physics.Raycast (transform.position, upLeft, float.MaxValue) && Physics.Raycast (transform.position, upBack, float.MaxValue)
			   && Physics.Raycast (transform.position, upFront, float.MaxValue)) {
				on = true;
			} else {
				on = false;

			}
		}

		this.GetComponent<Light> ().enabled = on;

		
	}
}
