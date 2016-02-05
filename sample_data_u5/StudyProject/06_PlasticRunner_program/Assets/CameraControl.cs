using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	private GameObject player = null;
	private Vector3 position_offset = Vector3.zero;

	void Start()
	{
		// 멤버 변수 player로 Player 오브젝트를 가져온다.
		this.player = GameObject.FindGameObjectWithTag("Player");
		// 카메라 위치(this.transform.position)와.
		// 플레이어 위치(this.player.transform.position)의 차이를 보관.
		this.position_offset =
			this.transform.position - this.player.transform.position;
	}

	void LateUpdate()
	{
		// 카메라의 현재 위치를 new_position에 저장한다.
		Vector3 new_position = this.transform.position;
		// 플레이어의 X좌표에 차이를 더해서 변수 new_position의 X에 대입한다.
		new_position.x =
			this.player.transform.position.x + this.position_offset.x;
		// 카메라의 위치를 새로운 위치(new_position)으로 갱신.
		this.transform.position = new_position;
	}
}
