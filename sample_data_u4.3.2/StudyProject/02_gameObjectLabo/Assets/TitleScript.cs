using UnityEngine;
using System.Collections;

public class TitleScript : MonoBehaviour {

	void Update() {
		if(Input.GetMouseButtonDown(0)) { // 좌클릭되었으면.
			Application.LoadLevel("studyScene"); // studyScene으로 이동한다. 
		}
	}
	void OnGUI() {
		// 화면에 'title'이라고 표시한다.
		GUI.Label(new Rect(Screen.width/2, Screen.height/2, 128, 32), "title");
	}
}
