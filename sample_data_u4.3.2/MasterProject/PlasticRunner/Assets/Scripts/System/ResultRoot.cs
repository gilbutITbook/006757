using UnityEngine;
using System.Collections;

// 타이틀 화면 시퀀스.
public class ResultRoot : MonoBehaviour {

	private static float	RESULT_SCORE_POS_X 	= 640.0f/2.0f -64.0f;		// 결과 화면　점수 표시 위치.
	private static float	RESULT_SCORE_POS_Y 	= 120.0f;					// 결과 화면　점수 표시 위치.

	private static float	RESULT_HIGH_SCORE_POS_X = 640.0f/2.0f -64.0f;	// 결과 화면　최고 점수 표시 위치.
	private static float	RESULT_HIGH_SCORE_POS_Y	= 240.0f;				// 결과 화면　최고 점수 표시 위치.

	private static float	RESULT_HIGH_COIN_POS_X = 640.0f/2.0f -64.0f;	// 결과 화면　최고 점수 코인 표시 위치.
	private static float	RESULT_HIGH_COIN_POS_Y = 340.0f;				// 결과 화면  최고 점수 코인 표시 위치.

	// ---------------------------------------------------------------- //

	public Texture	result_texture = null;
	public Texture	next_texture   = null;

	private ScoreDisp		score_disp = null;		// 점수 표시.

	// ---------------------------------------------------------------- //

	private ScoreControl.Score		high_score;				// 최고 점수.
	private ScoreControl.Score		last_socre;				// 이번 점수.

	private int 	disp_last_score = 0;	// 표시용.

	// ---------------------------------------------------------------- //

	private SoundControl sound_control = null;


	public enum STEP {

		NONE = -1,

		RESULT = 0,				// 결과
		RESULT_ACTION,		// 결과 화면에서 클릭된 후의 연출.
		TITLE,				// 타이틀 화면으로.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	static private	float	ACTION_TIME = 1.0f;

	// ================================================================ //
	// MonoBehaviour에서 상속. 

	void	Start()
	{
		this.score_disp = GameObject.FindGameObjectWithTag("Score Disp").GetComponent<ScoreDisp>();
		this.high_score = GlobalParam.getInstance().getHighScore();
		this.last_socre = GlobalParam.getInstance().getLastScore();
		this.next_step = STEP.RESULT;

		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();
	}

	void	Update()
	{

		// -------------------------------------------------------------------- //
		// 스텝 내의 경과 시간을 진행한다.

		this.step_timer += Time.deltaTime;

		// -------------------------------------------------------------------- //
		// 다음 상태로 갈지 체크한다.


		if(this.next_step == STEP.NONE) {

			switch(this.step) {

				case STEP.RESULT:
				{
					if(Input.GetMouseButtonDown(0)) {

						this.next_step = STEP.RESULT_ACTION;
						this.sound_control.playSound(Sound.SOUND.CLICK);

					}
				}
				break;

				case STEP.RESULT_ACTION:
				{
					if(this.step_timer > ACTION_TIME) {

						this.next_step = STEP.TITLE;
					}
				}
				break;
			}
		}

		// -------------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.next_step != STEP.NONE) {

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			switch(this.step) {
	
				case STEP.RESULT:
				{
					// 최고 점수를 보존.
					GlobalParam.getInstance().saveSaveData();

					this.sound_control.playBgm(Sound.BGM.RESULT);
				}
				break;

				case STEP.RESULT_ACTION:
				{
					this.sound_control.stopBgm();
					this.disp_last_score = this.last_socre.score;
				}
				break;



				case STEP.TITLE:
				{
					Application.LoadLevel("TitleScene");
				}
				break;
			}

			this.step_timer = 0.0f;
		}

		// -------------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step) {

			case STEP.RESULT:
			this.disp_last_score += (int)(100 *Time.deltaTime);
			this.disp_last_score = Mathf.Clamp(this.disp_last_score, 0, this.last_socre.score);
			if(this.disp_last_score < this.last_socre.score) {
				this.sound_control.playSound(Sound.SOUND.COIN_GET);
			}
				break;
		}
	}

	void	OnGUI()
	{
		Rect	rect = new Rect();

		// -------------------------------------------------------------------- //
		// 배경.

		Texture		back_texture;

		switch(this.step) {
			default:
			{
				back_texture = this.result_texture;
			}
			break;
		}

		rect.x = 0.0f;
		rect.y = 0.0f;
		rect.width  = back_texture.width;
		rect.height = back_texture.height;

		GUI.DrawTexture(rect, back_texture);

		// -------------------------------------------------------------------- //
		// 넥스트 버튼.

		float	scale = 1.0f;

		if(this.step == STEP.RESULT_ACTION) {

			// 클릭되면 순간적으로 이렇게 된다(적당).

			scale = this.step_timer/(ACTION_TIME/4.0f);
			scale = Mathf.Min(scale, 1.0f);
			scale = Mathf.Sin(scale*Mathf.PI);

			scale = Mathf.Lerp(1.0f, 1.2f, scale);
		}

		rect.width  = this.next_texture.width*scale;
		rect.height = this.next_texture.height*scale;

		rect.x = Screen.width*0.9f - rect.width/2.0f;
		rect.y = Screen.height*0.9f - rect.height/2.0f;

		GUI.DrawTexture(rect, this.next_texture);

		// -------------------------------------------------------------------- //
		// 점수, 코인 수 등.

		switch(this.step) {
			case STEP.RESULT:
			case STEP.RESULT_ACTION:
			case STEP.TITLE:
			{
			this.score_disp.dispNumber(new Vector2(RESULT_SCORE_POS_X, RESULT_SCORE_POS_Y), this.disp_last_score);// this.last_socre.score);
				this.score_disp.dispNumber(new Vector2(RESULT_HIGH_SCORE_POS_X, RESULT_HIGH_SCORE_POS_Y), this.high_score.score);
				this.score_disp.dispNumber(new Vector2(RESULT_HIGH_COIN_POS_X, RESULT_HIGH_COIN_POS_Y), this.high_score.coins);
			}
			break;
		}
	}
}
