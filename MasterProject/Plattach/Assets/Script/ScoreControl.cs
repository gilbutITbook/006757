using UnityEngine;
using System.Collections;

// 점수 관리.
// 콤보 계산 등도 이곳에서 할 예정.
public class ScoreControl : MonoBehaviour {

	public struct Score {
		public int		score;		// 점수.
		public int		coins;		// 코인 개수.
	};

	//private ScoreDisp		score_disp = null;			// 점수 표시.

	// ================================================================ //

	//private static float	POS_X 	= 0.0f;			// 표시 위치.
	//private static float	POS_Y 	= 10.0f;			// 표시 위치.

	private Score		score;							// 현재 점수ー.

	// ================================================================ //

	public static ScoreControl	getInstance()
	{
		ScoreControl	score_control = GameObject.FindGameObjectWithTag("GameRoot").GetComponent<ScoreControl>();
		return(score_control);
	}

	// ================================================================ //
	// MonoBehaviour에서의 상속.

	void	Start()
	{
		this.score.score = 100;
		this.score.coins = 0;

		//this.score_disp = GameObject.FindGameObjectWithTag("Score Disp").GetComponent<ScoreDisp>();
	}
	
	void	Update()
	{
	
	}

	void	OnGUI()
	{
		//this.score_disp.dispNumber(new Vector2(POS_X, POS_Y), this.score.score, 64.0f);
	}


	// ================================================================ //

	// 현재 점수를 가져온다.
	public ScoreControl.Score	getCurrentScore()
	{
		return(this.score);
	}
}
