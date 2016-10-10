using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consts;

[System.Serializable]
public class Item  {


	[HideInInspector]
	public BLOCKID blockId;

	private int _stack=1;

	public static int maxStack =4;
	public int stack 	
	{	
		get
		{
			return _stack;
		}
		set
		{
			if(value>0){
				_stack = (int)Mathf.Clamp((value%(maxStack+1)),1,maxStack);
			}
		}
	}

	private int _numItems =0;

	public int numItems {
		get
		{
			return _numItems;
		}
		set
		{
			if(value>=0){
				_numItems = value;
			}
		}
	}

	public bool add { get; private set; }

	public bool remove { get; private set; }

	public Item(BLOCKID id, int s, int n, bool add){
		this.blockId = id;
		stack = s;
		numItems = n;
		this.add = add;
		this.remove = !add;
	}

}
