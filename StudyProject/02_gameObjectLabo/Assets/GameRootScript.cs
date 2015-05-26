using UnityEngine;
using System.Collections;

public class GameRootScript : MonoBehaviour {

	public GameObject prefab = null;

	private AudioSource audio;
	public AudioClip jumpSound;

	public Texture2D icon = null;
	public static string mes_text = "test";

	void Start () {
		this.audio = this.gameObject.AddComponent<AudioSource>();
		// jumpSound에 저장한 음원을 울리게 준비.
		this.audio.clip = this.jumpSound;
		// 루프 재생(반복 재생)을 무효로.
		this.audio.loop = false;
	}



	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			// Instantiate(prefab);
			// prefab변수에서 만들어진 GameObject를 가져옴.
			GameObject go =
				GameObject.Instantiate(this.prefab) as GameObject;
			// 가져온 GameObject의 설정을 변경.
			go.transform.position =
				new Vector3(Random.Range(-2.0f, 2.0f), 1.0f, 1.0f);

			this.audio.Play(); // audio에 들어있는 음원을 재생.
		}

	}


	void OnGUI() {
		GUI.DrawTexture(new Rect(Screen.width / 2, 64, 64, 64), icon);
		GUI.Label(new Rect(Screen.width / 2, 128, 128, 32), mes_text);
	}
}
