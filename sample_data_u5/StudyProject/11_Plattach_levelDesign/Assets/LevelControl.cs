using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelData {
	public float[] probability; // 블록의 출현빈도를 저장하는 배열.
	public float heat_time; // 연소시간.
	public LevelData() // 생성자.
	{
		// 블록의 종류 수와 같은 크기로 데이어 영역을 확보.
		this.probability = new float[(int)Block.COLOR.NORMAL_COLOR_NUM];
		// 모든 종류의 출현확률을 우선 균등하게 해둔다.
		for(int i = 0; i < (int)Block.COLOR.NORMAL_COLOR_NUM; i++) {
			this.probability[i] =
				1.0f / (float)Block.COLOR.NORMAL_COLOR_NUM;
		}
	}
	// 모든 종류의 출현확률을 0으로 리셋하는 메소드.
	public void clear()
	{
		for(int i = 0;i < this.probability.Length; i++) {
			this.probability[i] = 0.0f;
		}
	}
	// 모든 종류의 출현확률의 합계를 100%(=1.0)로 하는 메소드.
	public void normalize()
	{
		float sum = 0.0f;
		// 출현확률의 '임시 합계값'을 계산한다.
		for(int i = 0; i < this.probability.Length; i++) {
			sum += this.probability[i];
		}
		for(int i = 0;i < this.probability.Length; i++) {
			// 각각의 출현확률을 '임시 합계값'으로 나누면, 합계가 100%(=1.0) 딱 떨어진다.
			this.probability[i] /= sum;
			// 만약 그 값이 무한대라면 .
			if(float.IsInfinity(this.probability[i])) {
				this.clear(); // 모든 확률을 0으로 리셋하고.
				this.probability[0] = 1.0f; // 최초의 요소만 1.0으로 해둔다.
				break; // 그리고 루프를 빠져나간다.
			}
		}
	}
}


public class LevelControl {
	private List<LevelData> level_datas = null; // 각 레벨의 레벨 데이터.
	private int select_level = 0; // 선택된 레벨.

	public void initialize()
	{
		// List를 초기화.
		this.level_datas = new List<LevelData>();
	}

	public void loadLevelData(
		TextAsset level_data_text)
	{
		// 텍스트 데이터를 문자열로서 받아들인다.
		string level_texts = level_data_text.text;
		// 개행 코드'\'마다 나누어, 문자열 배열에 집어넣는다.
		string[] lines = level_texts.Split('\n');
		// lines 안의 각 행에 대하여 차례로 처리해가는 루프.
		foreach(var line in lines) {
			if(line == "") { // 행이 비었으면.
				continue; // 아래 처리는 하지 않고 루프의 처음으로 점프.
			}
			string[] words = line.Split(); // 행 내의 워드를 배열에 저장.
			int n = 0;
			// LevelData형 변수를 작성.
			// 여기에 현재 처리하는 행의 데이터를 넣는다.
			LevelData level_data = new LevelData();
			// words내의 각 워드에 대해서, 순서대로 처리해 가는 루프.
			foreach(var word in words) {
				if(word.StartsWith("#")) { // 워드의 시작 문자가 #이면.
					break; // 루프 탈출.
				}
				if(word == "") { // 워드가 비었으면.
					continue; // 루프 시작으로 점프.
				}
				// 'n'의 값을 0,1,2,...6으로 변화시켜감으로써 일곱 개 항목을 처리.
				// 각 워드를 float값으로 변환하고 level_data에 저장. 
				switch(n) {
				case 0:
					level_data.probability[(int)Block.COLOR.PINK] =
						float.Parse(word); break;
				case 1:
					level_data.probability[(int)Block.COLOR.BLUE] =
						float.Parse(word); break;
				case 2:
					level_data.probability[(int)Block.COLOR.GREEN] =
						float.Parse(word); break;
				case 3:
					level_data.probability[(int)Block.COLOR.ORANGE] =
						float.Parse(word); break;
				case 4:
					level_data.probability[(int)Block.COLOR.YELLOW] =
						float.Parse(word); break;
				case 5:
					level_data.probability[(int)Block.COLOR.MAGENTA] =
						float.Parse(word); break;
				case 6:
					level_data.heat_time =
						float.Parse(word); break;
				}
				n++;
			}
			if(n >= 7) { // 8항목(이상)이 제대로 처리되었다면.
				// 출현 확률의 합계가 정확히 100%가 되도록 하고 나서.
				level_data.normalize();
				// List 구조의 level_datas에 level_data를 추가한다.
				this.level_datas.Add(level_data);


			} else { // 그렇지 않으면(오류 가능성이 있다).
				if(n == 0) { // 1워드도 처리하지 않은 경우는 주석이므로.
					// 문제 없음. 아무것도 하지 않는다.
				} else { // 그 이외라면 오류.
					// 데이터의 개수가 맞지 않는다는 오류 메시지를 표시.
					Debug.LogError("[LevelData] Out of parameter.\n");
				}
			}
		}


		// level_datas에 데이터가 하나도 없으면.
		if(this.level_datas.Count == 0) {
			// 오류 메시지를 표시.
			Debug.LogError("[LevelData] Has no data.\n");
			// level_datas에 LevelData를 하나 추가해 둔다.
			this.level_datas.Add(new LevelData());
		}
	}
	public void selectLevel()
	{
		// 0~패턴 사이의 값을 임의로 선택.
		this.select_level = Random.Range(0, this.level_datas.Count);
		Debug.Log("select level = " + this.select_level.ToString());
	}
	public LevelData getCurrentLevelData()
	{
		// 선택된 패턴의 레벨 데이터를 반환한다.
		return(this.level_datas[this.select_level]);
	}
	public float getVanishTime()
	{
		// 선택된 패턴의 연소시간을 반환한다.
		return(this.level_datas[this.select_level].heat_time);
	}
}











