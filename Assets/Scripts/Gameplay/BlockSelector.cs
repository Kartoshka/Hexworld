using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consts;


public class BlockSelector : MonoBehaviour {

    public float heightPlayer = 0;

	public ChunkManager cManager;

    public GameObject SelectorPrefab;
	private GameObject selector;

	private Vector3 hitBlock;
	private bool active;
	private bool blockSelected;

	public float rayCastDistance = 8f;
	public float normalOffsetDistance = 0.2f;
    public float minimumDistance = 1f;
	// Use this for initialization
	void Start () {

		if (SelectorPrefab != null) {
			selector = (GameObject)Instantiate (SelectorPrefab, new Vector3(0,1024,0), SelectorPrefab.transform.rotation);
		}
	}

	public void UpdateState(bool removing,bool allowOverlap,int blockSize){

		Vector3 trackedCursor;
		Vector3 fwd = transform.TransformDirection (Vector3.forward);
		RaycastHit hit;

        blockSelected = false;
        if (Physics.Raycast (transform.position, fwd, out hit, rayCastDistance)) {

            //Determine which way to have our selector (in block or out of block)
            if (removing)
            {
                trackedCursor = hit.point + hit.normal * (-normalOffsetDistance);
            }
            else
            {
                trackedCursor = hit.point - hit.normal * (-normalOffsetDistance);
            }


            //Snap currently looked at position to a grid point
            hitBlock = cManager.snapCoordsToGrid(trackedCursor);
            Vector3 playerPosition = cManager.snapCoordsToGrid(this.gameObject.transform.position);

            bool canfit = true;
            //Check if there's space for selecton
            
                for (int i = 0; i < blockSize; i++)
                {
                    short b = cManager.getBlockTypeAtAbsPos(trackedCursor + new Vector3(0, i * cManager.blockSize, 0));

                    canfit = canfit && (b == (short)BLOCKID.Air);
                }
            canfit = canfit || removing;

            if (hitBlock.x == playerPosition.x && hitBlock.z == playerPosition.z)
            {
                int divisions = (int)(heightPlayer / cManager.blockSize);
                float maxHeight = playerPosition.y + cManager.blockSize * divisions * 0.5f;
                float minHeight = playerPosition.y - cManager.blockSize * divisions * 0.5f;

                if (hitBlock.y < minHeight || hitBlock.y > maxHeight)
                {
                    blockSelected = true && canfit;
                }
            }
            else
            {
                blockSelected = true && canfit;
            }  
        } 
		else
		{
			blockSelected = false;
		}

        //Set position of block
        selector.SetActive(blockSelected);
        if (blockSelected)
        {
            selector.transform.position = hitBlock;
            selector.transform.localScale = new Vector3(selector.transform.localScale.x, selector.transform.localScale.y, blockSize * cManager.blockSize);
        }

    }

	//public Vector3 snapCoordsToGrid(Vector3 position){
	//	int[] gridPos =getBlockCoords (position);
	//	return new Vector3 (gridPos [0] * 0.866f * 2f + Mathf.Abs (gridPos [2] % 2) * 0.866f, gridPos [1] * cManager.blockSize - 0.002f, gridPos [2] * 1.5f);
	//}



	//public int[] getBlockCoords(Vector3 position)
	//{

	//	int zRound = Mathf.FloorToInt ((position.z + 1) / 1.5f);
	//	float z = (float)zRound * 1.5f;

	//	int xRound = Mathf.FloorToInt ((position.x + 0.866f + Mathf.Abs(zRound % 2)*0.866f) / (2 * 0.866f));
	//	float x = xRound * 2f * 0.866f - Mathf.Abs(zRound % 2)*0.866f;

	//	float zInTile = position.z + 1 - z;
	//	float xInTile = position.x - x;


	//	if (zInTile > Mathf.Abs (xInTile * (0.866f / 2f))) 
	//	{
	//		xRound -= Mathf.Abs(zRound % 2);
	//	} 
	//	else 
	//	{
	//		//z -= 1.5f;
	//		zRound--;
	//		if (xInTile > 0) {
	//			//x += 0.866f;
	//			//xRound++;
	//		} else {
	//			//x -= 0.866f;
	//			xRound--;
	//		}
	//	}
	//	int[] coords = {xRound , Mathf.FloorToInt (position.y / cManager.blockSize), zRound};
    
	//	return coords;
		
	//}

	public bool isBlockSelected(){
		return blockSelected && selector.active;
	}

	public Vector3 getSelectedBlock(){
		return hitBlock;
	}

	public void Deactivate(){
		selector.SetActive (false);
		blockSelected = false;
	}
}
