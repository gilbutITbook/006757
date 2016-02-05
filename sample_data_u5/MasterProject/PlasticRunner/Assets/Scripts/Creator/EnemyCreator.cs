using UnityEngine;
using System.Collections;

// 방해 캐릭터를 만든다.
public class EnemyCreator : MonoBehaviour {

	public GameObject	enemy_prefab = null;			// 코인 프리팹. Inspector에서 설정한다.

	private int			next_block = 10;				// 다음에 방해 캐릭터를 만들 블록.

	// ---------------------------------------------------------------- //

	public	MapCreator		map_creator = null;			// 맵 크리에이터.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	
	}


	// ================================================================ //
	// 방해 키릭터를 만든다.
	public void		createEnemy(LevelData level_data, int block_count, Vector3 block_position)
	{
		// 블록을 일정 개수(임의) 만들 때마다 방해 캐릭터를 만든다.
		if(block_count >= this.next_block) {

			Vector3		p0 = block_position;
	
			// 블록 위에 딱 맞게 올라갔을 때의 높이.
			p0.y += MapCreator.BLOCK_HEIGHT/2.0f + CoinControl.COLLISION_SIZE/2.0f;
			p0.y += PlayerControl.COLLISION_SIZE;
	
			this.create_enemy_object(p0);

			// 다음에 방해 캐릭터를 만들 블록을 갱신해 둔다.
			this.next_block += Random.Range(level_data.enemy_interval.min, level_data.enemy_interval.max + 1);
		}
	}

	// ================================================================ //

	// 방해 캐릭터의 게임 오브젝트를 만든다.
	private EnemyControl	create_enemy_object(Vector3 position)
	{
		GameObject	go = GameObject.Instantiate(this.enemy_prefab) as GameObject;

		EnemyControl	enemy = go.GetComponent<EnemyControl>();

		enemy.map_creator = this.map_creator;

		enemy.transform.position = position;

		return(enemy);
	}
}
