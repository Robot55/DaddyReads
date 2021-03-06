
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using VoxelBusters.NativePlugins;
using System.Linq;




public class GUILogic : MonoBehaviour {
	//make public button vars and populate them in Inspector. used for changing functionality (addListener)
	public Button recAudioButton, playButton, attachAudioToPageButton, playButtonOnImage, daddyButton, takePhotoButton;
	public string currentBookFileName = "UserBook";
	List<string> allPlayerFiles = new List<string>();
	public List<AudioClip> guiSoundFX = new List<AudioClip> ();
	public int currentBookTotalPages;
	public AudioSource tmpAudio ;
	private AudioSource sfx;
	public Book screenBook;
	public Sprite playBtnSprite, stopBtnSprite;
	public Image bookPageDisplayImage;
	public ScreenManager mainCanvas;
	public Texture2D newPageTexture;
	public Sprite nextPageButtonTextureNormal, nextPageButtonTextureAddPage;
	public GameObject fileNameButtonPrefab, fileListContainer, newBookPrefab, nextPageButton, prevPageButton, modalWindow;
	public GameObject onScreenMessageTextInScene, onScreenMessageContainer, kidModeBackground, daddyModeBackground;
	public GameObject photoButtonContainer, recAudioButtonContainer, keypadInputText, kidProofRiddle, loadingAnimationPanel, createNewBookContainer;
	public int pageIndex = 0;
	public int kidProofRiddleValue = 99;
	public string demoBookUpdateFilename;


	public float mobilePhotoResolution; // set in inspector

	public bool pageAudioPlayed=false, editorMode=false, loadInBackground=false, creatingNewBook=false, demoBookUpdateChecked=false;
	SaveManager savemanager;

	void demoBookUpdate (string fileToCheck){
		Debug.Log ("demoBookUpdate : Started : Checking to see if update file exists");
		if (!ES3.FileExists (fileToCheck)) { // if updateFile does NOT exist
			Debug.Log ("demoBook " + fileToCheck + " does NOT Exist. <color=red>PERFORMING UPDATE</color>");
			//Do a complete save of current screenBook to demoBook File
			currentBookFileName = "UserBook000";
			completeBookSave ();
			//delete legacy demoBook user if exists
			if (ES3.DirectoryExists ("UserBook00")){
				Debug.Log ("Legacy book exists - deleteing");
				deleteSavedBook ("UserBook00");
			}
			//create updateFile on disk so it doesn't run again
			ES2.Save ("123", fileToCheck);

		} else {
			Debug.Log ("demoBook " + fileToCheck + " already exist. <color=green>NOT PERFORMING UPDATE</color>");
		}
	}
	public void Hi(){
		Debug.Log(" <color=green>@@@ Ding! Ding! Ding! @@@</color>");
	}

	void Awake(){
		Debug.Log("<<< Awake method started >>>");
		savemanager = this.gameObject.AddComponent<SaveManager>();
		Debug.Log ("finding SFX audioSource. before: " + sfx);
		sfx = GameObject.FindWithTag ("SFX").GetComponent<AudioSource> ();
		sfx.clip = guiSoundFX [0];
		Debug.Log ("finding SFX audioSource. after: " + sfx);

	}
		
	void Start () {
		Debug.Log("<< GUILogic: Start(): Started");
		loadingAnimationPanel.SetActive (true);
		creatingNewBook = false;
		Debug.Log("current pageIndex is: " + pageIndex + ". resetting to 0");
		pageIndex = 0;
		if (screenBook != null){
			Debug.Log("book exists: " + screenBook);	
			if (demoBookUpdateChecked == false) {
				demoBookUpdateChecked = true;
				demoBookUpdate (demoBookUpdateFilename);
			}
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
			//setNextPageButton ();

		}
		
		
	}

	
// ###### SECTION START: UPDATE FUNCTION METHODS ##########

