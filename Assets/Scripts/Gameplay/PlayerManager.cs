using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryManager;

public class PlayerManager : MonoBehaviour {

	public ChunkManager cManager;

	public Inventory inv;

	public BlockSelector bSelector;

    
	private bool removing = false;
	
	// Update is called once per frame
	void Update ()
    {		 
        //Switch items
		if (Input.GetButtonDown ("NextItem")) {
			inv.nextItem ();
		}
		HexObj selectedItem = inv.getSelection ();

		removing = selectedItem.remove;

        if (removing || selectedItem.add)
        {
            bSelector.UpdateState(removing, !removing, selectedItem.stack);
            //If Left click
            if (Input.GetButtonDown("Fire1") && bSelector.isBlockSelected())
            {
               
                if (removing) //Remove block
                { 

                }
                else //add block
                {               
                    if (selectedItem.numItems > 0)
                    {
                        try
                        {
                            selectedItem.numItems -= 1;
                            ChunkManager.Chunk chunk = cManager.getChunkAtPos(bSelector.getSelectedBlock());
                            Block b = new Block(bSelector.getSelectedBlock(), selectedItem.stack * 0.25f, (short)selectedItem.blockId);

                            if (cManager.addBlock(b, chunk, true, true))
                            {

                            }

                        }
                        catch (UnityException e)
                        {
                            Debug.Log(e.Message);
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
