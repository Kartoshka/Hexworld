using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryManager{
	public class Inventory : MonoBehaviour {


		public List<HexObj> itemList;
		private int selectedItem =0;
		
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
			if (selectedItem + 1 < itemList.Count) {
				selectedItem++;
			}
		}

		public void selectItem(int index){
			if (index < itemList.Count) {
				selectedItem = index;
			}
		}
			
	}

	[System.Serializable]
	public struct HexObj{
		public GameObject associatedGameObject;
		//Selected will be dependent on object to be added
		public GameObject associatedshadow;

		public bool add;

		public bool remove;
	}
}