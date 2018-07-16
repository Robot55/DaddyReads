
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using VoxelBusters.NativePlugins;




public class GUILogic : MonoBehaviour {
	//make public button vars and populate them in Inspector. used for changing functionality (addListener)
	public Button recAudioButton, playButton, attachAudioToPageButton, playButtonOnImage, daddyButton, takePhotoButton;
	public string currentBookFileName = "PlayerBook";
	List<string> allPlayerFiles = new List<string>();
	public AudioSource tmpAudio ;
	public Book screenBook;
	public Sprite playBtnSprite, stopBtnSprite;
	public Image bookPageDisplayImage;
	public ScreenManager mainCanvas;
	public Texture2D newPageTexture;
	public Sprite nextPageButtonTextureNormal, nextPageButtonTextureAddPage;
	public GameObject fileNameButtonPrefab, fileListContainer, newBookPrefab, nextPageButton, prevPageButton, modalWindow;
	public GameObject onScreenMessageTextInScene, onScreenMessageContainer, kidModeBackground, daddyModeBackground;
	public GameObject photoButtonContainer, recAudioButtonContainer, keypadInputText, kidProofRiddle, loadingAnimationPanel;
	public int pageIndex = 0;
	public int kidProofRiddleValue = 99;

	public float mobilePhotoResolution; // set in inspector

	public bool pageAudioPlayed=false, editorMode=false;

	public void Hi(){
		Debug.Log(" <color=green>@@@ Ding! Ding! Ding! @@@</color>");
	}

	void Awake(){
		Debug.Log("<<< Awake method started >>>");
	}
	void Start () {
		//SaveManager saveManager = new SaveManager();
		bool tbool = SaveManager.test();
		Debug.Log(SaveManager.test().ToString());
		Debug.Log("tbool = " + tbool.ToString());
		Debug.Log("<< GUILogic Start() Begun >>");
		Debug.Log("current pageIndex is: " + pageIndex + ". resetting to 0");
		pageIndex = 0;
		if (screenBook != null){
			Debug.Log("book exists: " + screenBook);	
			tmpAudio =  tmpAudio==null ? GetComponent<AudioSource> () : tmpAudio;
			mainCanvas = mainCanvas==null ? GameObject.FindWithTag("mainCanvas").GetComponent<ScreenManager>() : mainCanvas;
			getBookFiles();
			createBookButtonList();
			setAutoPlayAudioState();
		} else { // if screenBook = null
			Debug.Log("screenBook=null trying to fetch");
			screenBook = GameObject.FindWithTag("BOOK").GetComponent<Book>();
			Debug.Log(screenBook!=null ? "book script found: "+ screenBook.gameObject.name : "Error: main Book script can't be found");
			if (screenBook!=null) Start();
		}
	}

	void Update () {
		if (mainCanvas.currentScreen==mainCanvas.editorScreen && mainCanvas.currentScreen.activeInHierarchy==true){
			//if current screen is EDITOR and is active
			setRecordButtonState();
		}

		if (mainCanvas.currentScreen!=mainCanvas.homeScreen && mainCanvas.currentScreen.activeInHierarchy==true){
			setPlayPageAudioState();

		}
		
		
	}

	
// ###### SECTION START: UPDATE FUNCTION METHODS ##########

	void drawSprite () {
		Sprite tmpSprite;
		Texture2D tex;
		if(screenBook.pages[pageIndex].texture==null){
			Debug.LogWarning("page doesn't have texture. showing PlaceHolder");
			tex=newPageTexture;

		} else{

		tex=screenBook.pages [pageIndex].texture;
		}
		tmpSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
		bookPageDisplayImage.sprite=tmpSprite;
		bookPageDisplayImage.type=Image.Type.Simple;
		bookPageDisplayImage.preserveAspect=true;
	}
	void setNextPageButton(){
		if (mainCanvas.currentScreen==mainCanvas.editorScreen){ //if in editor screen
			if (screenBook.pages.Count-1 == pageIndex){ // if this is last page
					if (screenBook.pages[pageIndex].texture==null){ //  and if page texture is still null
					nextPageButton.SetActive(false);
					} else { // if page texture no longer null
					nextPageButton.SetActive(true);
					nextPageButton.GetComponent<Button>().image.sprite=nextPageButtonTextureAddPage;

					}
				} else { //if this is not the last page
					nextPageButton.SetActive(true);
					nextPageButton.GetComponent<Button>().image.sprite=nextPageButtonTextureNormal;
				}	
		} else { //if not in editor screen
			if (mainCanvas.currentScreen==mainCanvas.playerScreen){ // if in player screen
				nextPageButton.GetComponent<Button>().image.sprite=nextPageButtonTextureNormal;
				if (screenBook.pages.Count-1 == pageIndex){ //if last page
					nextPageButton.SetActive(false);
				} else { //if not last page
					nextPageButton.SetActive(true);
				}	
			}
		}
	
	}
	void setPrevPageButton(){
	
		if(pageIndex==0) {
			prevPageButton.SetActive(false);
	 	} else {
			 prevPageButton.SetActive(true);
			 };
	}

