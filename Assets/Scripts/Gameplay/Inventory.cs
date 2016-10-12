using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consts;

namespace InventoryManager{
	public class Inventory : MonoBehaviour {

		public InventoryUI ui;

		[SerializeField]
		private List<Item> itemList;


		public int selectedItem =0;

		void Start(){
			itemList = new List<Item> ();
			BLOCKID[] blocks = ConstsClass.getAllIDs ();

			foreach (BLOCKID b in blocks) {
				itemList.Add (new Item (b,1,99,true));
			}

			if (ui != null) {
				ui.init (this);
			}
		}

		private Item getSelection(){
			
			if (selectedItem > itemList.Count) {	
				selectedItem = itemList.Count - 1;
			}

			return itemList [selectedItem];
		}

		public void nextItem(){
			selectedItem = (selectedItem + 1) % itemList.Count;
			if (ui != null) {
				ui.UpdateValues ();
			}
		}

		public void selectItem(int index){
			if (index < itemList.Count) {
				selectedItem = index;
			}
			if (ui != null) {
				ui.UpdateValues ();
			}
		}

		public BLOCKID getCurrentID(){
			return this.getSelection ().blockId;
		}

		public bool getCurrentRemove(){
			return this.getSelection ().remove;
		}

		public bool getCurrentAdd(){
			return this.getSelection ().add;
		}

		public int getCurrentNumItems(){
			return this.getSelection ().numItems;
		}

		public int getCurrentStack(){
			return this.getSelection ().stack;
		}

		public void IncrementCurrentNumItems(int amount){
			this.getSelection ().numItems = this.getSelection ().numItems + amount;
			if (this.getSelection ().numItems == 0) {
				this.getSelection ().numItems = 99;
			}
			if (ui != null) {
				ui.UpdateValues ();
			}
		}

		public void IncrementCurrentStack(int amount){
			if(this.getSelection().blockId != BLOCKID.LampO)
				this.getSelection ().stack = this.getSelection ().stack + amount;
			if (ui != null) {
				ui.UpdateValues ();
			}
		}

		public Item[] getItems(){
			return itemList.ToArray();
		}

	}



}