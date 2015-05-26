using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 레벨 데이터.
public class LevelData {
	
	public LevelData()
	{
		this.probability = new float[(int)Block.COLOR.NORMAL_COLOR_NUM];

		int		block_num = (int)(Block.COLOR.MAGENTA - Block.COLOR.PINK + 1);

		for(int i = 0;i < block_num;i++) {

			this.probability[i] = 1.0f/(float)block_num;
		}
	}

	// 블록의 확률을 전부 0.0으로 한다.
	public void		clear()
	{
		for(int i = 0;i < this.probability.Length;i++) {

			this.probability[i] = 0.0f;
		}
	}

	// 블록의 확률 합계를 1.0으로 한다.
	public void		normalize()
	{
		float	sum = 0.0f;

		for(int i = 0;i < this.probability.Length;i++) {

			sum += this.probability[i];
		}

		for(int i = 0;i < this.probability.Length;i++) {

			this.probability[i] /= sum;

			if(float.IsInfinity(this.probability[i])) {

				this.clear();
				this.probability[0] = 1.0f;
				break;
			}
		}
	}

	public float[]	probability;			// 블록의 출현 확률.
	public float	heat_time;				// 재발화 접수 시간.
};

// 블록 배치의 관리(맵 패턴. 다음으로 만들 블록의 종류를 결정한다).
public class LevelControl {

	private List<LevelData>		level_datas = null;		// 텍스트 파일에서 읽은 레벨 데이터.

	private int		select_level = 0;					// 선택된 레벨(level_datas[]의 인덱스).

	// ================================================================ //

	public void		initialize()
	{
		this.level_datas = new List<LevelData>();
	}

	// 레벨 데이터를 텍스트 파일에서 읽는다.
	public void		loadLevelData(TextAsset level_data_text)
	{
		// 텍스트 전체를 하나의 문자열로.
		string		level_texts = level_data_text.text;

		// 개행 코드로 구별해서.
		// 텍스트 전체를 한 줄 단위로 배열로 만든다.
		string[]	lines = level_texts.Split('\n');

		foreach(var line in lines) {

			if(line == "") {

				continue;
			}

			// 공백으로 구분해서 단어 배열을 만든다.
			string[]	words = line.Split();

			int			n = 0;
			LevelData	level_data = new LevelData();

			foreach(var word in words) {

				// "#" 이후는 주석이므로 스킵.
				if(word.StartsWith("#")) {

					break;
				}
				if(word == "") {

					continue;
				}

				switch(n) {

					case 0:		level_data.probability[(int)Block.COLOR.PINK]    = float.Parse(word);	break;
					case 1:		level_data.probability[(int)Block.COLOR.BLUE]    = float.Parse(word);	break;
					case 2:		level_data.probability[(int)Block.COLOR.GREEN]   = float.Parse(word);	break;
					case 3:		level_data.probability[(int)Block.COLOR.ORANGE]  = float.Parse(word);	break;
					case 4:		level_data.probability[(int)Block.COLOR.YELLOW]  = float.Parse(word);	break;
					case 5:		level_data.probability[(int)Block.COLOR.MAGENTA] = float.Parse(word);	break;
					case 6:		level_data.heat_time = float.Parse(word);	break;
				}

				n++;
			}

			if(n >= 7) {

				level_data.normalize();
				this.level_datas.Add(level_data);

			} else {

				if(n == 0) {
					// 단어가 없었다 = 행전체가 주석이었다.
				} else {
					// 파라미터가 부족하다.
					Debug.LogError("[LevelData] Out of parameter.\n");
				}
			}
		}

		if(this.level_datas.Count == 0) {

			// 데이터가 한 개도 없을 때.

			Debug.LogError("[LevelData] Has no data.\n");

			// 기본 데이터를 하나 추가해 둔다.
			this.level_datas.Add(new LevelData());
		}
	}

	// 레벨을 임의로 고른다.
	public void		selectLevel()
	{
		this.select_level = Random.Range(0, this.level_datas.Count);

		//Debug.Log("select level = " + this.select_level.ToString());
	}

	// 현재 레벨의 레벨 데이터를 가져온다.
	public LevelData	getCurrentLevelData()
	{
		return(this.level_datas[this.select_level]);
	}

	// 블록의 연소시간을 가져온다.
	public float	getVanishTime()
	{
		return(this.level_datas[this.select_level].heat_time);
	}
}
