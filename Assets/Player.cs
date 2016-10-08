using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public ChunkManager cManager;
	public GameObject selector;

	private Vector3 hitIn = Vector3.zero;
	private Vector3 hitOut = Vector3.zero;
	public float rayCastDistance = 8f;
	public float normalOffsetDistance = 0.2f;
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
				else{

					this.hitIn = hit.point + hit.normal * (-normalOffsetDistance);
					this.hitOut = hit.point + hit.normal * (normalOffsetDistance);


					int[] addCoords = getBlockCoords (hitOut);
					int[] removeCoords = getBlockCoords (hitIn);



					selector.transform.position = new Vector3 (removeCoords[0]*0.866f*2f + Mathf.Abs(removeCoords[2] % 2)*0.866f, removeCoords[1]*cManager.blockSize -0.002f, removeCoords[2]*1.5f);

				}


			} else {
				selector.SetActive (false);
			}
		}
		
	}

	public int[] getBlockCoords(Vector3 position){

		int zRound = Mathf.FloorToInt ((position.z + 1) / 1.5f);
		float z = (float)zRound * 1.5f;

		int xRound = Mathf.FloorToInt ((position.x + 0.866f + Mathf.Abs(zRound % 2)*0.866f) / (2 * 0.866f));
		float x = xRound * 2f * 0.866f - Mathf.Abs(zRound % 2)*0.866f;

		float zInTile = position.z + 1 - z;
		float xInTile = position.x - x;


		if (zInTile > Mathf.Abs (xInTile * (0.866f / 2f))) {
			xRound -= Mathf.Abs(zRound % 2);
		} else {
			//z -= 1.5f;
			zRound--;
			if (xInTile > 0) {
				//x += 0.866f;
				//xRound++;
			} else {
				//x -= 0.866f;
				xRound--;
			}
		}

		//float y = Mathf.Floor (position.y / cManager.blockSize) * cManager.blockSize;

		int[] coords = {xRound , Mathf.FloorToInt (position.y / cManager.blockSize), zRound};

		//Debug.Log (position.z + " : " + coords[2]);

		return coords;
		
	}


	void OnDrawGizmos(){
		Gizmos.DrawRay (transform.position, transform.TransformDirection (Vector3.forward).normalized * rayCastDistance);
		Gizmos.DrawSphere (hitIn, 0.05f);
		Gizmos.DrawSphere (hitOut, 0.05f);
	}
}
