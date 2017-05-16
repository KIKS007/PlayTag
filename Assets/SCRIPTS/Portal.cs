using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

    public Portal twinPortal;

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == 8)
            col.transform.position = twinPortal.transform.position + twinPortal.transform.forward * 2f;
    }
}
