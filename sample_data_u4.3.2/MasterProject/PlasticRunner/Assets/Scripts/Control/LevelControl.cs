using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 레벨 데이터.
public class LevelData {
	
	public LevelData()
	{
		this.end_time        = 15.0f;
		this.player_speed    = 6.0f;
		this.floor_count.min = 10;
		this.floor_count.max = 10;
		this.hole_count.min  = 2;
		this.hole_count.max  = 6;
		this.height_diff.min = 0;
		this.height_diff.max = 0;
		this.coin_interval.min = 10;
		this.coin_interval.max = 10;
		this.enemy_interval.min = 30;
		this.enemy_interval.max = 30;
	}

	public struct Range {
		
		public	int		min;
		public	int		max;
	};

	public float	end_time;			// 종료 시간.
	public float	player_speed;		// 플레이어의 속도.

	public Range	floor_count;		// 바닥이 이어지는 수.
	public Range	hole_count;			// 구멍이 이어지는 수.
	public Range	height_diff;		// 바닥의 높이 변화 .

	public Range	coin_interval;		// 코인이 나오는 간격.
	public Range	enemy_interval;		// 방해 캐릭터가 나오는 간격.
};

// 블록 배치 관리(맵패턴. 다음에 만들 블록의 타입을 정한다).
public class LevelControl {

	public int		HEIGHT_MAX = 20;					// 바닥의 최고 높이.
	public int		HEIGHT_MIN = -4;					// 바닥의 최저 높이.


	// 만들 블록 등의 정보.
	public struct CreationInfo {

		public	Block.TYPE		block_type;				// 블록 타입.
		public	int				max_count;				// 연속으로 만들 개수.
		public	int				height;					// 높이.

		public	int				current_count;			// 실제로 만든 개수.
	};

	public CreationInfo		current_block;				// 다음에 만들 블록.
	public CreationInfo		next_block;					// 다음 다음에 만들 블록.
														// 코인을 건너편까지 나열할 때를 위해서 미리 읽어 둔다.

	public	int				block_count = 0;			// 만든 블록의 수 .

	public int				current_level = 0;			// 현재 레벨.

	private List<LevelData>			level_datas = new List<LevelData>();

	// ================================================================ //

	public void		initialize()
	{
		this.block_count = 0;
		this.current_level = 0;

		this.clear_next_block(ref this.current_block);
		this.clear_next_block(ref this.next_block);
	}

	// 매 프레임 갱신 처리.
	public void		update(float passage_time)
	{
		this.current_block.current_count++;

		
		// 같은 블록을 일정 개수 이상 만들면 블록의 타입을 갱신.
		//
		if(this.current_block.current_count >= this.current_block.max_count) {

			this.current_block  = this.next_block;
			this.clear_next_block(ref this.next_block);

			this.update_level(ref this.next_block, this.current_block, passage_time);
	
		}

		this.block_count++;
	}

	// 플레이어의 속도를 얻는다.
	public float	getPlayerSpeed()
	{
		return(this.level_datas[this.current_level].player_speed);
	}

