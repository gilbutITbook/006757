using UnityEngine;
using System.Collections;

// 타이틀 화면 시퀀스.
public class TitleRoot : MonoBehaviour {

	public Texture	title_texture = null;
	public Texture	start_texture = null;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		LOAD_SAVE_DATA = 0,	// 세이브 데이터 로딩.
		WAIT_CLICK,			// 클릭 대기.
		START_ACTION,		// 클릭된 뒤의 연출.
		GAME_START,			// 게임 시작.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	static private	float	START_ACTION_TIME = 1.0f;

	private SoundControl	sound_control = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.next_step = STEP.LOAD_SAVE_DATA;

		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();
	}

	void	Update()
	{

		// ---------------------------------------------------------------- //
		// 스텝 내의 경과 시간을 진행한다.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 당ㅁ 상태로 이동할지 체크한다.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.LOAD_SAVE_DATA:
				{
					this.next_step = STEP.WAIT_CLICK;
				}
				break;

				case STEP.WAIT_CLICK:
				{
					if(Input.GetMouseButtonDown(0)) {

						this.next_step = STEP.START_ACTION;
						this.sound_control.playSound(Sound.SOUND.CLICK);
					}
				}
				break;

				case STEP.START_ACTION:
				{
					if(this.step_timer > START_ACTION_TIME) {

						this.next_step = STEP.GAME_START;
					}
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
	
				case STEP.LOAD_SAVE_DATA:
				{
					// 세이브 데이터를 읽는다(최초에만).
					GlobalParam.getInstance().loadSaveData();
				}
				break;

				case STEP.GAME_START:
				{
					Application.LoadLevel("GameScene");
				}
				break;
			}

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step) {

			case STEP.WAIT_CLICK:
			{
			}
			break;
		}
	}

	void	OnGUI()
	{
		Rect	rect = new Rect();

		// 배경.

		rect.x = 0.0f;
		rect.y = 0.0f;
		rect.width  = this.title_texture.width;
		rect.height = this.title_texture.height;

		GUI.DrawTexture(rect, this.title_texture);

		// 시작 버튼.

		if(this.step != STEP.LOAD_SAVE_DATA) {

			float	scale = 1.0f;
	
			if(this.step == STEP.START_ACTION) {
	
				// 클릭되면 순간에 이렇게 된다(적당).
	
				scale = this.step_timer/(START_ACTION_TIME/4.0f);
				scale = Mathf.Min(scale, 1.0f);
				scale = Mathf.Sin(scale*Mathf.PI);
				scale = Mathf.Lerp(1.0f, 1.2f, scale);
			}
	
			rect.width  = this.start_texture.width*scale;
			rect.height = this.start_texture.height*scale;
			rect.x = Screen.width*0.8f  - rect.width/2.0f;
			rect.y = Screen.height*0.9f - rect.height/2.0f;

			GUI.DrawTexture(rect, this.start_texture);
		}
	}
}
