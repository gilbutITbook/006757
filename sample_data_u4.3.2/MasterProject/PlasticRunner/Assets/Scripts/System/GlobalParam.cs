using UnityEngine;
using System.Collections;

// 씬을 넘어서 사용하고 싶은 파라미터.
public class GlobalParam : MonoBehaviour {
	
	// ================================================================ //

	private SaveData	save_data = null;

	public bool	save_data_loaded = false;				// 세이브 데이터를 로드했는가?.

	private ScoreControl.Score		high_score;				// 최고 점수.
	private ScoreControl.Score		last_socre;				// 이번 점수.	

	// ================================================================ //

	public void		initialize()
	{
		this.high_score.score = 100;
		this.high_score.coins = 0;

		this.last_socre.score = 100;
		this.last_socre.coins = 0;

		// 세이브 데이터(를 읽고쓰기 위한 오브젝트)를 만들어 둔다.
		this.create_save_data();
	}

	// 게임 오버 시에 이번 점수를 설정한다.
	public void		setLastScore(ScoreControl.Score last_score)
	{
		this.last_socre = last_score;

		// 최고 점수 - 갱신 체크.
		this.high_score.score = Mathf.Max(this.high_score.score, this.last_socre.score);
		this.high_score.coins = Mathf.Max(this.high_score.coins, this.last_socre.coins);
	}

	// 최고 점수를 가져온다.
	public ScoreControl.Score	getHighScore()
	{
		return(this.high_score);
	}

	// 이번 점수를 가져온다.
	public ScoreControl.Score	getLastScore()
	{
		return(this.last_socre);
	}

	// ================================================================ //
	// 저장 데이터.

	// 저장 데이터를 로드한다(첫번째만).
	public void		loadSaveData()
	{
		if(!this.save_data_loaded) {

			// 실제로 로드하는 것은 처음에 한 번만(속도문제).
			this.save_data.load();
			this.save_data_loaded = true;

			this.high_score.score = this.save_data.getInt("Hi-Score",  this.high_score.score);
			this.high_score.coins = this.save_data.getInt("Max-Coins", this.high_score.coins);

			foreach(SaveData.Item item in this.save_data.items) {

				DebugWindow.get().add_text(item.name + " " + item.value);
			}
		}
	}

	// 세이브 데이터를 저장한다.
	public void		saveSaveData()
	{
		this.save_data.setInt("Hi-Score",  this.high_score.score);
		this.save_data.setInt("Max-Coins", this.high_score.coins);

		this.save_data.save();
	}

	// 세이브 데이터를 얻는다.
	public SaveData		getSaveData()
	{
		return(this.save_data);
	}

	// 세이브 데이터(를 읽고쓰기 위한 오브젝트)를 만든다.
	protected void		create_save_data()
	{
		this.save_data = new SaveData();

		this.save_data.addInt("Hi-Score",  -1);
		this.save_data.addInt("Max-Coins", -1);
	}

	// ================================================================ //

	private static	GlobalParam instance = null;

	public static GlobalParam	getInstance()
	{
		if(instance == null) {

			GameObject	go = new GameObject("GlobalParam");

			instance = go.AddComponent<GlobalParam>();

			instance.initialize();

			DontDestroyOnLoad(go);
		}

		return(instance);
	}

}