	// 레벨 데이터를 텍스트 파일에서 읽는다.
	public void		loadLevelData(TextAsset level_data_text)
	{
		// 텍스트 전체를 하나의 문자열로.
		string		level_texts = level_data_text.text;

		// 개행 코드로 구분하여
		// 텍스트 전체를 한줄 단위의 배열로 만든다.
		string[]	lines = level_texts.Split('\n');

		foreach(var line in lines) {

			if(line == "") {

				continue;
			}

			// 공백으로 구분해서 단어의 배열로 만든다.
			string[]	words = line.Split();

			int			n = 0;
			LevelData	level_data = new LevelData();

			foreach(var word in words) {

				// "#" 이후는 주석이므로 건너뛴다.
				if(word.StartsWith("#")) {

					break;
				}
				if(word == "") {

					continue;
				}

				switch(n) {

					case 0:		level_data.end_time           = float.Parse(word);	break;
					case 1:		level_data.player_speed       = float.Parse(word);	break;
					case 2:		level_data.floor_count.min    = int.Parse(word);	break;
					case 3:		level_data.floor_count.max    = int.Parse(word);	break;
					case 4:		level_data.hole_count.min     = int.Parse(word);	break;
					case 5:		level_data.hole_count.max     = int.Parse(word);	break;
					case 6:		level_data.height_diff.min    = int.Parse(word);	break;
					case 7:		level_data.height_diff.max    = int.Parse(word);	break;
					case 8:		level_data.coin_interval.min  = int.Parse(word);	break;
					case 9:		level_data.coin_interval.max  = int.Parse(word);	break;
					case 10:	level_data.enemy_interval.min = int.Parse(word);	break;
					case 11:	level_data.enemy_interval.max = int.Parse(word);	break;
				}

				n++;
			}

			if(n >= 10) {

				this.level_datas.Add(level_data);

			} else {

				if(n == 0) {

					// 단어가 없다=행 전체가 주석.

				} else {

					// 파라미터가 부족하다.
					Debug.LogError("[LevelData] Out of parameter.\n");
				}
			}
		}

		if(this.level_datas.Count == 0) {

			// 데이터가 하나도 없으면.

			Debug.LogError("[LevelData] Has no data.\n");

			// 기본 데이터를 하나 추가해 둔다.
			this.level_datas.Add(new LevelData());
		}
	}

	// 현재 레벨의 레벨 데이터를 가져온다.
	public LevelData	getCurrentLevelData()
	{
		return(this.level_datas[this.current_level]);
	}

	// -------------------------------------------------------------------- //

	private void	update_level(ref CreationInfo current, CreationInfo previous, float passage_time)
	{
		// ---------------------------------------------------------------- //
		// 현재 시간이 포함되는 곳까지 레벨을 진행한다.
		
		// '마지막 데이터의 종료 시간'으로 반복하도록.
		float	local_time = Mathf.Repeat(passage_time, this.level_datas[this.level_datas.Count - 1].end_time);

		int		i;

		for(i = 0;i < this.level_datas.Count - 1;i++) {

			if(local_time <= this.level_datas[i].end_time) {

				break;
			}
		}

		this.current_level = i;

		// ---------------------------------------------------------------- //

		current.block_type = Block.TYPE.FLOOR;
		current.max_count  = 1;

		if(this.block_count >= 10) {

			LevelData	level_data;

			level_data = this.level_datas[this.current_level];

			switch(previous.block_type) {
	
				case Block.TYPE.FLOOR:
				{
					// 블록 hole_size 개분의 폭을 가진 구멍을 만든다.
	
					current.block_type = Block.TYPE.HOLE;
					current.max_count  = Random.Range(level_data.hole_count.min, level_data.hole_count.max + 1);
					current.height     = previous.height;
				}
				break;
	
				case Block.TYPE.HOLE:
				{
					// 블록 10개분의 바닥을 만든다.
	
					current.block_type = Block.TYPE.FLOOR;
					current.max_count  = Random.Range(level_data.floor_count.min, level_data.floor_count.max);

					int		height_min = previous.height + level_data.height_diff.min;
					int		height_max = previous.height + level_data.height_diff.max;

					height_min = Mathf.Clamp(height_min, HEIGHT_MIN, HEIGHT_MAX);
					height_max = Mathf.Clamp(height_max, HEIGHT_MIN, HEIGHT_MAX);

					current.height = Random.Range(height_min, height_max + 1);

					//Debug.Log(height_min.ToString() + " " + height_max + " " + current.height.ToString());
				}
				break;
			}
		}

	}

	// 블록 생성 정보를 클리어한다.
	private void	clear_next_block(ref CreationInfo block)
	{
		block.block_type         = Block.TYPE.FLOOR;
		block.max_count          = 15;
		block.height             = 0;

		block.current_count = 0;
	}


}
