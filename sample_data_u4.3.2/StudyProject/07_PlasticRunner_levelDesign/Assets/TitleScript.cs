using UnityEngine;
using System.Collections;

public class TitleScript : MonoBehaviour {

	void Update() {
		if(Input.GetMouseButtonDown(0)) {
			Application.LoadLevel("GameScene");
		}
	}

	void OnGUI() {
		GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 128, 32),
		          "PlasticRunner");
	}
}
