using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consts;

namespace InventoryManager{
	public class Inventory : MonoBehaviour {


		public List<HexObj> itemList;
		public int selectedItem =0;
		
		// Update is called once per frame
		void Update () {
			
		}


		public HexObj getSelection(){
			if (selectedItem > itemList.Count) {	
				selectedItem = itemList.Count - 1;
			}

			return itemList [selectedItem];
		}

		public void nextItem(){
			selectedItem = (selectedItem + 1) % itemList.Count;
		}

		public void selectItem(int index){
			if (index < itemList.Count) {
				selectedItem = index;
			}
		}
			
	}

	[System.Serializable]
	public struct HexObj{
		public BLOCKID blockId;

		public int stack;

		public int numItems;


		public bool add;

		public bool remove;
	}
}