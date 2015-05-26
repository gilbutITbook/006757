using UnityEngine;
using System.Collections;

//! 코인을 만든다.
public class CoinCreator : MonoBehaviour {

	public GameObject	coin_prefab = null;				// 코인 프리팹. Inspector에서 설정한다.
	public GameObject	dropped_coin_prefab = null;		// 점프 중에 떨어뜨릴 동전의 프리팹. Inspector에서 설정한다.

	private int			next_block = 10;				// 次にコインをつくるブロック.

	// ---------------------------------------------------------------- //

	private ScoreControl	score_control  = null;		// 점수 관리.
	public	MapCreator		map_creator = null;			// 맵 크리에이터.

	// ================================================================ //

	public static CoinCreator	getInstance()
	{
		CoinCreator	coin_creator = GameObject.FindGameObjectWithTag("Game Root").GetComponent<CoinCreator>();

		return(coin_creator);
	}

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.score_control  = this.gameObject.GetComponent<ScoreControl>();
	}
	
	void	Update()
	{
	
	}

	// ================================================================ //

	// 코인을 만든다.
	public void		createCoin(LevelData level_data, int block_count, Vector3 block_position)
	{
		// 블록을 일정 개수(임의) 만들 때마다 코인을 만든다.
		if(block_count >= this.next_block) {

			Vector3		p0 = block_position;
	
			// 블록 위에 딱 맞게 올라갔을 때의 높이.
			p0.y += MapCreator.BLOCK_HEIGHT/2.0f + CoinControl.COLLISION_SIZE/2.0f;
	
			p0.y += PlayerControl.COLLISION_SIZE;
	
			this.create_coin_object(p0);

			// 다음에 코인을 만들 블록 갱신해 둔다.
			this.next_block += Random.Range(level_data.coin_interval.min, level_data.coin_interval.max + 1);
		}
	}

	// 점프 중에 떨어뜨릴 동전을 만든다.
	public void		createDroppedCoin(Vector3 position)
	{
		DroppedCoinControl	coin = this.create_dropped_coin_object(position);

		// 왼쪽 방향으로.
		coin.rigidbody.velocity = new Vector3(-1.0f, 1.0f, 0.0f);
		// 빙글빙글 돌면서.
		coin.rigidbody.angularVelocity = new Vector3(0.0f, 10.0f, 0.0f);
	}


	// ================================================================ //

	// 코인 게임 오브젝트를 만든다.
	private CoinControl	create_coin_object(Vector3 position)
	{
		GameObject	go = GameObject.Instantiate(this.coin_prefab) as GameObject;

		CoinControl	coin = go.GetComponent<CoinControl>();

		coin.score_control = this.score_control;
		coin.map_creator   = this.map_creator;

		coin.transform.position = position;

		return(coin);
	}

	// '떨어뜨린 코인'의 게임 오브젝트를 만든다.
	private DroppedCoinControl	create_dropped_coin_object(Vector3 position)
	{
		GameObject	go = GameObject.Instantiate(this.dropped_coin_prefab) as GameObject;

		DroppedCoinControl	coin = go.GetComponent<DroppedCoinControl>();

		coin.map_creator = this.map_creator;

		coin.transform.position = position;

		return(coin);
	}

}
