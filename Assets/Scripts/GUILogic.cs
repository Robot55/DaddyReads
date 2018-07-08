using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using VoxelBusters.NativePlugins;




public class GUILogic : MonoBehaviour {
	//make public button vars and populate them in Inspector. used for changing functionality (addListener)
	public Button recAudioButton, playButton, attachAudioToPageButton, playButtonOnImage;
	public string currentBookFileName = "PlayerInfo.dat";
	List<string> allPlayerFiles = new List<string>();
	AudioSource tmpAudio ;
	public Book screenBook;
	public Sprite playBtnSprite, stopBtnSprite;
	public Image bookPageDisplayImage;
	public ScreenManager mainCanvas;
	public Texture2D newPageTexture;
	public GameObject fileNameButtonPrefab, fileListContainer, newBookPrefab, nextPageText;
	public int pageIndex = 0;

	void Start () {
		Debug.Log("<< GUILogic Start() Begun >>");
		Debug.Log("current pageIndex is: " + pageIndex + ". resetting to 0");
		pageIndex = 0;
		if (screenBook != null){
			Debug.Log("book exists: " + screenBook);	
			tmpAudio =  tmpAudio==null ? GetComponent<AudioSource> () : tmpAudio;
			//bookPageDisplayImage = GameObject.FindWithTag("pagePlaceHolder").GetComponent<Image>();
			//Debug.Log("bookPageDisplayImage found: " + bookPageDisplayImage);
			mainCanvas = mainCanvas==null ? GameObject.FindWithTag("mainCanvas").GetComponent<ScreenManager>() : mainCanvas;
			Debug.Log("mainCanvas found: " + mainCanvas + " | " + mainCanvas.gameObject.name);
			Debug.Log("mainCanvas found: " + mainCanvas);
			getBookFiles();
			createBookButtonList();
		} else { // if screenBook = null
			Debug.Log("screenBook=null trying to fetch");
			screenBook = GameObject.FindWithTag("BOOK").GetComponent<Book>();
			Debug.Log(screenBook!=null ? "book script found: "+ screenBook.gameObject.name : "Error: main Book script can't be found");
			if (screenBook!=null) Start();
		}
	}

	void Update () {
		/*if (mainCanvas.currentScreen==mainCanvas.editorScreen && mainCanvas.currentScreen.activeInHierarchy==true){
			//if current screen is EDITOR and is active
			drawSprite();
			setRecordButtonState();
			setPlayButtonState();
			setAttachAudioButtonState();
			setPlayPageAudioState();
		}*/

		if (mainCanvas.currentScreen!=mainCanvas.homeScreen && mainCanvas.currentScreen.activeInHierarchy==true){
			//if current screen is not HOME (I.E. Player OR Editor) and is active
			drawSprite();
			setRecordButtonState();
			setPlayButtonState();
			setAttachAudioButtonState();
			setPlayPageAudioState();
			setNextPageText();
		}
		
		
	}

	
// ###### SECTION START: UPDATE FUNCTION METHODS ##########

	void drawSprite () {
		Sprite tmpSprite;
		Texture2D tex=screenBook.pages [pageIndex].texture;
		tmpSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
		bookPageDisplayImage.sprite=tmpSprite;
	}
	void setNextPageText(){
		if (mainCanvas.currentScreen==mainCanvas.playerScreen) return;
		nextPageText.GetComponent<Text>().text = screenBook.pages.Count-1 == pageIndex ? "+ ADD NEW PAGE" : " Next";
	
	}

