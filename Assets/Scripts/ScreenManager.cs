using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.NativePlugins;
using UnityEngine.UI;


public class ScreenManager : MonoBehaviour {
	public GameObject homeScreen, editorScreen, playerScreen;
	public GameObject currentScreen;
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
	}

	public void changeScreen (GameObject screen){
		homeScreen.SetActive(screen==homeScreen ? true : false);
		playerScreen.SetActive(screen==homeScreen ? false : true);
		editorScreen.SetActive(screen==editorScreen ? true : false);
		currentScreen=screen;
		guiLogic.SendMessage("stopPlayback");
	}

	
}
