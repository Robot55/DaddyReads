using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using VoxelBusters.NativePlugins;

public class SaveManager : MonoBehaviour {

	private static string rootFolder;
	//private static string rootFolder = Application.persistentDataPath;
	

	public static bool test(){
		Debug.Log(" <color=green>@@@ Ding! SAVEMANAGER! Ding! @@@</color>");
		return true;
	}

	// Use this for initialization
	void Awake () {
		rootFolder = Application.persistentDataPath;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void _onCompletion(bool onCompletion){
		Debug.Log("callBack Result: " +onCompletion.ToString());
	}
		



	public void savePageImage(Texture2D pageTexture, string currentBookFileName, int pageIndex, string _result) {
		Debug.Log ("<<< SaveMAnager.savePageImage started >>>");
		Debug.Log ("rootFolder is: " + rootFolder.ToString());
		string bookFolderPath = Path.Combine(rootFolder, currentBookFileName);
		string pageFolderPath = Path.Combine (bookFolderPath, "Page_" + pageIndex.ToString ("000"));
		string photoFileName = "pagePhoto.PNG";
		string pathToFile = Path.Combine (pageFolderPath, photoFileName);
		Debug.Log ("pathTofile is: " + pathToFile);
		BinaryFormatter binaryFormatter = new BinaryFormatter ();
		byte[] photoData = pageTexture.EncodeToPNG();

		//check to see if book folder exists already - if not create it (do not break)
		if (!Directory.Exists(bookFolderPath)){
			Directory.CreateDirectory(bookFolderPath);
		}
		//check to see if page folder exists already - if not create it (do not return)
		if (!Directory.Exists(pageFolderPath)){
			Directory.CreateDirectory(pageFolderPath);
		}
		//check to see if image file exists already - if so delete it
		if (File.Exists(pathToFile)){
			File.Delete (pathToFile);
		}
		//create the file in correct folder, serialize, and close file
		//FileStream file = File.Create (pathToFile);
		File.WriteAllBytes (pathToFile, photoData);
		//binaryFormatter.Serialize (file, photoData);
		//file.Close ();
	}

	public void savePageAudio(AudioClip clip, string currentBookFileName, int pageIndex, string _result) {
		string bookFolderPath = Path.Combine(rootFolder, currentBookFileName);
		string pageFolderPath = Path.Combine (bookFolderPath, "Page_" + pageIndex.ToString ("000"));
		string audioFileName = "Audio.WAV";
		string pathToFile = Path.Combine (pageFolderPath, audioFileName);

		//check to see if book folder exists already - if not create it (do not break)
		if (!Directory.Exists(bookFolderPath)){
			Directory.CreateDirectory(bookFolderPath);
		}
		//check to see if page folder exists already - if not create it (do not return)
		if (!Directory.Exists(pageFolderPath)){
			Directory.CreateDirectory(pageFolderPath);
		}
		//check to see if image file exists already - if so delete it
		if (File.Exists(pathToFile)){
			File.Delete (pathToFile);
		}
		//create the file in correct folder, serialize, and close file
		SavWav.Save (pathToFile, clip);
	}

	public Texture2D loadTitleTexture(string currentBookFileName){
		Debug.Log("<< Load Texture Method Began >>");
		string bookFolderPath = Path.Combine(rootFolder, currentBookFileName);
		string pageFolderPath = Path.Combine (bookFolderPath, "Page_000");
		string photoFileName = "pagePhoto.PNG";
		string pathToFile = Path.Combine (pageFolderPath, photoFileName);
		Debug.Log ("pathToFile: " + pathToFile);
		//BinaryFormatter bf = new BinaryFormatter ();
		//FileStream file = File.Open (pageFolderPath, FileMode.Open);
		byte[] photoData = File.ReadAllBytes(pathToFile);
		//file.Close ();
		Debug.Log ("## Load Texture Method completed ##");
		return deserializePhoto(photoData);
	}

	public Texture2D deserializePhoto (byte[] bytes){
		var texture = new Texture2D(1,1);
		texture.LoadImage(bytes);
		return texture;
	}






}


/*
 *public void save(){
		Debug.Log("<< Save Method Began >>");
		//SaveManager saveManager = new SaveManager();
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
//		string currentFolderName = currentBookFileName;
//		if (!Directory.Exists(Application.persistentDataPath + "/" + currentFolderName)){
//			Directory.CreateDirectory(Application.persistentDataPath + "/" + currentFolderName);
//		}


		SaveManager.createFolder(currentBookFileName, "result");
		
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/" + currentBookFileName + "/" + currentBookFileName + ".dat");
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
		//SavWav.Save("myweirdfile.wav", screenBook.pages[0].clip);
		Debug.Log ("file saved: " + currentBookFileName);
		Debug.Log ("<color=green>## Save Method completed ##</color>");
	}
 *
 * 
 * 
 */
