using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interrupter : MonoBehaviour {

    public bool active;
    public int life;
    protected Renderer _rend;

    private int _life;

    protected virtual void Start()
    {
        _life = life;
        _rend = GetComponent<Renderer>();
    }

    virtual public void Activate()
    {
        _life--;
        if (_life <= 0)
        {
            active = true;
            _rend.material.color = Color.red;
            GameManager.Instance.CheckButton();
        }
    }
}
