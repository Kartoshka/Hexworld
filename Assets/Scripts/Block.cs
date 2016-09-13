using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {


    public Vector3 pos;
    public float vertScale;
    public short blockType;

    public Block(Vector3 p, float vs, short type) {
        pos = p;
        vertScale = vs;
        blockType = type;
    }
}
