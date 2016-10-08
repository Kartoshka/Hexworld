using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {


	public List<HexObj> itemList;
	public int selectedItem =0;
	
	// Update is called once per frame
	void Update () {
		
	}


	public HexObj getCurrentSelectedItem(){
		if (selectedItem > itemList.Count) {	
			selectedItem = itemList.Count - 1;
		}

		return itemList [selectedItem];
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
