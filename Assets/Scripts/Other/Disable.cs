using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disable : MonoBehaviour {
    void OnEnable () {
        if (FindObjectOfType<VirtualOS> ()) {
            gameObject.SetActive (false);
        }
    }
}