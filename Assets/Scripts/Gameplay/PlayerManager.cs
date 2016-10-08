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
		 
		HexObj selectedItem = inv.getSelection ();

		removing = selectedItem.remove;

		if (removing || selectedItem.add) 
		{
			bSelector.UpdateState (removing);
		} 
		else 
		{
			bSelector.Deactivate ();
		}


		if (bSelector.isBlockSelected()) {
			if (Input.GetButtonDown ("Fire1")) {
				Debug.Log ("removed block at position " + bSelector.getSelectedBlock());
			}
		}
		if (Input.GetButtonDown ("Fire2")) {
			removing = !removing;
		}
		
	}
}
