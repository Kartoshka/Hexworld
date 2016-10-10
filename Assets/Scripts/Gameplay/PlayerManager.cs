using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryManager;
using Consts;

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

		removing = inv.getCurrentRemove ();

		int stack = inv.getCurrentStack ();
		int numItems = inv.getCurrentNumItems ();
		BLOCKID id = inv.getCurrentID ();

		if (removing || inv.getCurrentAdd())
        {
			bSelector.UpdateState(removing, !removing, stack);
            //If Left click
            if (Input.GetButtonDown("Fire1") && bSelector.isBlockSelected())
            {
               
                if (removing) //Remove block
                { 

                }
                else //add block
                {               
					if (numItems > 0)
                    {
                        try
                        {
                            ChunkManager.Chunk chunk = cManager.getChunkAtPos(bSelector.getSelectedBlock());
							Block b = new Block(bSelector.getSelectedBlock(), stack* 0.25f, (short)id);

							if (inv.getCurrentNumItems()-1*inv.getCurrentStack()>=0 && cManager.AddBlock(b, chunk, true, true))
                            {
								inv.IncrementCurrentNumItems(-1*inv.getCurrentStack());
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
			Debug.Log (inv.getCurrentStack ());

			inv.IncrementCurrentStack (1);
		}
		
	}
}
