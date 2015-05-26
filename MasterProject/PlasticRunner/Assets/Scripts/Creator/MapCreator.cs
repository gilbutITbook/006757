using UnityEngine;
using System.Collections;

// 블록.
public class Block {

	// 종류.
	public enum TYPE {

		NONE = -1,

		FLOOR = 0,			// 바닥.
		HOLE,				// 구멍.

		NUM,
	};
};

// 맵 크리에이터.
public class MapCreator : MonoBehaviour {

	public TextAsset	level_data_text = null;

	// ================================================================ //

	public static float		BLOCK_WIDTH  = 1.0f;		// 블록의 폭(X방향 크기).
	public static float		BLOCK_HEIGHT = 0.2f;		// 블록의 높이(Y방향 크기).
	public static int		BLOCK_NUM_IN_SCREEN = 24;	// 한 화면 속의 블록 수(가로 방향).


	// 바닥 블록.
	// 블록이 없는 장소를 빈 블록이 있는 걸로  
	// 블록이 있는 장소와 똑같이 처리할 수 있게 한다.
	private struct FloorBlock {

		public bool		is_created;						// false일 때는 하나도 블록이 만들어지지 않았다.	
		public Vector3	position;						// 위치.
	};

	private PlayerControl	player = null;				// 플레이어.
	private FloorBlock		last_block;					// 마지막으로 만든 블록.

	private GameRoot		game_root     = null;		// 게임 진행 관리.
	private	LevelControl	level_control = null;		// 블록 배치 관리(맵 패턴. 다음에 만들 블록 타입 결정).
	private BlockCreator	block_creator = null;		// 블록 크리에이터.
	private CoinCreator		coin_creator  = null;		// 코인 크리에이터.
	private EnemyCreator	enemy_creator = null;		// 방해 캐릭터 크리에이터.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

		//

		this.last_block.is_created = false;

		this.level_control = new LevelControl();
		this.level_control.initialize();
		this.level_control.loadLevelData(this.level_data_text);

		this.player.level_control = this.level_control;

		//

		this.game_root     = this.gameObject.GetComponent<GameRoot>();
		this.block_creator = this.gameObject.GetComponent<BlockCreator>();
		this.coin_creator  = this.gameObject.GetComponent<CoinCreator>();
		this.enemy_creator = this.gameObject.GetComponent<EnemyCreator>();

		this.block_creator.map_creator = this;
		this.coin_creator.map_creator = this;
		this.enemy_creator.map_creator = this;

		//

		this.create_floor_block();
	}

	void	Update()
	{
		// DebugPrint.print("time  " + ((int)this.game_root.getPlayTime()).ToString());
		// DebugPrint.print("speed " + (this.level_control.getPlayerSpeed()).ToString());
		// DebugPrint.print("level " + this.level_control.current_level.ToString());

		// -------------------------------------------------------------------- //
		// 플레이어와 '마지막에 만든 블록'의 거리가 어느 정도 가까워지면.
		// 다음 블록을 만든다.

		float	block_generate_x = this.player.transform.position.x;

		// 플레이어의 전방(왼쪽, 대체로 화면 왼쪽 끝).
		block_generate_x += BLOCK_WIDTH*((float)BLOCK_NUM_IN_SCREEN + 1)/2.0f;

		// 무한 루프 방지용 카운터.
		int		fail_safe_count = 100;

		while(this.last_block.position.x < block_generate_x) {

			this.create_floor_block();

			// 프로그램에 버그가 있어도 무한 루프가 되지 않도록 
			// 어느 정도 이상 실행하면 강제로 중단한다.

			fail_safe_count--;

			if(fail_safe_count <= 0) {

				break;
			}
		}
	}

	// ================================================================ //

	// 블록/코인을 지운다?.
	public bool		isDelete(GameObject block_object)
	{
		bool	ret = false;

		float	left_limit = this.player.transform.position.x - BLOCK_WIDTH*((float)BLOCK_NUM_IN_SCREEN/2.0f);

		// 플레이어보다도 일정 거리 이상 왼쪽으로 가면.
		// (화면 왼쪽 끝에서 밖으로 나가면)지운다.
		if(block_object.transform.position.x < left_limit) {

			ret = true;
		}

		// 화면 아래로 사라지면 지운다.
		if(block_object.transform.position.y < PlayerControl.NARAKU_HEIGHT) {

			ret = true;
		}

		return(ret);
	}		

	// ================================================================ //

	// 블록을 만든다.
	private void	create_floor_block()
	{
		Vector3		block_position;

		// -------------------------------------------------------------------- //
		// 다음에 만들 블록 타입(바닥 또는 구멍)을 결정한다.

		this.level_control.update(this.game_root.getPlayTime());

		// -------------------------------------------------------------------- //
		// 블록의 위치.

		// '마지막에(직전에)' 만든 블록의 위치를 구한다.
		if(!this.last_block.is_created) {

			// 또한 하나도 블록을 만들지 않았을 때는.
			// 플레이어의 뒤쪽, 스크린 왼쪽 끝을 기준으로 한다.
			block_position = this.player.transform.position;
			block_position.x -= BLOCK_WIDTH*((float)BLOCK_NUM_IN_SCREEN/2.0f);

		} else {

			block_position = this.last_block.position;
		}

		block_position.x += BLOCK_WIDTH;
		block_position.y = (float)this.level_control.current_block.height*BLOCK_HEIGHT;

		// -------------------------------------------------------------------- //
		// 블록 게임 오브젝트를 만든다.

		LevelControl.CreationInfo	current = this.level_control.current_block;

		this.block_creator.createBlock(current, block_position);

		// -------------------------------------------------------------------- //
		// 코인을 만든다.

		LevelData	level_data = this.level_control.getCurrentLevelData();

		this.coin_creator.createCoin(level_data, this.level_control.block_count, block_position);

		// -------------------------------------------------------------------- //
		// 방해 캐릭터를 만든다.

		this.enemy_creator.createEnemy(level_data, this.level_control.block_count, block_position);

		// -------------------------------------------------------------------- //
		// '마지막에 만든 블록의 위치'를 갱신해 둔다.

		this.last_block.position   = block_position;
		this.last_block.is_created = true;

		// -------------------------------------------------------------------- //
	}
}
