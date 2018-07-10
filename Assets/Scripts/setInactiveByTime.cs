using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setInactiveByTime : MonoBehaviour {
	public float timeToKeepActive;
	float timer=0f;
	// Use this for initialization
	void Start () {
		this.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		timer = this.gameObject.activeSelf ? timer + Time.deltaTime : 0;
		if (timer >= timeToKeepActive) {
			timer = 0f;
			this.gameObject.SetActive (false);
		}
	}
		
}
