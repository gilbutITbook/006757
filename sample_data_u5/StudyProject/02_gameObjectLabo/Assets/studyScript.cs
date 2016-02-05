using UnityEngine;
using System.Collections;

public class studyScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.A)) {
			// 0.0~0.5 사이의 난수를 만들어낸다.
			float rnd = Random.Range(0.0f, 5.0f);
			// 자신(Capsule)의 위치를 변경.
			this.transform.position = new Vector3(0.0f, 0.0f, rnd);
		}

		if(Input.GetKeyDown(KeyCode.B)) {
			float rnd = Random.Range(0.0f, 360.0f);
			// X축 방향 회전 상태를 임의로 변경.
			this.transform.rotation = Quaternion.Euler(rnd, 0.0f, 0.0f);
		}

		if(Input.GetKeyDown(KeyCode.C)) {
			float rnd = Random.Range(0.5f, 2.0f);
			// 크기를 임의로 변경.
			this.transform.localScale = new Vector3(rnd, rnd, rnd);
		}
		if(Input.GetKey(KeyCode.UpArrow)) { // ↑키로 forward(전).
			// this.transform.Translate(
			// new Vector3(0.0f, 0.0f, 3.0f *Time.deltaTime));
			this.transform.Translate(Vector3.forward * 3.0f * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.DownArrow)) { // ↓키로 back(후).
			this.transform.Translate(Vector3.back * 2.0f * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.LeftArrow)) { // ←키로 left(좌).
			this.transform.Translate(Vector3.left * 2.0f * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.RightArrow)) { // →키로 right(우).
			this.transform.Translate(Vector3.right * 2.0f * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.U)) { // U키로 up(위).
			this.transform.Translate(Vector3.up * 2.0f * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.D)) { // D키로 down(아래).
			this.transform.Translate(Vector3.down * 2.0f * Time.deltaTime);
		}

		if(Input.GetKey(KeyCode.R)) { // R키로 우회전.
			this.transform.Rotate(90.0f * Time.deltaTime, 0.0f, 0.0f);
		}
		if(Input.GetKey(KeyCode.L)) { // L키로 좌회전.
			this.transform.Rotate(-90.0f * Time.deltaTime, 0.0f, 0.0f);
		}


		if(Input.GetKey(KeyCode.P)) { // Cube의 부모를 자신(Cupsule)으로 한다.
			GameObject go = GameObject.Find("Cube") as GameObject;
			go.transform.parent = this.transform;
		}
		if(Input.GetKey(KeyCode.N)) { // Cube의 부모를 해제한다.
			GameObject go = GameObject.Find("Cube") as GameObject;
			go.transform.parent = null;
		}

		if(Input.GetKey(KeyCode.G)) {
			GameObject go = GameObject.Find("Cube") as GameObject;
			go.GetComponent<cubeScript>().bigsize();
		}
	}
}
