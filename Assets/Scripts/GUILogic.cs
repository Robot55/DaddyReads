using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GUILogic : MonoBehaviour {
	public Button recAudioButton, playButton, attachAudioToPageButton, playButtonOnImage;

	public string bookFileName = "PlayerInfo.dat";
	AudioSource tmpAudio ;
	public Sprite playBtnSprite, stopBtnSprite;
	Texture texture ;
	Image image;
	//SpriteRenderer spr ;
	Sprite tmpSprite;
	public int pageIndex = 0;
	public Color defCol; // doesnt need to be public once all color GUI stuff is ok

	void Start () {
		if (Book.book != null){
			Debug.Log("book exists!");	
			defCol = GUI.backgroundColor;
			texture = Book.book.pages[pageIndex].texture;
			tmpAudio = GetComponent<AudioSource> ();
			//cnvsRenderer = GameObject.FindWithTag("pagePlaceHolder").GetComponent<Image>().canvasRenderer	;
			image = GameObject.FindWithTag("pagePlaceHolder").GetComponent<Image>();

		
		} else {
			Debug.Log("book script object doesn't exist in scene");
		}
	}

	void Update () {
		drawSprite();
		setRecordButtonState();
		setPlayButtonState();
		setAttachAudioButtonState();
		setPlayPageAudioState();
		
		
	}

	void setAttachAudioButtonState(){
		if (tmpAudio.clip != null && !Microphone.IsRecording(null)) { //if audio cip exists AND Mic is NOT currently recording
			//show attach button
			attachAudioToPageButton.interactable=true;
			attachAudioToPageButton.onClick.RemoveAllListeners();
			attachAudioToPageButton.onClick.AddListener(attachCurrentRecording);
		} else {
			attachAudioToPageButton.interactable=false;
		}
	}
	void setPlayButtonState(){
		ColorBlock cb;
		cb=playButton.colors;
		    
		if (tmpAudio.clip != null) {	// does playable clip exist already?
			playButton.interactable=true;	
			if (tmpAudio.isPlaying) {	// Is recording currently being played?
				// If yes - button should be a STOP playback button
				playButton.GetComponentInChildren<Text>().text = "Stop";
				playButton.onClick.RemoveAllListeners();
				playButton.onClick.AddListener(stopPlayback);
			} else { // If No - make Play button
				playButton.GetComponentInChildren<Text>().text = "Play";
				playButton.onClick.RemoveAllListeners();
				playButton.onClick.AddListener(playPlayback);
				
			}

		} else { // if no playable clip exists
			//hide button
			playButton.interactable=false;
		}	

	}
	void attachCurrentRecording(){
		AttachRecording(tmpAudio.clip, pageIndex);
	}
	void stopPlayback() {
		tmpAudio.Stop();
	}

	void playPlayback() {
		tmpAudio.Play();
	}
	void setRecordButtonState(){
		ColorBlock cb;
		cb=recAudioButton.colors;
		if (Microphone.IsRecording (null)) { //if Mic currently IS recording
			//button should function as "end recording button"
			recAudioButton.GetComponentInChildren<Text>().text="End Recording";
			recAudioButton.onClick.RemoveAllListeners();
			recAudioButton.onClick.AddListener(recordAudioStop);
			//set normal color to BLUE
			cb.normalColor=Color.blue;
		} else {
			// button should function as "start recording"
			recAudioButton.GetComponentInChildren<Text>().text="RECORD Audio";
			recAudioButton.onClick.RemoveAllListeners();
			recAudioButton.onClick.AddListener(recordAudio);
			//set normal color to RED
			cb.normalColor=Color.red;
		}
		cb.highlightedColor=cb.normalColor;
		recAudioButton.colors=cb;
	}

	void playPageAudio(){
		Book.book.curAudio.clip = Book.book.pages[pageIndex].clip;
		Book.book.curAudio.Play();
	}

	void stopPageAudio(){
		Book.book.curAudio.Stop ();
	}
	void setPlayPageAudioState (){
		if (Book.book.pages[pageIndex].clip != null){
			playButtonOnImage.gameObject.SetActive(true);
			if (Book.book.curAudio.isPlaying) {	
				// If yes - make a Stop Button
				playButtonOnImage.image.sprite=stopBtnSprite;	
				playButtonOnImage.onClick.RemoveAllListeners();
				playButtonOnImage.onClick.AddListener(stopPageAudio);
			} else { // auio currently NOT playing - make a play button
				playButtonOnImage.image.sprite=playBtnSprite;	
				playButtonOnImage.onClick.RemoveAllListeners();
				playButtonOnImage.onClick.AddListener(playPageAudio);
			}
		} else{ //if no audio clip for this page - don't display button at all
			playButtonOnImage.gameObject.SetActive(false);
		}
	}
	void drawSprite () {
		Texture2D tex=Book.book.pages [pageIndex].texture;
		tmpSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
		image.sprite=tmpSprite;
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


	public void recordAudioStop(){
		EndRecording (tmpAudio, null);
	}
	public void recordAudio(){
		tmpAudio.clip = Microphone.Start (null, false, 60, 44100);
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
		FileStream file = File.Create (Application.persistentDataPath + "/" + bookFileName);
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
		FileStream file = File.Open (Application.persistentDataPath + "/" +bookFileName, FileMode.Open);
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
