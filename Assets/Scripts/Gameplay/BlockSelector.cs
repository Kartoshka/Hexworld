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
			hitBlock = cManager.snapCoordsToGrid(trackedCursor) - new Vector3(0, -0.002f, 0); //Vertical selected block y offset to make it evenly spaced over regular blocks
            
			Vector3 playerPosition = cManager.snapCoordsToGrid(this.gameObject.transform.position);

            bool canfit = true;


			if (!removing) {
				ChunkManager.Chunk container = cManager.getChunkAtPos (hitBlock); //NOT SURE

				for (int f = 0; f < blockSize; f++) {
					canfit = true;
					Vector3 originalLocalPos = cManager.getLocalBlockCoords (hitBlock); //NOT SURE

					Debug.Log (originalLocalPos);

					//Check if there's space for selection
					for (int i = 0; i < blockSize; i++) {
						canfit = canfit && (container.blockTypes [(int)originalLocalPos.x, (int)originalLocalPos.z, (int)originalLocalPos.y + i] == (short)BLOCKID.Air); //ERROR OUT OF BOUNDS ON X
					}

					if (canfit)
					{
						break;
					}
					hitBlock += new Vector3 (0, -cManager.blockSize, 0);
				}
			}

			canfit = canfit || removing;


            if (hitBlock.x == playerPosition.x && hitBlock.z == playerPosition.z)
            {
                int divisions = (int)(heightPlayer / cManager.blockSize);
                float maxHeight = playerPosition.y + cManager.blockSize * divisions * 0.5f;
                float minHeight = playerPosition.y - cManager.blockSize * divisions * 0.5f;

				if (hitBlock.y < minHeight || hitBlock.y > maxHeight) {
					blockSelected = true && canfit;
				}
            }
            else
            {
                blockSelected = true && canfit;
            }  
			//blockSelected = true;
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
