using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.NativePlugins;
using UnityEngine.UI;


public class ScreenManager : MonoBehaviour {
	public GameObject homeScreen, editorScreen, playerScreen;
	public GameObject currentScreen, afterLoadingDoneScreen;
	public GUILogic guiLogic;
	// Use this for initialization
	void Start () {
		Debug.Log ("<< SCREEN MGR Started >>");
		guiLogic = GameObject.FindWithTag("MainCamera").GetComponent<GUILogic>();
		initScreenUI();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void initScreenUI (){
		// this method goes over all screen containers - make sure only home is enabled - regardless of how you left it in Editor
		Debug.Log("<<Init Screen UI method started>>");
		editorScreen.SetActive(false);
		playerScreen.SetActive(false);
		homeScreen.SetActive(true);
		currentScreen = homeScreen;
		Debug.Log ("currentScreen is: "+currentScreen);
		guiLogic.modalWindow.SetActive(false);
		//guiLogic.loadingAnimationPanel.SetActive(false);
	}

	public void changeScreen (GameObject screen){
		Debug.Log("<<< changeScreen func started >>>");
		Debug.Log("current screen BEFORE: " + currentScreen.name);
		
		homeScreen.SetActive(screen==homeScreen ? true : false);
		playerScreen.SetActive(screen==homeScreen ? false : true);
		editorScreen.SetActive(screen==editorScreen ? true : false);
		currentScreen=screen;
		Debug.Log("current screen AFTER: " + currentScreen.name);
	
		guiLogic.SendMessage("initPageDisplay");
		if (currentScreen==homeScreen) guiLogic.SendMessage("Start");

	}

	
}
