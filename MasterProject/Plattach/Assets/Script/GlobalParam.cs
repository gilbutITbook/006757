using UnityEngine;
using System.Collections;

// 씬을 넘어서 사용하고 싶은 파라미터.
public class GlobalParam : MonoBehaviour {
	

	public struct Score {
		public float	score;		// 점수.
		public int		ignit;		// 발화수.
	};

	// ================================================================ //

	private Score		high_score;				// 최고 점수.
	private Score		last_socre;				// 이번 점수.	

	// ================================================================ //

	public void		initialize()
	{
		this.high_score.score = 99;
		this.high_score.ignit = 0;

		this.last_socre.score = 99;
		this.last_socre.ignit = 0;
	}

	// 게임 오버 시에 이번 점수를 설정한다.
	public void		setLastScore(float s, int i)
	{
		this.last_socre.score = s;
		this.last_socre.ignit = i;

		// 최고 점수 갱신 체크.
		this.high_score.score = Mathf.Min(this.high_score.score, this.last_socre.score);
		this.high_score.ignit = Mathf.Max(this.high_score.ignit, this.last_socre.ignit);
	}

	// 최고 점수를 가져온다.
	public Score	getHighScore()
	{
		return(this.high_score);
	}

	// 이번 스코어를 가져온다.
	public Score	getLastScore()
	{
		return(this.last_socre);
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
