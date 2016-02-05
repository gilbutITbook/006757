using UnityEngine;
using System.Collections;

//! 코인.
public class CoinControl : MonoBehaviour {

	public static float	COLLISION_SIZE = 1.0f;			// COLLISION_SIZE(지름).


	public	ScoreControl	score_control = null;		// 점수 관리.
	public	MapCreator		map_creator = null;			//맵 크리에이터.
	public	Vector3			goal_position;
	public	float			height_offset = 0.0f;





	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.goal_position = this.transform.position;

	}
	
	void	Update()
	{
		// 뿅.
		this.height_offset *= 0.90f*(Time.deltaTime/(1.0f/60.0f));
		this.transform.position = this.goal_position + this.height_offset*Vector3.up;

		// 빙글빙글.
		float	spin_speed = (360.0f/2.0f);
		this.transform.rotation *= Quaternion.AngleAxis(spin_speed*Time.deltaTime, Vector3.up);

		// 화면왼쪽끝으로 벗어나면 지운다.
		if(this.map_creator.isDelete(this.gameObject)) {
			GameObject.Destroy(this.gameObject);
		}
	}

	// ---------------------------------------------------------------- //

	void 	OnTriggerEnter(Collider other)
	{
		// 플레이어가 주우면 스코어를 더한다.
		if(other.tag == "Player") {
			this.score_control.addCoinScore();
		}

		GameObject.Destroy(this.gameObject);
	}

	// ================================================================ //
}
