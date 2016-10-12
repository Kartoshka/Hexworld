using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Consts;

namespace Consts {
	public enum BLOCKID : short { Air = 0, Stone = 1, Dirt = 2, Grass = 3, StoneTile = 4, CrystalO = 5, LampO = 6 };
}


public class ConstsClass {



	private static string[] fileNames = { "HB_Air", "HB_Stone", "HB_Dirt", "HB_Grass", "HB_StoneTile", "HB_CrystalO", "HB_LampO" };

	private static string[] iconNames = { "Icon_Air", "Icon_Stone", "Icon_Dirt", "Icon_Grass" , "Icon_StoneTile", "Icon_CrystalO", "Icon_LampO"};

	public static GameObject getPrefab(BLOCKID id){
		return Resources.Load ("Art/Models/Blocks/" + fileNames [(int)(short)id]) as GameObject;
	}

	public static Sprite getSprite(BLOCKID id){
		return Resources.Load<Sprite> ("Art/UI/Icons/Blocks/" + iconNames [(int)(short)id]);
	}

	public static BLOCKID[] getAllIDs(){
		List<BLOCKID> list =Enum.GetValues(typeof(BLOCKID)).Cast<BLOCKID>().ToList();

		list.RemoveAt (0);

		return list.ToArray ();
	}

    //public enum BLOCKID : short { Air = 0, Stone = 1, Dirt = 2, Grass = 3 };

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
