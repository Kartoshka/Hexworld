using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSelector : MonoBehaviour {

	public ChunkManager cManager;

	public GameObject selector;

	private Vector3 hitBlock;
	private bool active;
	private bool blockSelected;

	public float rayCastDistance = 8f;
	public float normalOffsetDistance = 0.2f;
	// Use this for initialization
	void Start () {
		if (selector != null) {
			selector = (GameObject)Instantiate (selector, new Vector3(0,1024,0), selector.transform.rotation);
		}
	}

	public void UpdateState(bool removing){

		Vector3 trackedCursor;
		Vector3 fwd = transform.TransformDirection (Vector3.forward);
		RaycastHit hit;

		if (Physics.Raycast (transform.position, fwd, out hit, rayCastDistance)) {
			
			if (removing) {
				trackedCursor =  hit.point + hit.normal * (-normalOffsetDistance);
			} else {
				trackedCursor =  hit.point - hit.normal * (-normalOffsetDistance);
			}
				
			hitBlock = snapCoordsToGrid (trackedCursor);

			selector.SetActive (true);
			selector.transform.position = hitBlock;

			blockSelected = true;
		} 
		else
		{
			selector.SetActive (false);
			blockSelected = false;
		}

	}

	public Vector3 snapCoordsToGrid(Vector3 position){
		int[] gridPos =getBlockCoords (position);
		return new Vector3 (gridPos [0] * 0.866f * 2f + Mathf.Abs (gridPos [2] % 2) * 0.866f, gridPos [1] * cManager.blockSize - 0.002f, gridPos [2] * 1.5f);
	}



	public int[] getBlockCoords(Vector3 position)
	{

		int zRound = Mathf.FloorToInt ((position.z + 1) / 1.5f);
		float z = (float)zRound * 1.5f;

		int xRound = Mathf.FloorToInt ((position.x + 0.866f + Mathf.Abs(zRound % 2)*0.866f) / (2 * 0.866f));
		float x = xRound * 2f * 0.866f - Mathf.Abs(zRound % 2)*0.866f;

		float zInTile = position.z + 1 - z;
		float xInTile = position.x - x;


		if (zInTile > Mathf.Abs (xInTile * (0.866f / 2f))) 
		{
			xRound -= Mathf.Abs(zRound % 2);
		} 
		else 
		{
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
		
	public bool isBlockSelected(){
		return blockSelected;
	}

	public Vector3 getSelectedBlock(){
		return hitBlock;
	}

	public void Deactivate(){
		selector.SetActive (false);
		blockSelected = false;
	}
}
