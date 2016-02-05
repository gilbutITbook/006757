using UnityEngine;
using System.Collections;

// 게임 진행 관리.
public class GameRoot : MonoBehaviour {

	private PlayerControl	player = null;
	private ScoreControl	score_control = null;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		PLAY = 0,		// 게임 중.
		WAIT_CLICK,		// 게임 오버 후, 클릭 대기.
		RESULT,			// 결과.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	private SoundControl	sound_control = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

		this.score_control = this.gameObject.GetComponent<ScoreControl>();
		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();

		this.next_step = STEP.PLAY;
	}
	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 스텝 내의 경과 시간을 진행한다.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이행할지 체크한다.


		if(this.next_step == STEP.NONE) {

			switch(this.step) {

				case STEP.PLAY:
				{
					if(this.player.isPlayEnd()) {

						this.next_step = STEP.WAIT_CLICK;
					}
				}
				break;

				case STEP.WAIT_CLICK:
				{
					if(Input.GetMouseButtonDown(0)) {

						this.next_step = STEP.RESULT;
					}
				}
				break;

			}
		}

		// ---------------------------------------------------------------- //
		// 상태를 전환했을 때의 초기화.

		while(this.next_step != STEP.NONE) {

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			switch(this.step) {

				case STEP.PLAY:
				{
					this.sound_control.playBgm(Sound.BGM.PLAY);
				}
				break;

				case STEP.WAIT_CLICK:
				{
					this.sound_control.stopBgm();
				}	
				break;

				case STEP.RESULT:
				{
					// 이번 스코어를 기록해 둔다.
					ScoreControl.Score	score = this.score_control.getCurrentScore();
					GlobalParam.getInstance().setLastScore(score);
					Application.LoadLevel("ResultScene");
				}
				break;
			}

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step) {

			case STEP.PLAY:
			{
			}
			break;
		}
	}

	// ================================================================ //

	public float	getPlayTime()
	{
		float	time;

		if(this.step == STEP.PLAY) {

			time = this.step_timer;

		} else {

			time = 0.0f;
		}

		return(time);
	}
}
