using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	private PlayerControl	player = null;

	private	Vector3			position_offset = Vector3.zero;
	private Vector3			move_vector = Vector3.zero;					// 이전 프레임에서의 이동량.

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

		// 플레이어와 카메라 포지션의 오프셋(위치의 차이)를 구해둔다.
		// 플레이어와 함께 행동하도록 하기 위해.
		this.position_offset = this.transform.position - this.player.transform.position;
	}
	
	void	Update ()
	{

	}

	void	LateUpdate()
	{
		// 플레이어와 함께 이동한다.

		Vector3		new_position = this.transform.position;

		new_position.x = this.player.transform.position.x + this.position_offset.x;

		this.move_vector = (new_position - this.transform.position)/Time.deltaTime;

		this.transform.position = new_position;
	}

	public Vector3	getMoveVector()
	{
		return(this.move_vector);
	}
}
