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
			bSelector.UpdateState (removing);
		} 
		else 
		{
			bSelector.Deactivate ();
		}


		if (bSelector.isBlockSelected()) {
			if (Input.GetButtonDown ("Fire1")) {
			}
		}
		if (Input.GetButtonDown ("Fire2")) {
			removing = !removing;
		}
		
	}
}
