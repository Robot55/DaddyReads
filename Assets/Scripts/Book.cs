using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SinglePage{ 
	public Texture2D texture;
	public AudioClip clip;
}

public class Book : MonoBehaviour {

	public static Book book;
    public AudioSource curAudio;
	public List<SinglePage> pages = new List<SinglePage>();
    

	// Use this for initialization
	void Awake () {

		if (book == null) {
			DontDestroyOnLoad (gameObject);
			book = this;
		}
		else if (book !=this)
		{
			Destroy (gameObject);
		}
	}

	void Start () {
        curAudio = GetComponent<AudioSource>();
	}

	

}