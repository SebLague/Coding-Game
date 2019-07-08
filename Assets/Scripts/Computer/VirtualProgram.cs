using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualProgram : MonoBehaviour {
    public GameObject holder;
    protected bool active;

    public virtual void SetActive (bool active) {
        this.active = active;
        holder.SetActive (active);
    }

}