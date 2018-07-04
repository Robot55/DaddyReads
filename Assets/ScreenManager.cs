using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour {
	public GameObject homeScreen, editorScreen, playerScreen;
	public GameObject currentScreen;
	// Use this for initialization
	void Start () {
		Debug.Log ("<< SCREEN MGR Started >>");
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
		Debug.Log("<< changeScreen method started >>");
		Debug.Log("now disabling current screen: " + currentScreen + ". isActive: " + currentScreen.activeInHierarchy);
		currentScreen.SetActive(false);
		Debug.Log("current screen: " + currentScreen + " has been disabled. isActive: " +currentScreen.activeInHierarchy);
		screen.SetActive(true);
		currentScreen=screen;
		Debug.Log("new Current screen is: " + currentScreen + ". isActive: " +currentScreen.activeInHierarchy);
	}
}
