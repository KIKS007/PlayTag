using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZone : MonoBehaviour {

    public float catSpeedModifier;
    public float mouseSpeedModifier;

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (mouseSpeedModifier != 0f && col.tag == "Mouse")
            {
                col.GetComponent<Mouse>().speedBoost = mouseSpeedModifier;
            }
            else if (catSpeedModifier != 0f)
            {
                col.GetComponent<Cat>().speedBoost = catSpeedModifier;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (mouseSpeedModifier != 0f && col.tag == "Mouse")
            {
                col.GetComponent<Mouse>().speedBoost = 0f;
            }
            else if (catSpeedModifier != 0f)
            {
                col.GetComponent<Cat>().speedBoost = 0f;
            }
        }
    }
}
