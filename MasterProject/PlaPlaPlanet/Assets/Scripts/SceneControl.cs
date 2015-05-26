using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	private	GameStatus		game_status      = null;
	private PlayerControl	player_control	= null;
	private ItemRoot		item_root		= null;
	
	public enum STEP {
		NONE = -1,
		TITLE = 0,
		TITLE_WAIT,
		PLAY,		// 대기.
		CLEAR,			// 클리어.
		CLEAR_WAIT,
		GAMEOVER,		// 게임 오버.
		GAMEOVER_WAIT,
		WAIT_CLICK,		// 게임 오보 후 클릭 대기.
		RESULT,			// 결과
		RESULT_WAIT,
		NUM,
	};
	
	public STEP		step       = STEP.NONE;
	public STEP		next_step  = STEP.NONE;
	public float	step_timer = 0.0f;
	private	float	clear_time = 0.0f;

	private bool	flag_clear	= false;

	public GUIStyle guistyle;

	// texture.
	public Texture	title_texture = null;
	private Rect 	title_rect = new Rect();

	public Texture	start_texture = null;
	private Rect	start_rect = new Rect();

	public Texture 	result_texture = null;
	public Texture 	gameover_texture = null;
	private Rect	result_rect = new Rect();

	public Texture	next_texture = null;
	private Rect	next_rect = new Rect();

	// scoredisp.
	private ScoreDisp score_disp = null;

	// sound.
	private SoundControl sound_control = null;
	private Animation 	animation;		// (rocket)motion--.

	private static float CLEAR_ROCKET_LUNCH_TIME = 2.0f;
	private static float DEAD_EFFECT_TIME = 4.0f;

	void	Start()
	{
		this.game_status = this.gameObject.GetComponent<GameStatus>();
		this.player_control = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		this.item_root = this.gameObject.GetComponent<ItemRoot>();
		this.player_control.enabled	= false;
		this.item_root.enabled		= false;
		this.game_status.enabled	= false;

		this.score_disp = this.gameObject.GetComponent<ScoreDisp>();

		this.step = STEP.NONE;
		this.next_step = STEP.TITLE;

		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();	// sound.

		this.animation = GameObject.Find("rocket").transform.FindChild("rocket_model").gameObject.GetComponentInChildren<Animation>();		//motion
		this.guistyle.fontSize = 64;

		this.title_rect = new Rect(0, -Screen.height*0.2f, title_texture.width, title_texture.height);
		this.start_rect = new Rect(Screen.width/2-start_texture.width/2, Screen.height*0.8f -start_texture.height/2,start_texture.width,start_texture.height);
		this.result_rect = new Rect(Screen.width/2-result_texture.width/2, Screen.height/2 -result_texture.height/2, result_texture.width, result_texture.height);
		this.next_rect = new Rect(Screen.width/2-next_texture.width/2, Screen.height*0.9f -next_texture.height/2,next_texture.width,next_texture.height);
	}
	
	void	Update()
	{
		this.step_timer += Time.deltaTime;

		// 변화대기.
		if(this.next_step == STEP.NONE) {
			switch(this.step) {
			case STEP.TITLE:
				//if(Input.GetMouseButtonDown(0)){
				if(Input.anyKeyDown){
					this.next_step = STEP.TITLE_WAIT;
				}
				break;
			case STEP.TITLE_WAIT:
				if(this.title_rect.y < -Screen.height){
					this.next_step = STEP.PLAY;
				}
				break;

			case STEP.PLAY:
				if(this.game_status.isGameClear()) {
					this.flag_clear = true;
					this.next_step = STEP.CLEAR;
				}
				if(this.game_status.isGameOver()){
					this.flag_clear = false;
					this.next_step = STEP.GAMEOVER;
				}
				if(this.step_timer >60.0f){
					// this.flag_clear = false;
					// this.next_step = STEP.RESULT;
				}
				break;
			
			case STEP.CLEAR:
				if(this.step_timer > CLEAR_ROCKET_LUNCH_TIME){
					this.next_step = STEP.CLEAR_WAIT;
				}
				break;

			case STEP.CLEAR_WAIT:
				if(this.step_timer > CLEAR_ROCKET_LUNCH_TIME){
					this.next_step = STEP.RESULT;
				}
				break;

			case STEP.GAMEOVER:
				if(this.step_timer > DEAD_EFFECT_TIME){
					this.next_step = STEP.RESULT;
				}
				// this.next_step = STEP.RESULT;
				break;

			case STEP.RESULT:
				if(result_rect.y <= Screen.width/2 -result_texture.height/2){
					if(Input.anyKeyDown){
						Application.LoadLevel("GameScene");
					}
				}
				break;
			}
		}
		
		// 변화 시.
		while(this.next_step != STEP.NONE) {
			Debug.Log("step = "+ this.next_step);
			this.step      = this.next_step;
			this.next_step = STEP.NONE;
			switch(this.step) {
			case STEP.TITLE_WAIT:
				this.sound_control.SoundPlay(Sound.SOUND.CLICK);
				break;

			case STEP.PLAY:
				this.player_control.enabled	= true;
				this.item_root.enabled		= true;
				this.game_status.enabled	= true;
				this.sound_control.playBgm(Sound.BGM.PLAY);
				break;

			case STEP.CLEAR:
				this.player_control.stateControl(PlayerControl.STEP.CLEAR);
				// this.player_control.enabled = false;

				this.item_root.enabled		= false;
				this.game_status.enabled	= false;
				this.animation.Play("02_launchingstart");
				this.sound_control.SoundPlay(Sound.SOUND.JINGLE);
				this.clear_time = this.step_timer;
				break;

			case STEP.CLEAR_WAIT:
				this.animation.Play("03_launching");
				this.sound_control.SoundPlay(Sound.SOUND.ROCKET);
				break;


			case STEP.GAMEOVER:
				this.player_control.stateControl(PlayerControl.STEP.DEAD);
				// this.sound_control.SoundPlay(Sound.SOUND.VO_MISS);
				// this.player_control.enabled = false;
				this.sound_control.stopBgm();
				break;

			case STEP.RESULT:
				this.sound_control.SoundPlay(Sound.SOUND.KIRARIN);

				this.player_control.enabled = false;
				this.item_root.enabled		= false;
				this.game_status.enabled	= false;
				// this.clear_time = this.step_timer;
				result_rect.y = Screen.height *0.8f - result_texture.height/2;
				this.sound_control.stopBgm();
				// this.sound_control.SoundPlay(Sound.SOUND.JINGLE);
				break;
			}
			this.step_timer = 0.0f;
		}
	}
	
	void	OnGUI()
	{
		float pos_x = 250.0f;
		float pos_y = 400.0f;
		Rect	rect = new Rect();
		Vector2 v;
		float	num;

		switch(this.step) {
		case STEP.TITLE:
			GUI.DrawTexture(title_rect, this.title_texture);
			GUI.DrawTexture(start_rect, this.start_texture);
			break;

		case STEP.TITLE_WAIT:
			title_rect.y -= 320.0f *Time.deltaTime;
			GUI.DrawTexture(title_rect, this.title_texture);

			// start_button.
			float scale = 1.0f;
			scale = this.step_timer/(1.0f/4.0f);
			scale = Mathf.Min (scale, 1.0f);
			scale = Mathf.Sin (scale *Mathf.PI);
			scale = Mathf.Lerp (1.0f, 1.2f, scale);
			start_rect.width = this.start_texture.width *scale;
			start_rect.height = this.start_texture.height *scale;
			start_rect.x = Screen.width/2 -start_texture.width/2 *scale;
			start_rect.y = Screen.height*0.8f -start_texture.height/2 *scale;
			GUI.DrawTexture(start_rect, this.start_texture);
			break;

		case STEP.PLAY:
			v = new Vector2(640-150,480-64);
			num = this.step_timer;
			this.score_disp.dispNumber(v, (int)num, 64, 2);
			v.x +=96;
			v.y +=32;
			this.score_disp.dispNumber(v, (int)((num*100)/10%10), 32, 1);
			v.x +=16;
			this.score_disp.dispNumber(v, (int)((num*100)/1%10), 32, 1);

			/*
			GUI.color = Color.black;
			GUI.Label(new Rect(pos_x,pos_y,200,20), this.step_timer.ToString("0.00"), guistyle);
			float blast_time = 30.0f - this.step_timer;
			GUI.Label(new Rect(pos_x,pos_y+64,200,20), blast_time.ToString("0.00"));
			*/
			break;


		case STEP.CLEAR:
			v = new Vector2(640-150,480-64);
			num = this.clear_time;
			this.score_disp.dispNumber(v, (int)num, 64, 2);
			v.x +=96;
			v.y +=32;
			this.score_disp.dispNumber(v, (int)((num*100)/10%10), 32, 1);
			v.x +=16;
			this.score_disp.dispNumber(v, (int)((num*100)/1%10), 32, 1);
			break;

		case STEP.RESULT:
			v = new Vector2(640-150,480-64);
			num = this.clear_time;
			this.score_disp.dispNumber(v, (int)num, 64, 2);
			v.x +=96;
			v.y +=32;
			this.score_disp.dispNumber(v, (int)((num*100)/10%10), 32, 1);
			v.x +=16;
			this.score_disp.dispNumber(v, (int)((num*100)/1%10), 32, 1);

			if(flag_clear){
				if(result_rect.y >Screen.height/2 -result_texture.height/2){
					result_rect.y -= 30.0f *Time.deltaTime;
				}else{
					GUI.DrawTexture(next_rect, this.next_texture);
				}
				GUI.DrawTexture(result_rect, result_texture);

				// clear_time.
				v = new Vector2(result_rect.x +128, result_rect.y +150);
				num = this.clear_time;
				this.score_disp.dispNumber(v, (int)num, 64, 2);
				v.x +=96;
				v.y +=32;
				this.score_disp.dispNumber(v, (int)((num*100)/10%10), 32, 1);
				v.x +=16;
				this.score_disp.dispNumber(v, (int)((num*100)/1%10), 32, 1);

				GUI.color = Color.black;
				// GUI.Label(new Rect(pos_x,pos_y,200,20), "탈출" + this.clear_time.ToString("0.00"), guistyle);

				int time = (int)clear_time;
				if(time >50){
					GUI.Label(new Rect(pos_x, result_rect.y +240, 200,20), "아슬아슬 탈출 축하합니다!");	break;
				}else if(time >40){
					GUI.Label(new Rect(pos_x, result_rect.y +240, 200,20), "탈출 축하합니다!");	break;
				}else if(time >30){
					GUI.Label(new Rect(pos_x, result_rect.y +240, 200,20), "조금만 더 하면! 플라플라 마스터!"); 		break;
				}else{
					GUI.Label(new Rect(pos_x, result_rect.y +240, 200,20), "플라플라마스터!"); 		break;
				}

			}else{
				if(result_rect.y >Screen.height/2 -result_texture.height/2){
					result_rect.y -= 30.0f *Time.deltaTime;
				}else{
					GUI.DrawTexture(next_rect, this.next_texture);
				}
				GUI.DrawTexture(result_rect, gameover_texture);
				
				// GUI.color = Color.black;
				// GUI.Label(new Rect(pos_x,pos_y,200,20), "게임오버", guistyle);
			}
			break;
		}
	}
}
