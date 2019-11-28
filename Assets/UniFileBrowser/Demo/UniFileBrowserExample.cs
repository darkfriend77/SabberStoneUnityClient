// Example of open/save usage with UniFileBrowser
// This script is free to use in any manner

using UnityEngine;
using System.Collections;
using System.IO;

public class UniFileBrowserExample : MonoBehaviour {
	
	string message = "";
	float alpha = 1.0f;
	
	void OnGUI () {
		if (GUI.Button (new Rect(100, 50, 95, 35), "Open")) {
			if (UniFileBrowser.use.allowMultiSelect) {
				UniFileBrowser.use.OpenFileWindow (OpenFiles);
			}
			else {
				UniFileBrowser.use.OpenFileWindow (OpenFile);
			}
		}
		if (GUI.Button (new Rect(100, 125, 95, 35), "Save")) {
			UniFileBrowser.use.SaveFileWindow (SaveFile);
		}
		if (GUI.Button (new Rect(100, 200, 95, 35), "Open Folder")) {
			UniFileBrowser.use.OpenFolderWindow (true, OpenFolder);
		}
		var col = GUI.color;
		col.a = alpha;
		GUI.color = col;
		GUI.Label (new Rect(100, 275, 500, 1000), message);
		col.a = 1.0f;
		GUI.color = col;
	}
	
	void OpenFile (string pathToFile) {
		var fileName = Path.GetFileName (pathToFile);
		message = "You selected file: " + fileName;
		Fade();
	}
	
	void OpenFiles (string[] pathsToFiles) {
		message = "You selected these files:\n";
		for (var i = 0; i < pathsToFiles.Length; i++) {
			message += Path.GetFileName (pathsToFiles[i]) + "\n";
		}
		Fade();
	}
	
	void SaveFile (string pathToFile) {
		var fileName = Path.GetFileName (pathToFile);
		message = "You're saving file: " + fileName;
		Fade();
	}
	
	void OpenFolder (string pathToFolder) {
		message = "You selected folder: " + pathToFolder;
		Fade();
	}
	
	void Fade () {
		StopCoroutine ("FadeAlpha");	// Interrupt FadeAlpha if it's already running, so only one instance at a time can run
		StartCoroutine ("FadeAlpha");
	}
	
	IEnumerator FadeAlpha () {
		alpha = 1.0f;
		yield return new WaitForSeconds (5.0f);
		for (alpha = 1.0f; alpha > 0.0f; alpha -= Time.deltaTime/4) {
			 yield return null;
		}
		message = "";
	}
}