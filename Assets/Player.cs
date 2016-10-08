using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public ChunkManager cManager;
	public GameObject selector;

	private Vector3 hit = Vector3.zero;
	public float rayCastDistance =10f;
	// Use this for initialization
	void Start () {
		if (selector != null) {
			selector = (GameObject)Instantiate (selector, new Vector3(0,1024,0), selector.transform.rotation);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (selector != null) {
			Vector3 fwd = transform.TransformDirection (Vector3.forward);
			RaycastHit hit;
			if (Physics.Raycast (transform.position, fwd, out hit, rayCastDistance)) {
					selector.SetActive (true);
					if (cManager == null) 
					{
						selector.transform.position = hit.point;
					}
					else
					{

					Vector3 position = hit.point + fwd.normalized * 0.1f;

					this.hit = position;
					float y = Mathf.Floor (position.y / cManager.blockSize) * cManager.blockSize;

					int zRound = Mathf.FloorToInt ((position.z + 1) / 1.5f);

					//float z = Mathf.Floor ((position.z+1) / 1.5f) *1.5f;
					float z = (float)zRound * 1.5f;
						
					float x = Mathf.Floor ((position.x + 0.866f + (zRound % 2)*0.866f) / (2 * 0.866f)) * 2f * 0.866f - (zRound % 2)*0.866f;
						
					selector.transform.position = new Vector3 (x, y, z);
					}


			} else {
				selector.SetActive (false);
			}
		}
		
	}


	void OnDrawGizmos(){
		Gizmos.DrawRay (transform.position, transform.TransformDirection (Vector3.forward).normalized * rayCastDistance);
		Gizmos.DrawSphere (hit, 0.1f);
	}
}
