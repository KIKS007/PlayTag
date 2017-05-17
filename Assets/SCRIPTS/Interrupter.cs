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
        _rend.material.color = new Color((1f - ((float)_life / (float)life)/2f) , _rend.material.color.g, _rend.material.color.b);

        if (_life <= 0)
        {
            active = true;
            _rend.material.color = Color.red;
            GameManager.Instance.CheckButton();
        }
    }
}