	void setTakePhotoButtonState(){
		if (mainCanvas.currentScreen==mainCanvas.playerScreen) return;
		
		ColorBlock cb;
		cb=takePhotoButton.colors;
		Color transparentWhite= Color.white;
		transparentWhite.a=0.6f;
		cb.normalColor= screenBook.pages[pageIndex].texture==null ? Color.white : transparentWhite;
		takePhotoButton.colors=cb;
		photoButtonContainer.GetComponentInChildren<Text>().text= screenBook.pages[pageIndex].texture==null ? "Take Photo of Page" :"Re-Take Photo";

	}

	void setDeletePageButtonState(){
		if (mainCanvas.currentScreen==mainCanvas.playerScreen) return;
		
		ColorBlock cb;
		cb=takePhotoButton.colors;
		Color transparentWhite= Color.white;
		transparentWhite.a=0.6f;
		cb.normalColor= screenBook.pages[pageIndex].texture==null ? Color.white : transparentWhite;
		takePhotoButton.colors=cb;
		photoButtonContainer.GetComponentInChildren<Text>().text= screenBook.pages[pageIndex].texture==null ? "Take Photo of Page" :"Re-Take Photo";

	}

	void setRecordButtonState(){
		if (mainCanvas.currentScreen==mainCanvas.playerScreen) return;
		if (screenBook.pages[pageIndex].texture==null){
			recAudioButtonContainer.gameObject.SetActive(false);
			return;
		}
		recAudioButtonContainer.gameObject.SetActive(true);
		ColorBlock cb;
		cb=recAudioButton.colors;
		if (Microphone.IsRecording (null)) { //if Mic currently IS recording
			//button should function as "end recording button"
			recAudioButtonContainer.GetComponentInChildren<Text>().text="End Recording";
			recAudioButton.onClick.RemoveAllListeners();
			recAudioButton.onClick.AddListener(recordAudioStop);
			//set normal color to RED
			cb.normalColor=Color.red;
		} else {
			// button should function as "start recording"
			recAudioButtonContainer.GetComponentInChildren<Text>().text=screenBook.pages[pageIndex].clip==null? "Narrate this page" : "Re-Record Narration";
			recAudioButton.onClick.RemoveAllListeners();
			recAudioButton.onClick.AddListener(recordAudio);
			//set normal color to RED or HalfTransparentWHITE
			Color transparentWhite = Color.white;
			transparentWhite.a=0.6f;
			cb.normalColor= screenBook.pages[pageIndex].clip==null ? Color.white : transparentWhite;
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
				//playButton.GetComponentInChildren<Text>().text = "Stop";
				playButton.onClick.RemoveAllListeners();
				playButton.onClick.AddListener(stopPlayback);
			} else { // If No - make Play button
				//playButton.GetComponentInChildren<Text>().text = "Play";
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

	void setAutoPlayAudioState(){
		if (mainCanvas.currentScreen!=mainCanvas.playerScreen) return;
		if (screenBook.pages[pageIndex].clip==null) return;
		if (pageAudioPlayed==true) return;
		if (screenBook.curAudio.isPlaying) return;
		playPageAudio();


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
		Debug.Log ("<<< PickPhoto method started>>");
		// Set popover to last touch position on iOS. This has no effect on Android.
		NPBinding.UI.SetPopoverPointAtLastTouchPosition();
		// Pick image
		if (mobilePhotoResolution > 1.0f || mobilePhotoResolution < 0.2f) mobilePhotoResolution = 0.8f;
		NPBinding.MediaLibrary.PickImage(eImageSource.BOTH, mobilePhotoResolution, PickImageFinished);
		Debug.Log ("<<< PickPhoto method Ended>>");
	}

	//Callback
	private void PickImageFinished (ePickImageFinishReason _reason, Texture2D _image){
		Debug.Log("Reason = " + _reason);
		Debug.Log("Texture = " + _image);

		screenBook.pages[pageIndex].texture = smartScale(_image);
		initPageDisplay();
	}

	Texture2D smartScale(Texture2D _image){
		Debug.Log("attempting to rescale. BEFORE: width = " +_image.width.ToString() + " | height = " + _image.height.ToString());
		if (_image.width > _image.height){ // if landscape image
			if (_image.width <=1000) { // if width small enough - don't resize
				Debug.Log("image size already small. skipping resize");
				return _image;
			} else { //landscape image too big. resize
				int  scaleRatio = _image.width / 1000;
				MyTextureScale.Bilinear (_image, 1000, _image.height/scaleRatio);
				Debug.Log("rescale done. AFTER: width = " +_image.width.ToString() + " | AFTER = " + _image.height.ToString());
			}
			
			
		} else { // if not landscape image
			if (_image.width <= _image.height){ // if portrait   or square image
			if (_image.height <=1000) { // if height small enough - don't resize
				Debug.Log("image size already small. skipping resize");
				return _image;
			} else {//landscape image too big. resize
				int  scaleRatio = _image.height / 1000;
				MyTextureScale.Bilinear (_image, _image.width/scaleRatio, 1000);
				Debug.Log("rescale done. AFTER: width = " +_image.width.ToString() + " | AFTER = " + _image.height.ToString());
				}
			}
		}
		Debug.Log("Returning image. Size is: " +_image.width.ToString() + " X " + _image.height.ToString());
		return _image;
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
		pageAudioPlayed=true;
	}
	void stopPageAudio(){
		screenBook.curAudio.Stop ();
	}
	
	public void keypadEntry(GameObject go){
		keypadInputText.GetComponent<Text>().text += go.GetComponentInChildren<Text>().text;
		Debug.Log("Trying to add: " + go.GetComponentInChildren<Text>().text + " to input field");
	}
	
	public void toggleEditMode(){
		//first toggle editor mode
		editorMode = !editorMode; 
		//flip daddybutton icon vertically
		Quaternion newRotation = daddyButton.image.transform.rotation;
		newRotation.z= newRotation.z==0 ? 180 : 0;
		daddyButton.image.transform.rotation = newRotation;
		kidModeBackground.SetActive(!kidModeBackground.activeInHierarchy);
		daddyModeBackground.SetActive(!daddyModeBackground.activeInHierarchy);
		Start();
	}

	public void daddyButtonLocked(){
		onScreenMessageTextInScene.gameObject.SetActive(true);
	}
	public void daddyButtonUnlocked(){
		toggleEditMode();
	}
	public void prevPage () {
		if (pageIndex > 0) {
				pageIndex--;
				initPageDisplay();
			}
	}

	public void nextPage () {
		if (pageIndex < screenBook.pages.Count - 1) {
				pageIndex++;
				initPageDisplay();
				
				
				
				//texture = screenBook.pages [pageIndex].texture;
			} else { //add another page if in EDITOR mode
				if (mainCanvas.currentScreen==mainCanvas.editorScreen){
					SinglePage newPage = new SinglePage();
					newPage.texture=null;
					screenBook.pages.Add(newPage);
					nextPage();
				}
			}
	}

	void initPageDisplay(){
		if(tmpAudio.isPlaying) tmpAudio.Stop();
		if(screenBook.curAudio.isPlaying) screenBook.curAudio.Stop();
		stopPlayback();
		stopPageAudio();
		pageAudioPlayed=false;
		setAutoPlayAudioState();
			


		if (mainCanvas.currentScreen!=mainCanvas.homeScreen){
			drawSprite();
			setTakePhotoButtonState();
			setRecordButtonState();
			setNextPageButton();
			setPrevPageButton();
			}
	}
	public void recordAudioStop(){
		EndRecording (tmpAudio, null);
		if (tmpAudio.clip!=null){
			Debug.Log("<color=green>Do something when tempClip stopped recording and exists!</color>");
			attachCurrentRecording();
			
		}
	}
	public void recordAudio(){
		tmpAudio.clip = Microphone.Start (null, false, 60, 44100);
	}
	void AttachRecording (AudioClip recordedClip, int i) {
		
		screenBook.pages [i].clip = recordedClip; // Attaches Recording to specific texture/Photo
	}

	/*public void daddyButtonClicked(){
		if (onScreenMessageContainer.GetComponentsInChildren<Text>().Length>0) return;
		GameObject go=Instantiate(onScreenMessageTextPrefab, onScreenMessageContainer.transform.position, onScreenMessageContainer.transform.rotation, onScreenMessageContainer.transform);
	}*/	
	public void deleteSavedBook(string _name){
		Debug.Log("<<< deleteSavedBook Func started >>>");
		Debug.Log("Trying to DELETE book file: " + _name);
		string filePath = Application.persistentDataPath + "/" + _name;
		if (!File.Exists(filePath)){
			Debug.LogWarning ("File: " + filePath + " can't be found");
		} else {
			File.Delete(filePath);
			Debug.Log("File: " + _name + " deleted");
			Debug.Log("Refreshing by running Start()");
			Start();

		}
	}
	public void modalDeleteBook(string _name){
		callModalWindow("YesNoDeleteBookBox", "deleteSavedBook", _name);
	}
	public void callModalWindow(string modalPanelToShow, string yesButtonMethodName, string yesButtonMethodParameter){
		yesButtonMethodParameter = yesButtonMethodParameter==null ? "" : yesButtonMethodParameter;
		Debug.Log("<<< callModalWindow func started >>>");
		modalWindow.SetActive(true);
		foreach (Transform t in modalWindow.transform){
			if (t.gameObject.name.Contains(modalPanelToShow)) {
				Debug.Log("modal window found: " + modalPanelToShow);
				t.gameObject.SetActive(true);
				//get all buttons
				// if yes button add listener with delegate 
				foreach (Button btn in t.gameObject.transform.GetComponentsInChildren<Button>()){
					
					btn.onClick.RemoveAllListeners();
					if (btn.name.Contains("YesButton")){
						btn.onClick.AddListener(delegate{this.SendMessage(yesButtonMethodName, yesButtonMethodParameter);});
						btn.onClick.AddListener(closeModalWindow);

					} else {
						if (btn.name.Contains("YesButton")){ 
							//add the no button method here
							btn.onClick.AddListener(closeModalWindow);
						} else {
							if (btn.name.Contains("keyPadButton"))Debug.Log("Skipping keypadbutton"); 
						}
					}
					

				}
			} else { //not called panels are set to active=false
				t.gameObject.SetActive(false);
			}
		}
	}
	public void clearKeyPadInput(){
		keypadInputText.GetComponent<Text>().text="";
	}
	public void undoLastKeypadEntry(){
		string txt = keypadInputText.GetComponent<Text>().text;
		txt=txt.Length>0 ? txt.Remove(txt.Length-1): txt;
		keypadInputText.GetComponent<Text>().text=txt;

	}

	public void callKidProofModal(){
		if (editorMode) { //if already in editor mode - skip entire modal and toggle editor mode
			toggleEditMode();
			closeModalWindow();
			return;
		}
		keypadInputText.GetComponent<Text>().text="";
		generateKidProofRiddle();
		callModalWindow("KidProofPanel", "replyRiddleAttempt", kidProofRiddleValue.ToString());
	}
	void closeModalWindow(){
		modalWindow.SetActive(false);
	}
	void replyRiddleAttempt (string riddleString) {
		Debug.Log("<<<replyRiddleAttempt func started>>>");
		Debug.Log("checking if: "+ keypadInputText.GetComponent<Text>().text + " is equal to: "+ riddleString);
		if (keypadInputText.GetComponent<Text>().text==riddleString) {
			toggleEditMode();
		}
		closeModalWindow();
	}

	void generateKidProofRiddle(){
		int int1 =  UnityEngine.Random.Range(2,20);
		int int2 =  UnityEngine.Random.Range(2,20);
		kidProofRiddle.GetComponent<Text>().text= "How much is " +int1.ToString() + " + " + int2.ToString()+ "?";
		kidProofRiddleValue = int1+int2;

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
			
			foreach (Transform t in go.transform){
				if(t.gameObject.name.Contains("fileNameButton")){

					Debug.Log("Found the Big fileNameButton");
					t.gameObject.GetComponentInChildren<Text>().text = _name;
					Sprite tmpSprite;
					Texture2D tex=loadTitleTexture(_name);
					tmpSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
					t.gameObject.GetComponent<Image>().sprite=tmpSprite;
					t.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
					if (!editorMode) t.gameObject.GetComponent<Button>().onClick.AddListener(delegate{loadAndPlayBook(go);});
					if (editorMode) t.gameObject.GetComponent<Button>().onClick.AddListener(delegate{loadAndEditBook(go);});
					foreach (Image img in t.gameObject.GetComponentsInChildren<Image>()){
						if (img.gameObject.name.Contains("playButton") && editorMode) img.gameObject.SetActive(false);
						if (img.gameObject.name.Contains("editButton") && !editorMode) img.gameObject.SetActive(false);
					}

				}
				if(t.gameObject.name.Contains("DeleteButton_Containter")){
					if(_name.Contains("PlayerInfo00.dat")) {t.gameObject.SetActive(false);}; //if this is demo book hide delete option
					Debug.Log("Found the DeleteButton container");
					t.gameObject.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
					t.gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate{modalDeleteBook(_name);});
					if (!editorMode) t.gameObject.SetActive(false);
				}
			}


			
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
		Debug.Log("verifying book not empty");
		if (screenBook.pages.Count==0) {Debug.LogWarning("<color=red>Book is Empty. Save Cancelled</color>"); return;}
		List<SinglePage> emptyPages = new List<SinglePage>();
		foreach (SinglePage page in screenBook.pages){
			if (page.texture==null) {
				Debug.Log("page has no texture - adding to bad list");
				emptyPages.Add(page);
				}
		}
		foreach (SinglePage page in emptyPages){
			Debug.Log("page has no texture - removing it. texture= " +page.texture);
			screenBook.pages.Remove(page);
			//pageIndex= pageIndex>0 ? pageIndex-- : 0 ;
		}
		if (screenBook.pages.Count==0) {Debug.LogWarning("<color=red>Book is Empty. Save Cancelled</color>"); return;}
		if (screenBook.pages.Count==0) Debug.LogError("You should NEVER see this line!");
		Debug.Log("Book not Empty. has: "+ screenBook.pages.Count.ToString());
		string currentFolderName = currentBookFileName;
		if (!Directory.Exists(Application.persistentDataPath + "/" + currentFolderName)){
			Directory.CreateDirectory(Application.persistentDataPath + "/" + currentFolderName);
		}
		
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/" + currentBookFileName + ".dat");
		BookData bookdata = new BookData ();
		foreach (SinglePage page in screenBook.pages){
			
			PageData data = new PageData();
			// check if audio.clip exist
			//if (page.texture==null) {Debug.LogWarning("no photo for this page. skipping page"); return;}
			if (page.clip != null) {
				seralizeAudio (page.clip, out data.soundData, out data.clipName, out data.samples, out data.channels, out data.freq);
				Debug.Log ("Serialized AudioClip");
			} else {
				Debug.LogWarning ("No audio for page. Probably not recorded by user. saving anyway");
			}
			Debug.Log("page texture is: " + page.texture);
			data.photoData = page.texture.EncodeToPNG()!=null ? page.texture.EncodeToPNG() : page.texture.EncodeToJPG();
			bookdata._pages.Add(data);
		}
		bookdata.bookTitlePhotoData = bookdata._pages[0].photoData;
		bf.Serialize (file, bookdata);
		file.Close ();
		SavWav.Save("myweirdfile.wav", screenBook.pages[0].clip);
		Debug.Log ("file saved: " + currentBookFileName);
		Debug.Log ("<color=green>## Save Method completed ##</color>");
	}
	public void load(){
		Debug.Log("<< Load Method Began >>");
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/" +currentBookFileName, FileMode.Open);
		BookData bookData = (BookData)bf.Deserialize (file);
		screenBook.pages.Clear();
		screenBook.pages.TrimExcess();
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
			info = dir.GetFiles("Player*.dat");
			Debug.Log("FileInfo Count: "+ info.Length.ToString());
		}
		Debug.Log("Player Saved File List Count at beginning: " + allPlayerFiles.Count);
		//clean the allPlayerFiles list so func can be used multiple time at runtime
		allPlayerFiles.Clear();
		allPlayerFiles.TrimExcess();
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
		currentBookFileName = "PlayerBook00.dat";
		save();
	}
	public void backButtonClicked(){
		if (mainCanvas.currentScreen!=mainCanvas.editorScreen) {
			Debug.LogWarning("auto-save attempt denied");
			mainCanvas.changeScreen(mainCanvas.homeScreen);
			return;
			}
		Debug.LogWarning("Attempting auto-save on back button"); 
		save();  
		Debug.LogWarning("<color=green>auto-save complete</color>"); 
		if(editorMode) toggleEditMode(); 
		mainCanvas.changeScreen(mainCanvas.homeScreen);
	}
	public void createNewBook(){
		// currentBook should be newBookPrefab
		GameObject newBook= Instantiate(newBookPrefab,this.transform.position,this.transform.rotation,this.transform);
		Destroy(screenBook.gameObject);
		screenBook = newBook.GetComponent<Book>();
		// set filename to FileInfo.count+1
		currentBookFileName = "PlayerInfo"+allPlayerFiles.Count.ToString("000");
		// save
		if (!editorMode) toggleEditMode();
		//save();
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
