using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GUILogic : MonoBehaviour {

	AudioSource tmpAudio ;
	Texture texture ;

	SpriteRenderer spr ;

	Sprite tmpSprite;
	SpriteRenderer sr;
	public int pageIndex = 0;
	public Color defCol; // doesnt need to be public once all color GUI stuff is ok

	void Start () {
		if (Book.book != null){
			Debug.Log("book exists!");	
			defCol = GUI.backgroundColor;
			texture = Book.book.pages[pageIndex].texture;
			tmpAudio = GetComponent<AudioSource> ();
			spr = GameObject.FindWithTag("pagePlaceHolder").GetComponent<SpriteRenderer>();
			
			Debug.Log("===1) " + spr);
		
		} else {
			Debug.Log("book script object doesn't exist in scene");
		}
	}

	void Update () {
	//	tmpSprite.texture=Book.book.pages[pageIndex].texture;
	//	bookContainer.sprite=tmpSprite;
		drawSprite();
		
	}

	void drawSprite () {
		Texture2D tex=Book.book.pages [pageIndex].texture;
		tmpSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
		spr.sprite=tmpSprite;
	}
	public void prevPage () {
		if (pageIndex > 0) {
				pageIndex--;
				texture = Book.book.pages [pageIndex].texture;
			}
	}

	public void nextPage () {
		if (pageIndex < Book.book.pages.Count - 1) {
				pageIndex++;
				texture = Book.book.pages [pageIndex].texture;
			}
	}
	void OnGUI()
	{
		if (Book.book == null){
			Debug.Log("Book.book can't be found");
			return;
		}
		if (GUI.Button (new Rect (10, 200, 50, 30), "Save")) {
			save ();
		}
		if (GUI.Button (new Rect (10, 240, 50, 30), "Load")) {
			load ();
		}

		//Draw the main texture (book page photo)
		//GUI.DrawTexture (new Rect ((Screen.width / 2) - 512, 80, 1024, 576), texture, ScaleMode.ScaleToFit, true, 0.0F); //TODO: fix magic numbers with relative vars
		if (GUI.Button (new Rect ((Screen.width / 2) - 522, 368, 50, 30), "Back")) {
			if (pageIndex > 0) {
				pageIndex--;
				texture = Book.book.pages [pageIndex].texture;
			}
		}
		if (GUI.Button (new Rect ((Screen.width / 2) + 512, 368, 50, 30), "Next")) {
			if (pageIndex < Book.book.pages.Count - 1) {
				pageIndex++;
				texture = Book.book.pages [pageIndex].texture;
			}
		}
        if (Book.book.pages[pageIndex].clip != null) {
            
			// Is recording currently being played?		
			if (Book.book.curAudio.isPlaying) {	
				// If yes - make a Stop Button
				if (GUI.Button (new Rect ((Screen.width / 2) -90, (Screen.height/2)+300, 180, 30), "Stop Page Audio")) {
					Book.book.curAudio.Stop ();
				} 
			} else {
				if (GUI.Button(new Rect((Screen.width / 2) -90, (Screen.height/2)+300, 180, 30), "Play Page Audio")){
					Book.book.curAudio.clip = Book.book.pages[pageIndex].clip;
					Book.book.curAudio.Play();
				}
			}
        }

            //=====================================================================
            //===============         Play / Stop Button         ==================
            //=====================================================================

            // does playable clip exist already?
            if (tmpAudio.clip != null) {
			// Is recording currently being played?		
				if (tmpAudio.isPlaying) {	
					// If yes - make a Stop Button
					if (GUI.Button (new Rect (10, 60, 50, 30), "Stop")) {
						tmpAudio.Stop ();
					} 
				} else {
					// If No - make Play button
					if (GUI.Button (new Rect (10, 60, 50, 30), "Play")) {
						if (Microphone.IsRecording (null)) {
							EndRecording (tmpAudio, null);	
						}
						tmpAudio.Play ();
					} 
				}
		} 

//=====================================================================
//===============    Record / End Recording Button   ==================
//=====================================================================

		// Is Currently Recording?
		if (Microphone.IsRecording (null)) {
			// If yes - make end recording button
			GUI.backgroundColor = Color.blue;
			if (GUI.Button (new Rect (10, 20, 120, 30), "End Recording")) {
				EndRecording (tmpAudio, null);
			}
		} else {
			// If no - make Record button
			GUI.backgroundColor = Color.red;
			if (GUI.Button (new Rect (10, 20, 50, 30), "Rec")) {
				tmpAudio.clip = Microphone.Start (null, false, 60, 44100);
			} 
				
		}

//=====================================================================
//=================    Save Audio Clip to Photo    ====================
//=====================================================================
		if (tmpAudio.clip != null && !Microphone.IsRecording(null)) {
			GUI.backgroundColor = Color.blue;
			if (GUI.Button (new Rect ((Screen.width / 2) - 90, 20, 180, 30), "Save Audio to This page")) {
                AttachRecording (tmpAudio.clip, pageIndex);
            }
		}
	}

	public void EndRecording (AudioSource audS, string deviceName) {
		//Capture the current clip data
		AudioClip recordedClip = audS.clip;
		var position = Microphone.GetPosition(deviceName);
		var soundData = new float[recordedClip.samples * recordedClip.channels];
		recordedClip.GetData (soundData, 0);
		//Create shortened array for the data that was used for recording
		var newData = new float[position * recordedClip.channels];
		//Copy the used samples to a new array
		for (int i = 0; i < newData.Length; i++) {
			newData[i] = soundData[i];
		}
		//One does not simply shorten an AudioClip,
		//    so we make a new one with the appropriate length
		var newClip = AudioClip.Create (recordedClip.name, position, recordedClip.channels, recordedClip.frequency, false);
		newClip.SetData (newData, 0);        //Give it the data from the old clip
		//Replace the old clip
		AudioClip.Destroy (recordedClip);
		audS.clip = newClip;   
	}

	void AttachRecording (AudioClip recordedClip, int i) {
		
		Book.book.pages [i].clip = recordedClip; // Attaches Recording to specific texture/Photo
	}

	public void seralizeAudio (AudioClip recordedClip, out float[] soundData, out string clipName, out int samples, out int channels, out int freq){
		soundData = new float[recordedClip.samples * recordedClip.channels];
		recordedClip.GetData (soundData, 0);
		clipName = recordedClip.name;
		channels = recordedClip.channels;
		samples = recordedClip.samples;
		freq = recordedClip.frequency;
	}

	public AudioClip deseralizeAudio (float[] soundData, string clipName, int samples, int channels, int freq){
		AudioClip clip = AudioClip.Create (clipName, samples, channels, freq, false);
		clip.SetData (soundData, 0);
		return clip;
	}

	public byte[] serializePhoto (Texture2D myTexture){
		byte[] bytes = myTexture.EncodeToPNG ();
		return bytes;
	}

	public Texture2D deserializePhoto (byte[] bytes){
		var texture = new Texture2D(1,1);
		texture.LoadImage(bytes);
		return texture;
	}

	public void save(){
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/PlayerInfo.dat");
		BookData bookdata = new BookData ();
		foreach (SinglePage page in Book.book.pages){
			PageData data = new PageData();
			// check if audio.clip exist
			if (page.clip != null) {
				seralizeAudio (page.clip, out data.soundData, out data.clipName, out data.samples, out data.channels, out data.freq);
			} else {
				Debug.Log ("AudioClip doesn't exist. Probably not recorded by user");
			}
			data.photoData = page.texture.EncodeToPNG() ;
			bookdata._book.Add(data);
		}
		bf.Serialize (file, bookdata);
		file.Close ();
	}
	public void load(){
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/PlayerInfo.dat", FileMode.Open);
		BookData bookData = (BookData)bf.Deserialize (file);
		Book.book.pages.Clear();
		for (int i=0; i < bookData._book.Count; i++){
			PageData page = bookData._book[i];
			SinglePage newPage = new SinglePage ();
			if (page.soundData != null){
				newPage.clip = deseralizeAudio (page.soundData, page.clipName, page.samples, page.channels, page.freq);
			}
			newPage.texture = deserializePhoto (page.photoData);
			Book.book.pages.Add (newPage);
		}
		file.Close ();
	}
}

[Serializable]
class PageData {
	public float[] soundData;
	public string clipName;
	public int samples, channels, freq;
	public byte[] photoData;
}

[Serializable]
class BookData {
	public List<PageData> _book = new List<PageData>();
}