	void drawSprite () {
		Sprite tmpSprite;
		Texture2D tex;
		if(screenBook.pages[pageIndex].texture==null){
			Debug.LogWarning("page ("+ pageIndex +") doesn't have texture. showing PlaceHolder");
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
			if (screenBook.pages.Count-1 == pageIndex && !loadInBackground){ // if this is last page
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
		addClickSoundToButton (photoButtonContainer.GetComponentInChildren<Button>(), guiSoundFX [0]);
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
		//Debug.Log ("screenBook Length: " + screenBook.pages.Count + " pageIndex: " + pageIndex);
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
			addClickSoundToButton (recAudioButton, guiSoundFX [0]);
			recAudioButton.onClick.AddListener(recordAudioStop);
			//set normal color to RED
			cb.normalColor=Color.red;
		} else {
			// button should function as "start recording"
			recAudioButtonContainer.GetComponentInChildren<Text>().text=screenBook.pages[pageIndex].clip==null? "Narrate this page" : "Re-Record Narration";
			recAudioButton.onClick.RemoveAllListeners();
			//addClickSoundToButton (recAudioButton, guiSoundFX [0]);
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
				addClickSoundToButton (playButton, guiSoundFX [0]);
				playButton.onClick.AddListener(stopPlayback);
			} else { // If No - make Play button
				//playButton.GetComponentInChildren<Text>().text = "Play";
				playButton.onClick.RemoveAllListeners();
				addClickSoundToButton (playButton, guiSoundFX [0]);
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
		Debug.Log ("davedave pageIndex is: " + pageIndex);
		if (screenBook.pages[pageIndex].clip==null) return;
		if (pageAudioPlayed==true) return;
		if (screenBook.curAudio.isPlaying) return;
		playPageAudio();


	}
	void setPlayPageAudioState (){
		//Debug.Log (screenBook.pages [pageIndex].clip == null ? "page: " + pageIndex + " clip is null" : "clip load status: " + screenBook.pages [pageIndex].clip.loadState);
		if (screenBook.pages[pageIndex].clip != null){
			playButtonOnImage.gameObject.SetActive(true);
			if (screenBook.curAudio.isPlaying) {	
				// If yes - make a Stop Button
				playButtonOnImage.image.sprite=stopBtnSprite;	
				playButtonOnImage.onClick.RemoveAllListeners();
				addClickSoundToButton (playButtonOnImage, guiSoundFX [0]);
				playButtonOnImage.onClick.AddListener(stopPageAudio);
			} else { // auio currently NOT playing - make a play button
				playButtonOnImage.image.sprite=playBtnSprite;	
				playButtonOnImage.onClick.RemoveAllListeners();
				addClickSoundToButton (playButtonOnImage, guiSoundFX [0]);
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
	private void PickImageFinished (ePickImageFinishReason reason, Texture2D newTexture){
		Debug.Log ("<<< PickImageFinished: Started");
		Debug.Log("Reason = " + reason);
		Debug.Log(newTexture!=null ? "Texture = " + newTexture : "Texture is Null");

		//Error Handling
		if (reason == ePickImageFinishReason.SELECTED) { // If all is OK do stuff
			Debug.Log ("SUCCESS: PickImageFinished returned SELECTED. newTexture is: " + newTexture);
			screenBook.pages [pageIndex].texture = newTexture;
			Debug.Log ("saving photo to device");
			savemanager.savePageImage (screenBook.pages [pageIndex].texture, currentBookFileName, pageIndex, "");
		} else {
			if (reason == ePickImageFinishReason.CANCELLED) { // If user cancelled get image photo
				Debug.Log ("USER CANCELLED: PickImageFinished returned CANCELLED. ScreenBook texture is: " + screenBook.pages [pageIndex].texture);
				Debug.Log ("Doing nothing");
			} else {
				if (reason == ePickImageFinishReason.FAILED) { // If user cancelled get image photo
					Debug.Log ("FAILIURE: PickImageFinished returned FAILED. newTexture is: " + newTexture != null ? newTexture.ToString () : "Null");
					Debug.Log ("Doing nothing");
				}
			}
		}


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
		//Flip createNewBook visibility
		createNewBookContainer.SetActive(editorMode ? true : false);
		//Flip Kid mode background visibility
		kidModeBackground.SetActive(!kidModeBackground.activeInHierarchy);
		//Flip daddy mode background visibility
		daddyModeBackground.SetActive(!daddyModeBackground.activeInHierarchy);
		//Rebuild BookFile Buttons if this is the homeScreen
		if (mainCanvas.currentScreen==mainCanvas.homeScreen){
			
			createBookButtonList();
		}
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

	public void pauseNextPageButton(float pauseLength){
		StartCoroutine (pauseNextPageButtonCR (pauseLength));
	}

	IEnumerator pauseNextPageButtonCR (float pauseLength){
		Debug.Log ("pauseNextPageButtonCR: started: going to wait for " + pauseLength + " seconds.");
		yield return new WaitForSeconds (pauseLength);
		Debug.Log ("Waiting done. making nextPage button interactable again");
		nextPageButton.GetComponent<Button> ().interactable = true;
		setNextPageButton ();
	}

	void addClickSoundToButton (Button button, AudioClip clip){

//		delegate{this.SendMessage(yesButtonMethodName, yesButtonMethodParameter);}
		button.onClick.AddListener(delegate {sfx.PlayOneShot(clip);});
	}
	void setSmartLoaderState(int pageIndex){
		if (creatingNewBook)
			return;
		int pagesInDisk = currentBookTotalPages;
		int pagesLoaded = screenBook.pages.Count;
		Debug.Log ("<<< setSmartLoaderState: started");
		if (pagesInDisk > 0 && pagesInDisk == pagesLoaded) { // if book on disk and book on screen has equal # of pages
			//this means all pages are already loaded to the screenBook.
			// so do nothing. return
			Debug.Log("All book pages are loaded to screenBook");
			return;
		}
		Debug.Log ("total # of Book pages on disk: " + currentBookTotalPages + " | total # of Book pages in GUI screenBook: " + screenBook.pages.Count);
		if (pagesInDisk > pagesLoaded) {
			//there are more pages to load, but not necessarily the next one
			if (pageIndex + 1 == pagesLoaded) {
				// current page is the last loaded page. safe to load more!
				loadInBackground = true;
				StartCoroutine (singlePageLoadCR (pageIndex + 1));
			} else {
				Debug.Log ("this page is already - but some unloaded pages still exist");
			}
		}
	}
	void initPageDisplay(){
		//initialize audio
		//if(tmpAudio.isPlaying) tmpAudio.Stop(); // stop any currently playing tmpAudio
		//if(screenBook.curAudio.isPlaying) screenBook.curAudio.Stop(); // stop any currently playing curAudio
		stopPlayback();
		stopPageAudio();
		pageAudioPlayed=false; // reset pageAudioPlayed bool state for auto play feature
		setAutoPlayAudioState(); // run se autoPlay audio state method
		// if in player od editor
		if (mainCanvas.currentScreen!=mainCanvas.homeScreen){
			setSmartLoaderState (pageIndex);
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
			//Debug.Log("<color=green>Do something when tempClip stopped recording and exists!</color>");
			attachCurrentRecording();
			savemanager.savePageAudio (screenBook.pages[pageIndex].clip, currentBookFileName, pageIndex, "");		
		}
	}

	void recordAudioDelayed(){
		Invoke ("recordAudioRAW", sfx.clip.length);
	}
	public void recordAudio(){
		//recordAudioDelayed ();
		recordAudioRAW();
	}

	public void recordAudioRAW(){
		tmpAudio.clip = Microphone.Start (null, false, 60, 44100);
	}
	
	void AttachRecording (AudioClip recordedClip, int i) {
		
		screenBook.pages [i].clip = recordedClip; // Attaches Recording to specific texture/Photo
	}

	GameObject findBookFileButton (string fileName){
		foreach (Transform child in fileListContainer.transform) {
			foreach (Text text in child.GetComponentsInChildren<Text>()) {
				if (text.transform.parent.name == "fileNameButton") {
					Debug.Log ("found the correct text object!");
					if (text.text == fileName) {
						Debug.Log ("found matching object text");
						return child.gameObject;
					}
				}
			}
		}
		Debug.Log ("GameObject not found for: " + fileName + ". returning Null");
		return null;
	}

	public void deleteSavedBook(string _name){
		Debug.Log("<<< deleteSavedBook : Started :");
		Debug.Log("Trying to DELETE book file: " + _name);
		string filePath = _name + "/";
		if (!ES2.Exists(filePath)){ //Handle error file not found
			Debug.LogWarning ("File: " + filePath + " can't be found");
		} else { //if no errors
			ES2.Delete(filePath);
			Debug.Log("File: " + _name + " deleted");
			// Find the GUI object for this file and destroy it.
			Destroy (findBookFileButton (_name));
			// update the AllPlayerFiles list
			getBookFiles();
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
						if (btn.name.Contains("NoButton")){ 
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
			Debug.Log("Already in editor mode. Toggling to normal mode - no kidProof riddle required");
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


	IEnumerator destroyNestedChildrenCR (Transform parent){
		foreach (Transform child in parent) {
			Debug.Log ("destroying object: " + child.gameObject.name);
			Destroy (child.gameObject);
		}
		yield return null;

	}

	IEnumerator createBookButtonListCR(){
		float tt = Time.realtimeSinceStartup;
		Debug.Log ("<<< createBookButtonListCR : Coroutine Started at :" + (Time.realtimeSinceStartup - tt));
		loadingAnimationPanel.SetActive (true);

		Debug.Log ("allPlayerFiles: " + allPlayerFiles.Count.ToString ());
		if (allPlayerFiles.Count==0) {
			Debug.Log("no saved files. count is zero");
			yield break;
		}
		//delete all buttons (so you can "redraw" this every time w/o multiple buttons)
		foreach (Transform child in fileListContainer.transform) {
			Debug.Log ("destroying object: " + child.gameObject.name);
			Destroy (child.gameObject);
		}
		foreach (string bookFileName in allPlayerFiles)
		{	
			GameObject go;
			go = Instantiate (fileNameButtonPrefab, fileListContainer.transform.position, fileListContainer.transform.rotation, fileListContainer.transform);
			Debug.Log ("instantiated new object: " + go.name);
			foreach (Transform t in go.transform){
				Debug.Log ("now looping on: " + t.name);
				Debug.Log ((t!=null) ? t.name + "is NOT null. parent is: " + t.parent.name: ">>>>>>>>> " +t.name + "WARNING: is NULL. parent is: " + t.parent.name);

				if(t.gameObject.name.Contains("fileNameButton")){

					Debug.Log("fileNameButton found. setting its <Text> to: " + bookFileName);
					t.gameObject.GetComponentInChildren<Text>().text = bookFileName;

					string imagePath = Application.persistentDataPath + "/" + bookFileName + "/" + "Page_000" + "/pagePhoto.png";
					WWW txLoader = new WWW ("file://" + imagePath);
					Debug.Log ("createBookButtonListCR : yielding for txLoader @" + (Time.realtimeSinceStartup - tt));
					yield return txLoader;
					Debug.Log ("createBookButtonListCR : resuming from txLoader yield  @" + (Time.realtimeSinceStartup - tt));

					// error handling
					if (!string.IsNullOrEmpty (txLoader.error)) {
						// handle error
						Debug.LogWarning ("some error loading the book cover texture: " + txLoader.error);
					}

					Texture2D tex=txLoader.texture;
					Sprite tmpSprite = null;
					tmpSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
					Debug.Log ((t!=null) ? "if(transform) reutrns not null. parent is: " + t.parent.name : ">>>>>>>>>> WARNING: if(transform) reutrns null. parent is: " + t.parent.name);
					//Debug.Log (" BUGHUNT: t. gameobject: " + t.gameObject!=null ? t.gameObject.name : "is null. and tmpSprite: " + tmpSprite!=null ? tmpSprite.name : "is null");
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
					Debug.Log("DeleteButton container found: " + t.name + " / " + t.gameObject.name);
					t.gameObject.SetActive (true);
					if(bookFileName=="UserBook000" || bookFileName=="UserBook00") {t.gameObject.SetActive(false);}; //if this is demo book hide delete option
					t.gameObject.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
					t.gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate{modalDeleteBook(bookFileName);});
					if (!editorMode) t.gameObject.SetActive(false);
				}
			}
		}
		loadingAnimationPanel.SetActive (false);

	}


	void createBookButtonList (){
		StartCoroutine (createBookButtonListCR ());
	}



	void loadAndEditBook (GameObject go) { //for onClick button
		StartCoroutine(loadAndEditBookCR(go));
	}
	void loadAndPlayBook (GameObject go) { //for onClick button
		StartCoroutine(loadAndPlayBookCR(go));
	}
	IEnumerator loadAndEditBookCR (GameObject go) {
		Debug.Log("<<< loadAndEditBookCR : Coroutine started:");
		Debug.Log("Text gameObject is: " + go.name);
		Debug.Log("found text: " + go.GetComponentInChildren<Text>().text);
		//set global filename for load/save
		currentBookFileName = go.GetComponentInChildren<Text>().text;
		//call load()
		yield return singlePageLoadCR(0);
		pageIndex = 0;
		//tell ui to change into editor mode
		mainCanvas.changeScreen(mainCanvas.editorScreen);
	}
	IEnumerator loadAndPlayBookCR (GameObject go) {
		Debug.Log("<<< loadAnPlayBookCR Coroutine started >>>");
		Debug.Log(" Text gameObject is: " + go.name);
		Debug.Log("found text: " + go.GetComponentInChildren<Text>().text);
		//set global filename for load/save
		currentBookFileName = go.GetComponentInChildren<Text>().text;
		//call load()
		yield return singlePageLoadCR(0);
		pageIndex = 0;
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

	public void completeBookSave(){
		Debug.Log("<< completeBookSave : Started : >>");
		//SaveManager saveManager = new SaveManager();
		Debug.Log("verifying book not empty");
		if (screenBook.pages.Count==0) {Debug.LogWarning("<color=red>Book is Empty. Save Cancelled</color>"); return;}
//		List<SinglePage> emptyPages = new List<SinglePage>();
		foreach (SinglePage page in screenBook.pages){
			if (page.texture==null) {
				Debug.Log("page has no texture - adding to bad list");
				screenBook.pages.Remove(page);
				}
		}
//		foreach (SinglePage page in screenBook.pages){
//			if (page.texture==null) {
//				Debug.Log("page has no texture - adding to bad list");
//				emptyPages.Add(page);
//			}
//		}
//		foreach (SinglePage page in emptyPages){
//			Debug.Log("page has no texture - removing it. texture= " +page.texture);
//			screenBook.pages.Remove(page);
//			//pageIndex= pageIndex>0 ? pageIndex-- : 0 ;
//		}
		if (screenBook.pages.Count==0) {Debug.LogWarning("<color=red>Book is Empty. Save Cancelled</color>"); return;}
		if (screenBook.pages.Count==0) Debug.LogError("You should NEVER see this line!");
		Debug.Log("Book not Empty. has: "+ screenBook.pages.Count.ToString());

		int stash = pageIndex;
		pageIndex = 0;
		foreach (SinglePage page in screenBook.pages){
			Debug.Log ("saving Loop: pageIndex : " + pageIndex + ". stash: " + stash);
			savemanager.savePageImage (page.texture, currentBookFileName, pageIndex, "result");
			if (page.clip != null) {
				savemanager.savePageAudio (page.clip, currentBookFileName, pageIndex, "result");
			} else {
				Debug.LogWarning ("No audio for page. Probably not recorded by user. saving anyway");
			}

			pageIndex++;

		}
		pageIndex = stash;
		Debug.Log ("saving Loop ended: pageIndex : " + pageIndex + ". stash: " + stash);

		//SavWav.Save("myweirdfile.wav", screenBook.pages[0].clip);
		Debug.Log ("file saved: " + currentBookFileName);
		Debug.Log ("<color=green>## Save Method completed ##</color>");
	}

	IEnumerator singlePageLoadCR(int pageIndex) {

		float t = Time.realtimeSinceStartup;

		if (!loadInBackground) loadingAnimationPanel.SetActive (true);

		Debug.Log ("about to load page: " + pageIndex + ". currentBook total pages: " + getBookPages (currentBookFileName).ToString ());
		if (pageIndex == 0) { // about to load page 0 of a book. first time loading in a book
			currentBookTotalPages = getBookPages(currentBookFileName); // check to see how many page folders are on DISK (tots # pages in saved book)
			screenBook.pages.Clear (); // clear screenBook
			screenBook.pages.TrimExcess ();
		
		}
		Debug.Log (">>>>>>> G" + (Time.realtimeSinceStartup - t));
		string imagePath = Application.persistentDataPath + "/" + currentBookFileName + "/" + "Page_" + pageIndex.ToString ("000") + "/pagePhoto.png";
		string audioPath = Application.persistentDataPath + "/" + currentBookFileName + "/" + "Page_" + pageIndex.ToString ("000") + "/pageAudio.wav";
		WWW txLoader = new WWW ("file://" + imagePath);
		WWW audioLoader = new WWW ("file://" + audioPath);
		bool txError = false, audioError=false;

		Debug.Log (">>>>>>> 0" + (Time.realtimeSinceStartup - t));
		yield return txLoader;
		Debug.Log (">>>>>>> 1 " + (Time.realtimeSinceStartup - t));
		yield return audioLoader;
		Debug.Log (">>>>>>> 2 " + (Time.realtimeSinceStartup - t));

		// error handling
		if (!string.IsNullOrEmpty (txLoader.error)) {
			// handle error
			Debug.LogWarning ("some error loading texture: " + txLoader.error.ToString());
			txError = true;
		}
		if (!string.IsNullOrEmpty (audioLoader.error)) {
			// handle error
			Debug.Log ("Audio NOT loaded");
			audioError = true;
		} else {
			// no errors from audio loading
		}

		Texture2D mytx = txError==false ? txLoader.texture : null;
		Debug.Log (">>>>>>> 3 " + (Time.realtimeSinceStartup - t));
		AudioClip myclip = audioError==false ? audioLoader.GetAudioClip (): null;
		Debug.Log (">>>>>>> 4 " + (Time.realtimeSinceStartup - t));

		SinglePage newPage = new SinglePage ();

		newPage.texture = mytx;
		newPage.clip = myclip;
		screenBook.pages.Add (newPage);


		loadingAnimationPanel.SetActive(false);
		loadInBackground = false;
		setNextPageButton ();
		Debug.Log (">>>>>>> 5 " + (Time.realtimeSinceStartup - t));
	}

	public Texture2D loadTitleTexture(string bookFileName){
		Debug.Log("<< Load Texture Method Began >>");
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/" + bookFileName, FileMode.Open);
		BookData bookData = (BookData)bf.Deserialize (file);
		file.Close ();
		Debug.Log ("## Load Texture Method completed ##");
		return deserializePhoto(bookData.bookTitlePhotoData);
	}

	int getBookPages (string currentBookFileName){
		Debug.Log ("getBookPages : started");
		DirectoryInfo bookFolder = new DirectoryInfo (Path.Combine (Application.persistentDataPath, currentBookFileName));
		DirectoryInfo[] pagesInBook = bookFolder.GetDirectories ("Page*");
		Debug.Log ("pages in book: "+ pagesInBook.Length);
		if (pagesInBook.Length == 0) {
			//handlError
			Debug.LogWarning("seems this book folder has zero pages in it. ERROR");
		}
		return pagesInBook.Length;
	} 

	void getBookFiles(){
		Debug.Log("<< GetBookFiles Started >>");
		DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
		DirectoryInfo[] bookFolders = dir.GetDirectories ("UserBook*").OrderBy (p => p.FullName).ToArray();
		Debug.Log("bookFolders Count: "+ bookFolders.Length.ToString());
		if (bookFolders.Length==0){
			Debug.Log("no saved books in user device. saving demo Book now");
			currentBookFileName = "UserBook000";
			completeBookSave();
			bookFolders = dir.GetDirectories("UserBook*");
			Debug.Log("DirInfo Count: "+ bookFolders.Length.ToString());
		}
		Debug.Log("Player Saved File List Count at beginning: " + allPlayerFiles.Count);
		//clean the allPlayerFiles list so func can be used multiple time at runtime
		allPlayerFiles.Clear();
		allPlayerFiles.TrimExcess();
		Debug.Log("Player Saved File List Count after cleaning (should be 0): " + allPlayerFiles.Count);
		foreach (DirectoryInfo d in bookFolders)
		{
				Debug.Log("DirInfo File Full Name: " + d.FullName);
				Debug.Log("DirInfo File Name: " + d.Name);
				allPlayerFiles.Add(d.Name);
		}
		Debug.Log("Player Saved File List Count after populating: " + allPlayerFiles.Count);
	}
	public void backButtonClicked(){
		if(editorMode) toggleEditMode(); 
		mainCanvas.changeScreen(mainCanvas.homeScreen);
	}

	public string generateNewBookFileName (){
		Debug.Log ("<<< generateNewBookFileName : Started : ");
		if (allPlayerFiles.Count==0){
			Debug.Log ("no user book folders found. this shouldn't happen. ERROR!");
		}
		int highestNumber = 0;
		foreach (string bookFileName in allPlayerFiles) {
			string lastThreeDigits = bookFileName.Substring (bookFileName.Length - 3);
			Debug.Log ("filename is: " + bookFileName + ". Last three digits are: " + lastThreeDigits);
			int i;
			if (Int32.TryParse (lastThreeDigits, out i)) {
				//if nothing went wrong and integer is fine
				Debug.Log ("integer from ("+ lastThreeDigits    +") is OK. value = " + i);
				if (i > highestNumber) {
					Debug.Log ("higher number found: " + i);
					highestNumber = i;
				}
			} else {
				//integer from threeDigits string failed
				Debug.Log ("threeDigits PARSE error: (" + lastThreeDigits + ") interger from fileName string failed.");
				string lastTwoDigits = lastThreeDigits.Substring (lastThreeDigits.Length - 2);
				if (Int32.TryParse (lastTwoDigits, out i)) {
					//if nothing went wrong and integer is fine
					Debug.Log ("integer from filename is OK. value = " + i);
					if (i > highestNumber) {
						Debug.Log ("higher number found: " + i);
						highestNumber = i;
					}
				} else { // TwoDigitis int form string failed
					Debug.Log ("lastTwoDigits PARSE error: (" + lastTwoDigits + ") interger from fileName string failed.");

				}
			}
		}
		highestNumber++;
		Debug.Log ("setting new book number to: " + highestNumber);
		return "UserBook"+highestNumber.ToString("000");
	}
	public void createNewBook(){
		// currentBook should be newBookPrefab
		creatingNewBook=true;
		GameObject newBook= Instantiate(newBookPrefab,this.transform.position,this.transform.rotation,this.transform);
		Debug.Log ("screenbook tag: " + screenBook.tag);
		Destroy(screenBook.gameObject);
		pageIndex = 0;
		screenBook = newBook.GetComponent<Book>();
		screenBook.tag = "BOOK";
		Debug.Log ("screenbook tag: " + screenBook.tag);
		// set filename to new file
		currentBookFileName = generateNewBookFileName ();
		//change to editor mode BEFORE toggling editorMode boolean
		mainCanvas.changeScreen(mainCanvas.editorScreen);
		if (!editorMode) toggleEditMode();
		//save();
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
