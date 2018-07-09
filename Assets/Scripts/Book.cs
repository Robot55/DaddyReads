using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.NativePlugins;


[System.Serializable]
public class SinglePage{ 
	public Texture2D texture;
	public AudioClip clip;
}

public class Book : MonoBehaviour {
	public Texture2D bookTitleTexture;
    public AudioSource curAudio;
	public List<SinglePage> pages = new List<SinglePage>();
    

	// Use this for initialization

	void Awake () {
        curAudio = GetComponent<AudioSource>();
	}

	

}