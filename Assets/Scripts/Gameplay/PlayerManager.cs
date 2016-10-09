using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryManager;

public class PlayerManager : MonoBehaviour {

	public ChunkManager cManager;

	public Inventory inv;

	public BlockSelector bSelector;


	public bool removing = true;
	
	// Update is called once per frame
	void Update () {
		 
		if (Input.GetButtonDown ("NextItem")) {
			inv.nextItem ();
		}
		HexObj selectedItem = inv.getSelection ();

		removing = selectedItem.remove;

		if (removing || selectedItem.add) 
		{
			bSelector.selector.transform.localScale = new Vector3(bSelector.selector.transform.localScale.x,bSelector.selector.transform.localScale.y,selectedItem.stack * 0.25f);
			bSelector.UpdateState (removing);
			if (removing) {
			
			}
			else
			{
				if (bSelector.isBlockSelected()) {
					if (Input.GetButtonDown ("Fire1")) {
						if (selectedItem.numItems > 0) {
							try {
								selectedItem.numItems -=1;
								ChunkManager.Chunk chunk = cManager.getChunkAtPos (bSelector.getSelectedBlock ());
								Block b = new Block (bSelector.getSelectedBlock (), selectedItem.stack*0.25f, (short)selectedItem.blockId);

								if (cManager.addBlock (b, chunk, true,true)) {

								}

							} catch (UnityException e) {
								Debug.Log (e.Message);
							}
						}
					}
				}

			}

		} 
		else 
		{
			bSelector.Deactivate ();
		}



		if (Input.GetButtonDown ("Fire2")) {
			removing = !removing;
		}
		
	}
}
