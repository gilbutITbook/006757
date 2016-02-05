using UnityEngine;
using System.Collections;

public class RigidBodyScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.UpArrow)) { // ↑키로 안쪽 방향.
			this.transform.rigidbody.AddForce(
				Vector3.forward * 300 * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.DownArrow)) { // ↓키로 앞쪽 방향.
			this.transform.rigidbody.AddForce(
				Vector3.back * 300* Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.LeftArrow)) { // ←키로 왼쪽 방향.
			this.transform.rigidbody.AddForce(
				Vector3.left * 300 * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.RightArrow)) { //→키로 오른쪽 방향.
			this.transform.rigidbody.AddForce(
				Vector3.right * 300 * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.U)) { // U키로 위 방향.
			this.transform.rigidbody.AddForce(
				Vector3.up * 300 * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.D)) { // D키로 아래방향.
			this.transform.rigidbody.AddForce(
				Vector3.down * 300 * Time.deltaTime);
		}


		if(Input.GetKeyDown(KeyCode.Keypad0)) { // 텐키의 0.
			Physics.gravity = Vector3.zero; // 중력을 제로로 한다.
		}
		if(Input.GetKeyDown(KeyCode.Keypad8)) { // 텐키의 8.
			Physics.gravity = Vector3.up; // 중력을 위 방향으로 한다.
		}
		if(Input.GetKeyDown(KeyCode.Keypad2)) { // 텐키의 2.
			Physics.gravity = Vector3.down; // 중력을 아래 방향으로 한다.
		}


	}
}
