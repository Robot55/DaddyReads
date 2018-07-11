using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.NativePlugins;

public class KidProofTimedButton : MonoBehaviour {
	public float holdDuration;
	GameObject objContainingMethodToRun, buttonObjectUsed;
	public string nameOfMethodToRun;
	bool timerRunning = false;
	float timer = 0f;

	// Use this for initialization
	void Start () {
		Debug.Log("<<< KidProof Btn Script Srarted >>>");
		buttonObjectUsed = GetComponent<Button>().gameObject;
		Debug.Log("Button object is: " + buttonObjectUsed.name);
		objContainingMethodToRun= GameObject.FindWithTag("MainCamera");

	}
	
	// Update is called once per frame
	void Update () {
		timer = timerRunning ? timer + Time.deltaTime : 0f;
		if (timer >= holdDuration) {
			stopTimer ();
			print ("DING!" + timer.ToString());
			objContainingMethodToRun.BroadcastMessage (nameOfMethodToRun.ToString(), buttonObjectUsed);
		}
	}

	public void startTimer (){
		timerRunning = true;
	}

	public void stopTimer () {
		timerRunning = false;
	}
}
