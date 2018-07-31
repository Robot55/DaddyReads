using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class simpleAnimation : MonoBehaviour {

	public Sprite[] sprites;
	public float secsPerFrame;
	private int spriteIndex = 0;
	private float deltaTime = 0.0f;
	Image mainImage;
	// Use this for initialization
	void Awake () {
		if (this.GetComponent<Image>() != null) {
			print ("Image found");
			mainImage = this.GetComponent<Image> ();
		} else {
			print ("Image not founf error");

		}
	}
	
	// Update is called once per frame
	void OnGUI () {
		//sum up deltatime
		deltaTime += Time.deltaTime;
		// if deltaTime gt secsPerFrame change sprite and zero it
		if (deltaTime >= secsPerFrame) {
			StartCoroutine (animate ());
		}
	}

	IEnumerator animate() {
		spriteIndex++;
		if (spriteIndex > sprites.Length - 1) {spriteIndex = 0;}
		mainImage.sprite = sprites [spriteIndex];
		deltaTime = 0.0f;
		yield break;
	}

}
