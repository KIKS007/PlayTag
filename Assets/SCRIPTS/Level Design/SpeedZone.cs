using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZone : MonoBehaviour
{

    public float speedModifier;

    void OnTriggerEnter(Collider col)
    {
        if (speedModifier != 0f && col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (col.tag == "Mouse")
                col.GetComponent<Mouse>().speedBoost = speedModifier;
            else
                col.GetComponent<Cat>().speedBoost = speedModifier;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (speedModifier != 0f && col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (col.tag == "Mouse")
                col.GetComponent<Mouse>().speedBoost = 0f;
            else
                col.GetComponent<Cat>().speedBoost = 0f;
        }
    }
}
