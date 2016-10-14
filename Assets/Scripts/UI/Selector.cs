using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour {

    GameObject target;
    void OnTriggerStay(Collider c)
    {
        if(c.gameObject.tag == "Player"){
            this.gameObject.SetActive(false);
            Debug.Log("hey player");
        }
    }
}
