using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	private	BlockRoot		block_root      = null;
	private ScoreCounter	score_counter	= null;

	// ================================================================ //

	// 게임 상태.
	public enum STEP {
		NONE = -1,
		PLAY = 0,		// 대기.
		CLEAR,			// 클리어.
		CLICK_ACTION,	// 클릭후 연출.
		RESULT,			// 결과 화면.
		NUM,
	};

	public STEP		step       = STEP.NONE;
	public STEP		next_step  = STEP.NONE;
	public float	step_timer = 0.0f;
	private	float	clear_time = 0.0f;

	private ScoreDisp		score_disp = null;			// 점수 표시.
	private SoundControl	sound_control = null;

	public Texture 	icon_time = null;
	public Texture  info_clear = null;
	private float	eff_clear_pos_y = 0.0f;
	public Texture 	next_button = null;

	public float	ACTION_TIME = 1.0f;


	private Animation 	rocket_motion;		// (rocket)motion--.


	// ================================================================ //
	// MonoBehaviour으로부터 상속.

	void	Start()
	{
		this.block_root = this.gameObject.GetComponent<BlockRoot>();
		this.block_root.create();
		this.block_root.initialSetUp();

		this.score_counter = this.gameObject.GetComponent<ScoreCounter>();

		this.next_step = STEP.PLAY;

		this.score_disp = GameObject.FindGameObjectWithTag("Score Disp").GetComponent<ScoreDisp>();

		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();

		this.rocket_motion = GameObject.Find("rocket_model").gameObject.GetComponentInChildren<Animation>();		//motion

	}
	
	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 스텝 내 경과시간 진행.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.

		if(this.next_step == STEP.NONE) {
			switch(this.step) {
			case STEP.PLAY:
				if(this.score_counter.isGameClear()) {
					this.next_step = STEP.CLEAR;
				}
				break;

			case STEP.CLEAR:
				if(eff_clear_pos_y <=0.0f){
					if(Input.GetMouseButtonDown(0)) {
						// this.next_step = STEP.RESULT;
						this.next_step = STEP.CLICK_ACTION;
					}
				}
				break;

			case STEP.CLICK_ACTION:
				if(this.step_timer > ACTION_TIME) {
					this.next_step = STEP.RESULT;
				}
				break;

			}
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.next_step != STEP.NONE) {

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			switch(this.step) {
			case STEP.PLAY:
				this.sound_control.playBgm(Sound.BGM.BGM1);
				break;

			case STEP.CLEAR:
				// 블록 교체 등을 할 수 없도록.
				this.block_root.enabled = false;
				this.clear_time = this.step_timer;
				Debug.Log(this.step_timer +"/"+ this.clear_time);
				this.sound_control.stopBgm();
				eff_clear_pos_y = 32.0f;
				this.rocket_motion.Play("02_launchingstart");

				this.sound_control.playSound(Sound.SOUND.GAME_CLEAR);

				break;

			case STEP.CLICK_ACTION:
				this.rocket_motion.Play("03_launching");
				this.sound_control.playSound(Sound.SOUND.CLICK);
				break;

			case STEP.RESULT:
				// 블록 교체 등을 할 수 없도록.
				this.block_root.enabled = false;
				int ignit = this.score_counter.bestIgnit();
				Debug.Log("CLEAR TIME= "+ this.clear_time);
				GlobalParam.getInstance().setLastScore( this.clear_time, ignit);
				Application.LoadLevel("resultScene");
				break;
			}

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step) {
		case STEP.PLAY:
			break;
		case STEP.CLEAR:

			break;
		}

		// ---------------------------------------------------------------- //
	}

	void	OnGUI()
	{
		float disp_timer = 0.0f;

		switch(this.step) {

		case STEP.PLAY:
			// 시간 표시.
			GUI.DrawTexture(new Rect(190,0,32,32), icon_time);

			disp_timer = this.step_timer;

			break;

		case STEP.CLEAR:

			// 시간 표시.
			GUI.DrawTexture(new Rect(190,0,32,32), icon_time);
			disp_timer = this.clear_time;

			// 클리어 표시--.
			GUI.DrawTexture(new Rect(Screen.width/2 -this.info_clear.width/2,
			                         Screen.height/2 -this.info_clear.height/2 +eff_clear_pos_y,
			                         this.info_clear.width,
			                         this.info_clear.height), this.info_clear);

			// 밑에서 올라온다---.
			if(eff_clear_pos_y >0.0f){
				eff_clear_pos_y -= 32.0f/5.0f *Time.deltaTime;
			}else{
				GUI.DrawTexture(new Rect(Screen.width/2 -this.next_button.width/2,
				                         Screen.height*0.9f -this.next_button.height/2,
				                         this.next_button.width,
				                         this.next_button.height), this.next_button);
			}
			break;

		case STEP.CLICK_ACTION:
			// 시간 표시.
			GUI.DrawTexture(new Rect(190,0,32,32), icon_time);


			disp_timer = this.clear_time;

			// 시작 버튼.
			float	scale = 1.0f;
			scale = this.step_timer/(ACTION_TIME/4.0f);
			scale = Mathf.Min(scale, 1.0f);
			scale = Mathf.Sin(scale*Mathf.PI);
			scale = Mathf.Lerp(1.0f, 1.2f, scale);

			GUI.DrawTexture(new Rect(Screen.width/2 -this.next_button.width/2*scale,
			                         Screen.height*0.9f -this.next_button.height/2*scale,
			                         this.next_button.width*scale,
			                         this.next_button.height*scale), this.next_button);



			break;

		}

		this.score_disp.dispNumber(new Vector2(200, 0), (int)disp_timer, 64.0f, 2);
		this.score_disp.dispNumber(new Vector2(280, 32), (int)(disp_timer*100)/10%10, 32.0f, 1);
		this.score_disp.dispNumber(new Vector2(296, 32), (int)(disp_timer*100)/1%10, 32.0f, 1);


	}
}
