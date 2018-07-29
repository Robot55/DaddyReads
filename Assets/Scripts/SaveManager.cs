using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using VoxelBusters.NativePlugins;

public class SaveManager : MonoBehaviour {

	public SaveLoadWav wavSave = new SaveLoadWav ();
	void Awake () {
	}
	// Update is called once per frame
	void _onCompletion(bool onCompletion){
		Debug.Log("callBack Result: " +onCompletion.ToString());
	}
		
	public void savePageImage(Texture2D pageTexture, string currentBookFileName, int pageIndex, string _result) {
		Debug.Log ("<<< SaveMAnager.savePageImage started >>>");
		string bookFolderPath = currentBookFileName;
		string pageFolderPath = Path.Combine (bookFolderPath, "Page_" + pageIndex.ToString ("000"));
		string photoFileName = "pagePhoto";
		string pathToFile = Path.Combine (pageFolderPath, photoFileName);
		Debug.Log ("pathTofile is: " + pathToFile);
		ES2.Save (pageTexture, pathToFile);
	}

//	public void savePageAudio(AudioClip clip, string currentBookFileName, int pageIndex, string _result) {
//		Debug.Log ("<<< SaveMAnager.savePageAudio started >>>");
//		string bookFolderPath = currentBookFileName;
//		string pageFolderPath = Path.Combine (bookFolderPath, "Page_" + pageIndex.ToString ("000"));
//		string audioFileName = "pageAudio.R55";
//		string pathToFile = Path.Combine (pageFolderPath, audioFileName);
//		ES2.Save (clip, pathToFile);
//	}

	public void savePageAudio(AudioClip clip, string currentBookFileName, int pageIndex, string _result) {
		Debug.Log ("<<< SaveMAnager.savePageAudio started >>>");
		string bookFolderPath = currentBookFileName;
		string pageFolderPath = Path.Combine (bookFolderPath, "Page_" + pageIndex.ToString ("000"));
		string audioFileName = "pageAudio";
		string pathToFile = Path.Combine (pageFolderPath, audioFileName);
		wavSave.Save (pathToFile, clip);
		ES2.Save (clip, pathToFile);
	}
	public Texture2D loadPageImage(string currentBookFileName, int pageIndex){
		Debug.Log("<< Load Texture Method Began >>");
		string bookFolderPath = currentBookFileName;
		string pageFolderPath = Path.Combine (bookFolderPath, "Page_" + pageIndex.ToString ("000"));
		string photoFileName = "pagePhoto";
		string pathToFile = Path.Combine (pageFolderPath, photoFileName);
		Debug.Log ("pathToFile: " + pathToFile);
		if (ES2.Exists (pathToFile)) {
			Debug.Log ("## ES2.Load Begins ## Time: " + Time.time);
			return ES2.Load<Texture2D> (pathToFile);
		} else {
			Debug.Log ("Texture path doesn't exist: " + pathToFile);
			return null;
		}
	}

	public AudioClip loadPageAudio(string currentBookFileName, int pageIndex){
		Debug.Log("<< Load Audio Method Began >>");
		string bookFolderPath = currentBookFileName;
		string pageFolderPath = Path.Combine (bookFolderPath, "Page_" + pageIndex.ToString ("000"));
		string audioFileName = "pageAudio";
		string pathToFile = Path.Combine (pageFolderPath, audioFileName);
		Debug.Log ("pathToFile: " + pathToFile);
		if (ES2.Exists (pathToFile)) {
			Debug.Log ("## Load Audio Method completed ##");
			return ES2.Load<AudioClip> (pathToFile);
		} else {
			Debug.Log ("Audio-File path doesn't exist: " + pathToFile);
			return null;
		}
	}
}
	