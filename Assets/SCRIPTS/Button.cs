using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {

    public bool active;
    private Renderer _rend;

    void Start()
    {
        _rend = GetComponent<Renderer>();
    }

    public void Activate()
    {
        active = true;
        _rend.material.color = Color.red;
        GameManager.Instance.CheckButton();
    }
}
