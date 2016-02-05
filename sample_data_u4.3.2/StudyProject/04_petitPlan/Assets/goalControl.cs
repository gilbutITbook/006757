using UnityEngine;
using System.Collections;

public class goalControl : MonoBehaviour {

	// 충돌했는가(true), 충돌하지 않았는가(false)를 나타낸다.
	private bool is_collided = false;

	public float GOAL_MIN = 5.0f; // 최솟값.
	public float GOAL_MAX = 10.0f; // 최댓값.

	void Start() {
		// GOAL_MIN~GOAL_MAX 사이의 임의의 값을 가져온다.
		float rnd = Random.Range(GOAL_MIN, GOAL_MAX);
		// Goal의 X위치를 임의의 값으로.
		this.transform.position = new Vector3(rnd, -1.0f, 0.0f);
	}


	void Update() {
	}

	// 다른 GameObject와 충돌하는 동안 계속 호출된다.
	void OnCollisionStay(Collision other)
	{
		this.is_collided = true;
	}
	void OnGUI() {
		if(is_collided) { // 충돌했으면.
			// 화면에 '성공'이라고 표시.
			GUI.Label(new Rect(Screen.width/2, 80, 100, 20), "성공");
		}
	}
}
