﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveBoard : MonoBehaviour {
    
	void OnEnable () {
        for(int i = 0; i < GameManager.Instance.playersCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
	}
}
