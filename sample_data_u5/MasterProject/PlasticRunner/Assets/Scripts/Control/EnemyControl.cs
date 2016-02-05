using UnityEngine;
using System.Collections;

// 적 캐릭터.
public class EnemyControl : MonoBehaviour {

	public	MapCreator		map_creator = null;			// 맵 크리에이터.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	
		// 화면 왼쪽으로 벗어나면 지운다.
		if(this.map_creator.isDelete(this.gameObject)) {

			GameObject.Destroy(this.gameObject);
		}
	}

	// ---------------------------------------------------------------- //

	void 	OnCollisionEnter(Collision other)
	{
		if(other.collider.tag == "Player") {
			// 플레이어와 부딪혔다면 플레이어에게 알려준다.
			PlayerControl	player = other.collider.gameObject.GetComponent<PlayerControl>();
			player.onTouchEnemy(this);
		}
	}
}