	void setRecordButtonState(){
		if (mainCanvas.currentScreen==mainCanvas.playerScreen) return;
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
	void setPlayButtonState(){
		if (mainCanvas.currentScreen==mainCanvas.playerScreen) return;
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
	void setAttachAudioButtonState(){
		if (mainCanvas.currentScreen==mainCanvas.playerScreen) return;
		if (tmpAudio.clip != null && !Microphone.IsRecording(null)) { //if audio cip exists AND Mic is NOT currently recording
			//show attach button
			attachAudioToPageButton.interactable=true;
			attachAudioToPageButton.onClick.RemoveAllListeners();
			attachAudioToPageButton.onClick.AddListener(attachCurrentRecording);
		} else {
			attachAudioToPageButton.interactable=false;
		}
	}

	void setPlayPageAudioState (){
		if (screenBook.pages[pageIndex].clip != null){
			playButtonOnImage.gameObject.SetActive(true);
			if (screenBook.curAudio.isPlaying) {	
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
// ###### SECTION END: UPDATE FUNCTION METHODS ##########

// ###### SECTION START: UI BUTTONS ONCLICK METHODS ##########
	public void PickPhoto(){
		// Set popover to last touch position on iOS. This has no effect on Android.
		NPBinding.UI.SetPopoverPointAtLastTouchPosition();
		// Pick image
		NPBinding.MediaLibrary.PickImage(eImageSource.BOTH, 0.5f, PickImageFinished);
	}

	//Callback
	private void PickImageFinished (ePickImageFinishReason _reason, Texture2D _image){
		Debug.Log("Reason = " + _reason);
		Debug.Log("Texture = " + _image);
		screenBook.pages[pageIndex].texture = _image;
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
	void playPageAudio(){
		screenBook.curAudio.clip = screenBook.pages[pageIndex].clip;
		screenBook.curAudio.Play();
	}
	void stopPageAudio(){
		screenBook.curAudio.Stop ();
	}
	public void prevPage () {
		if (pageIndex > 0) {
				pageIndex--;
				//texture = screenBook.pages [pageIndex].texture;
			}
	}
	public void nextPage () {
		if (pageIndex < screenBook.pages.Count - 1) {
				pageIndex++;
				//texture = screenBook.pages [pageIndex].texture;
			} else { //add another page if in EDITOR mode
				if (mainCanvas.currentScreen==mainCanvas.editorScreen){
					SinglePage newPage = new SinglePage();
					newPage.texture=newPageTexture;
					screenBook.pages.Add(newPage);
					nextPage();
				}
			}
	}
	public void recordAudioStop(){
		EndRecording (tmpAudio, null);
	}
	public void recordAudio(){
		tmpAudio.clip = Microphone.Start (null, false, 60, 44100);
	}
	void AttachRecording (AudioClip recordedClip, int i) {
		
		screenBook.pages [i].clip = recordedClip; // Attaches Recording to specific texture/Photo
	}

	

	void createBookButtonList (){
		if (allPlayerFiles.Count==0) {
			Debug.Log("no saved files. count is zero");
			return;
		}
		//delete all buttons (so you can "redraw" this every time w/o multiple buttons)
		foreach(Transform transform in fileListContainer.gameObject.transform)
		{
			Destroy(transform.gameObject);
		}
		foreach (string _name in allPlayerFiles)
		{	
			GameObject go;
			go = Instantiate (fileNameButtonPrefab, fileListContainer.transform.position, fileListContainer.transform.rotation, fileListContainer.transform);
			go.GetComponentInChildren<Text>().text = _name;
			Sprite tmpSprite;
			Texture2D tex=loadTitleTexture(_name);
			tmpSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
			foreach (Transform t in go.transform){
				if (t.transform.parent.transform==go.transform && t.gameObject.GetComponent<Image>()!=null){
					t.gameObject.GetComponent<Image>().sprite=tmpSprite;
				}

			}
			go.GetComponent<Button>().onClick.AddListener(delegate{loadAndPlayBook(go);});
			
		}
	}



	void loadAndEditBook (GameObject go) { //for onClick button
			Debug.Log("Button text: " + go.GetComponentInChildren<Text>().text);
			//set global filename for load/save
			currentBookFileName = go.GetComponentInChildren<Text>().text;
			//call load()
			load();
			//tell ui to change into editor mode
			mainCanvas.changeScreen(mainCanvas.editorScreen);
	}
	void loadAndPlayBook (GameObject go) { //for onClick button
			Debug.Log("<<< loadAnPlayBook func started >>>");
			Debug.Log("gameObject is: " + go.name);
			Debug.Log("Button text: " + go.GetComponentInChildren<Text>().text);
			//set global filename for load/save
			currentBookFileName = go.GetComponentInChildren<Text>().text;
			//call load()
			load();
			//tell ui to change into editor mode
			mainCanvas.changeScreen(mainCanvas.playerScreen);
	}


// ###### SECTION END: UI BUTTONS ONCLICK METHODS ##########

// ###### SECTION START: UTILITY FUNCTIONS ##########
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
		Debug.Log("<< Save Method Began >>");
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/" + currentBookFileName);
		BookData bookdata = new BookData ();
		foreach (SinglePage page in screenBook.pages){
			
			PageData data = new PageData();
			// check if audio.clip exist
			if (page.clip != null) {
				seralizeAudio (page.clip, out data.soundData, out data.clipName, out data.samples, out data.channels, out data.freq);
				Debug.Log ("Serialized AudioClip");
			} else {
				Debug.Log ("No AudioClip for this page. Probably not recorded by user");
			}
			data.photoData = page.texture.EncodeToPNG();
			bookdata._pages.Add(data);
		}
			bookdata.bookTitlePhotoData = bookdata._pages[0].photoData;
		bf.Serialize (file, bookdata);
		file.Close ();
		Debug.Log ("file saved: " + currentBookFileName);
		Debug.Log ("## Save Method completed ##");
	}
	public void load(){
		Debug.Log("<< Load Method Began >>");
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/" +currentBookFileName, FileMode.Open);
		BookData bookData = (BookData)bf.Deserialize (file);
		screenBook.pages.Clear();
		screenBook.bookTitleTexture = deserializePhoto (bookData.bookTitlePhotoData);
		for (int i=0; i < bookData._pages.Count; i++){
			PageData page = bookData._pages[i];
			SinglePage newPage = new SinglePage ();
			if (page.soundData != null){
				newPage.clip = deseralizeAudio (page.soundData, page.clipName, page.samples, page.channels, page.freq);
			}
			newPage.texture = deserializePhoto (page.photoData);
			screenBook.pages.Add (newPage);
		}
		file.Close ();
		Debug.Log ("## Load Method completed ##");
	}
	public Texture2D loadTitleTexture(string bookFileName){
		Debug.Log("<< Load Texture Method Began >>");
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/" +bookFileName, FileMode.Open);
		BookData bookData = (BookData)bf.Deserialize (file);
		file.Close ();
		Debug.Log ("## Load Texture Method completed ##");
		return deserializePhoto(bookData.bookTitlePhotoData);
	}
	void getBookFiles(){
		Debug.Log("<< GetBookFiles Started >>");
		DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
		FileInfo[] info = dir.GetFiles("Player*.*");
		Debug.Log("FileInfo Count: "+ info.Length.ToString());
		if (info.Length==0){
			Debug.Log("no saved files in user device. saving demo Book now");
			currentBookFileName = "PlayerInfo00.dat";
			save();
			info = dir.GetFiles("Player*.*");
			Debug.Log("FileInfo Count: "+ info.Length.ToString());
		}
		Debug.Log("Player Saved File List Count at beginning: " + allPlayerFiles.Count);
		//clean the allPlayerFiles list so func can be used multiple time at runtime
		allPlayerFiles.Clear();
		Debug.Log("Player Saved File List Count after cleaning (should be 0): " + allPlayerFiles.Count);
		foreach (FileInfo f in info)
		{
				Debug.Log("FileInfo File Full Name: " + f.FullName);
				Debug.Log("FileInfo File Name: " + f.Name);
				allPlayerFiles.Add(f.Name);
		}
		Debug.Log("Player Saved File List Count after populating: " + allPlayerFiles.Count);
	}
	void createDemoBookAndSave (){
		currentBookFileName = "PlayerInfo00.dat";
		save();
	}
	public void createNewBookAndSave(){
		// currentBook should be newBookPrefab

		// **** FIX *** create new book on the fly! see if that works!
		screenBook = newBookPrefab.GetComponent<Book>()!=null ? newBookPrefab.GetComponent<Book>() : screenBook;
		// set filename to FileInfo.count+1
		currentBookFileName = "PlayerInfo"+allPlayerFiles.Count.ToString("000")+".dat";
		// save
		save();
		mainCanvas.changeScreen(mainCanvas.editorScreen);
	}
// ###### SECTION END: UTILITY FUNCTIONS ##########
}


// ###### SECTION START: PAGE & BOOK DATA Classes ##########

[Serializable]
public class PageData {
	public float[] soundData;
	public string clipName;
	public int samples, channels, freq;
	public byte[] photoData;
}

[Serializable]
public class BookData {
	public byte[] bookTitlePhotoData;
	public List<PageData> _pages = new List<PageData>();
}
