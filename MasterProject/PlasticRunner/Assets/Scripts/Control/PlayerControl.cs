using UnityEngine;
using System.Collections;

// 플레이어.
public class PlayerControl : MonoBehaviour {

	public static float	ACCELERATION = 10.0f;				// 가속도.
	public static float	SPEED_MIN = 4.0f;					// 속도의 쵯솟값.
	public static float	SPEED_MAX = 8.0f;					// 속도의 최댓값.
	
	public static float	JUMP_HEIGHT_MAX = 3.0f;				// 점프 높이.
	public static float	JUMP_KEY_RELEASE_REDUCE = 0.5f;		// 점프 중에 키를 놓았을 때에 상승속도 감소.
	
	public static float	BIKKURI_HEIGHT_MAX = 2.0f;			// 방해 캐릭터에 닿았을 때 뛰어오르는 높이.

	public static float	NARAKU_HEIGHT = -5.0f;				// 화면 아래로 사라졌다고 간주할 수 있는 높이.

	public static float	COLLISION_SIZE = 1.0f;				// 충돌 구체의 크기(지름).

	public static float	COIN_DROP_INTERVAL = 0.2f;			// [sec]점프 중에 코인을 떨어뜨리는 간격.

	private static float	CLICK_GRACE_TIME = 0.3f;		// 클릭 후, 점프할 수 있을 때까지의 유예시간.

	// ---------------------------------------------------------------- //

	private	ScoreControl	score_control = null;		// 점수 관리.
	private CoinCreator		coin_creator  = null;		// 코인 크리에이터.
	public  LevelControl	level_control = null;

	public	float		passage_time = 0.0f;
	public	float		current_speed = 0.0f;			// 현재 속도.
	private bool		is_landed = false;				// 착지했는가?.
	private bool		is_colided = false;
	private bool		is_launched = false;			// 점프 직후, '착지 판정'이 사라졌다?

	private bool		is_key_released = false;		// 점프 후 스페이스키를 뗐는가?.

	private	float		drop_timer = 0.0f;				// 코인 떨어뜨리기용 타이머.

	private	float		click_timer = -1.0f;			// 클릭된 후의 시간.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		RUN = 0,			// 달리기.
		JUMP,				// 점프.
		MISS,				// 실수.

		TOUCH_ENEMY,		// 방해 캐릭터에 닿았다.
		OUT,				// 방해 캐릭터에 닿은 뒤, 화면 아래로 사라졌다.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;


	private Animation 		anim_player;		// motion.

	private SoundControl	sound_control = null;

	private BlockControl	stepped_block = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.next_step     = STEP.RUN;
		this.current_speed = SPEED_MIN;

		this.score_control = ScoreControl.getInstance();
		this.coin_creator  = CoinCreator.getInstance();

		this.anim_player = this.transform.GetComponentInChildren<Animation>();		//motion.

		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();

	}

	private	bool	is_debug_immortal = false;

	// Update is called once per frame
	void	Update()
	{
		Vector3		velocity = this.rigidbody.velocity;

		// ---------------------------------------------------------------- //
		// 클릭 후의 경과 시간.
		//
		// 착지 직후에 버튼을 클릭해도 캐릭터가 점프해 주지 않는 경우가 있다.
		// 원인은 착지 직후에 버턴을 클릭했다고 생각하지만 실제로는 
		// 착지 직전에 클릭하기 때문이다.
		// 이런 때라도 점프할 수 있도록 '클릭한 순간'의 판정이 
		// 몇 프레임에 이어지도록 한다.
		// 
		if(Input.GetMouseButtonDown(0)) {

			this.click_timer = 0.0f;

		} else {

			if(this.click_timer >= 0.0f) {

				this.click_timer += Time.deltaTime;
			}
		}

		// ---------------------------------------------------------------- //
		// 스피드 컨트롤.

		this.current_speed = this.level_control.getPlayerSpeed();

		// --------------------------------------------------------------- //
		// 유니티 처리 엔진의 특성?으로 바닥 블록의 이음메에서 울퉁불퉁 뛰어오르므로
		// 억지로 아래로 눌러 붙인다.

		if(this.step == STEP.TOUCH_ENEMY) {

		} else {

			if(this.is_colided) {

				if(velocity.y > Physics.gravity.y*Time.deltaTime) {

					velocity.y = Physics.gravity.y*Time.deltaTime;

					this.rigidbody.velocity = velocity;
				}
			}
	
			// '착지했는가？' 조사한다.
			// (원래는 유니티의 기능을 사용하면 충분하지만 밀어내기로
			//  공중에 떠버리는 경우도 있으니 각자 조사한다).
			//
			this.check_landed();
		}

		// ---------------------------------------------------------------- //
		// 화면 아래로 떨어지면 실패.

		switch(this.step) {

			case STEP.RUN:
			case STEP.JUMP:
			{
				if(this.transform.position.y < NARAKU_HEIGHT) {

					this.next_step = STEP.MISS;
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 스텝 내의 경과 시간을 진행한다.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 검사한다.


		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.RUN:
				{
					// 마우스의 왼쪽 버튼이 눌렸으면 점프.
					if(0.0f <= this.click_timer && this.click_timer <= CLICK_GRACE_TIME) {

						if(this.is_landed || this.is_colided) {

							this.click_timer = -1.0f;
							this.next_step = STEP.JUMP;

						} else {

							// 발밑에 바닥이 없을 때는 점프할 수 없다. 
						}
					}
				}
				break;

				case STEP.JUMP:
				{
					// 착지했으면 달리기로.
					if(this.is_launched) {

						if(this.is_landed) {

							this.sound_control.playSound(Sound.SOUND.TDOWN);

							if(this.stepped_block != null) {

								this.stepped_block.onStepped();
							}

							this.next_step = STEP.RUN;
						}
					}
				}
				break;

				case STEP.TOUCH_ENEMY:
				{
					if(this.transform.position.y < NARAKU_HEIGHT) {

						this.next_step = STEP.OUT;
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
	
				case STEP.RUN:
				{
					this.is_launched = false;
					this.anim_player.CrossFade("02_Move", 0.1f);		//motion
				}
				break;

				case STEP.JUMP:
				{
					velocity.y = PlayerControl.calcJumpVelocityY();

					this.is_launched     = false;
					this.is_key_released = false;
					this.anim_player.CrossFade("03_jumpup", 0.1f);		//motion
					
					this.sound_control.playSound(Sound.SOUND.JUMP);

					// 점프 시의 음성은 jump1 jump2를 임의로 계속 재생하고 30％의 확률로 재생되지 않을 정도로 조정한다.
					int rnd = Random.Range(0,3);
					switch(rnd){
					case 0:	this.sound_control.playSound(Sound.SOUND.JUMP1);	break;
					case 1:	this.sound_control.playSound(Sound.SOUND.JUMP2);	break;
					}

				}
				break;

				case STEP.TOUCH_ENEMY:
				{
					// 적에게 닿았다.

					// 화면 아래로 사라져가도록 충돌을 무효로 한다.
					this.collider.enabled = false;

					this.next_step = STEP.MISS;
				}
				break;

				case STEP.MISS:
				{
					velocity.y = PlayerControl.calcBikkuriVelocityY();

					velocity.x = 0.0f;

					this.collider.enabled = false;

					this.anim_player.CrossFade("05_died", 0.1f);		//motion

					this.sound_control.playSound(Sound.SOUND.JINGLE);
				}
				break;
			}

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리 

		switch(this.step) {

			case STEP.RUN:
			{
				// 오른쪽 방향으로 가속 
				velocity.x += PlayerControl.ACCELERATION*Time.deltaTime;

				// 최대 속도 이상이 되지 않도록.
				if(Mathf.Abs(velocity.x) > this.current_speed) {
					velocity.x *= this.current_speed/Mathf.Abs(velocity.x);
				}
			}
			break;

			case STEP.JUMP:
			{
				if(!this.is_landed || this.is_key_released || velocity.y <= 0.0f) {

					this.is_launched = true;
				}

				// 점프 중에 왼쪽 버튼을 떼면, 상승 속도를 줄인다.
				// (왼쪽 버튼을 누른 길이로 점프 높이를 제어할 수 있게 한다.)
				do {

					if(Input.GetMouseButton(0)) {

						break;
					}

					// 한 번 버튼에서 손을 떼면 다음은 하지 않는다(연타대책).
					if(this.is_key_released) {

						break;
					}

					// 하강 중엔 하지 않는다.
					if(velocity.y <= 0.0f) {

						break;
					}

					//
				
					velocity.y *= JUMP_KEY_RELEASE_REDUCE;

					this.is_key_released = true;

				} while(false);

				if(velocity.y <= 0.0f){

					this.anim_player.CrossFade("04_jumpdown", 0.3f);	// motion.
				}

			}
			break;

			case STEP.MISS:
			{
				// 감속.
				velocity.x -= PlayerControl.ACCELERATION*Time.deltaTime;

				// 0.0 이하가 되지 않도록.
				if(velocity.x < 0.0f) {

					velocity.x = 0.0f;
				}
			}
			break;

			case STEP.TOUCH_ENEMY:
			{
				velocity.x = 0.0f;
			}
			break;

		}

		// '점프 중 코인을 떨어뜨린다'의 컨트롤.
		this.coin_drop_control();

		// ---------------------------------------------------------------- //

		this.rigidbody.velocity = velocity;

		this.passage_time += Time.deltaTime;

		this.is_colided = false;
		//this.is_landed = false;

		// ---------------------------------------------------------------- //
		// 디버그용 무적 모드.
	#if true
		if(Input.GetMouseButtonDown(1)) {
			this.is_debug_immortal = !this.is_debug_immortal;
			if(this.is_debug_immortal) {
				this.collider.enabled     = false;
				this.rigidbody.useGravity = false;
				this.rigidbody.velocity   = new Vector3(this.rigidbody.velocity.x, 0.0f, this.rigidbody.velocity.z);
			} else {
				this.collider.enabled     = true;
				this.rigidbody.useGravity = true;
			}
		}
	#endif
	}

	// ---------------------------------------------------------------- //

	// 충돌에 해당하는 동안 호출되는 메소드.
	void 	OnCollisionStay(Collision other)
	{
		do {
			if(other.gameObject.tag != "Floor") {
				break;
			}

			// 상승중이면 하지않는다.
			// 점프한 순간에 착지한 게 되지 않도록.
			// 상대 속도가 아래 방향＝자신이 블록으로부터 위를 향하고 있다 = 상승 중.
			if(other.relativeVelocity.y <= 0.0f) {
	
				break;
			}

			if(this.step == STEP.TOUCH_ENEMY) break;
			if(this.step == STEP.MISS) break;
			if(this.step == STEP.OUT) break;

			//this.is_landed = true;
			this.is_colided = true;

		} while(false);
	}

	// ================================================================ //
	
	// 방해 캐릭터에 닿았을 때 호출된다.
	public void		onTouchEnemy(EnemyControl enemy)
	{
		do{

			if(this.step == STEP.TOUCH_ENEMY) break;
			if(this.step == STEP.MISS) break;
			if(this.step == STEP.OUT) break;

			this.next_step = STEP.TOUCH_ENEMY;

			Debug.Log("miss");

		}while(false);
	}

	// 게임오버?.
	public bool		isPlayEnd()
	{
		bool	ret = false;

		switch(this.step) {

			case STEP.MISS:
			case STEP.OUT:
			{
				ret = true;
			}
			break;
		}

		return(ret);
	}

	// 점프중?.
	public bool		isJumping()
	{
		bool	ret = false;

		switch(this.step) {

			case STEP.JUMP:
			{
				ret = true;
			}
			break;
		}

		return(ret);
	}

	// ================================================================ //

	// 점프할 때의 속도(위 방향)를 구한다.
	public static float		calcJumpVelocityY()
	{
		// JUMP_HEIGHT_MAX 높이까지 점프할 수 있는 속도를 구한다.

		float	glavity = Mathf.Abs(Physics.gravity.y);
		float	vy      = Mathf.Sqrt(2.0f*glavity*JUMP_HEIGHT_MAX);

		return(vy);
	}

	// 적에게 당한 뒤의 속도(위 방향)를 구한다.
	public static float		calcBikkuriVelocityY()
	{
		// JUMP_HEIGHT_MAX 높이까지 점프할 수 있는 속도를 구한다.

		float	glavity = Mathf.Abs(Physics.gravity.y);
		float	vy      = Mathf.Sqrt(2.0f*glavity*BIKKURI_HEIGHT_MAX);

		return(vy);
	}

	// ================================================================ //

	// 착지했는지 조사한다.
	private void	check_landed()
	{
		this.is_landed = false;

		do {

			// 바로 아래를 향해서 선을 늘려서 다른 오브젝트와 충돌하는가
			// 조사한다

			float		line_length;

			line_length = COLLISION_SIZE;

			// 점프 직후.
			if(this.step == STEP.JUMP) {

				if(this.is_launched) {

					line_length = COLLISION_SIZE/2.0f;
				}
			}

			Vector3		s = this.transform.position;
			Vector3		e = s + Vector3.down*line_length;
			RaycastHit	hit;

			// 레이어 마스크를 지정해서 바닥만 조사하게 한다.
			// (코인 등은 조사하지 않는다).

			int		layer_mask = 0;

			layer_mask += 1 << LayerMask.NameToLayer("Floor Block");

			if(!Physics.Linecast(s, e, out hit, layer_mask)) {

				// 다른 오브젝트로와 충돌하지 않았다.
				break;
			}

			// 블록이면 기억해 둔다.

			BlockControl	block = hit.collider.GetComponent<BlockControl>();

			if(block != null) {

				this.stepped_block = block;
			}

			//

			this.is_landed = true;

		} while(false);
	}

	// '점프 중 코인을 떨어뜨린다'는 컨트롤.
	// 점프 중, 일정한 시간마다 코인을 떨어뜨린다.
	private void	coin_drop_control()
	{
		float	drop_timer_prev = this.drop_timer;
		if(this.step == STEP.JUMP) {
			this.drop_timer += Time.deltaTime;
		} else {
			drop_timer_prev = 0.0f;
			this.drop_timer = 0.0f;
		}

		// drop_timer가 "COIN_DROP_INTERVAL"을 넘으면 코인을 떨어뜨린다.
		//
		// drop_timer_prev = COIN_DROP_INTERVAL*n
		// drop_timer      = COIN_DROP_INTERVAL*(n + 1)
		//
		// 가 되었을 때, drop_timer가 "COIN_DROP_INTERVAL"의 간격을 넘었을 때.
		//
		if(Mathf.Ceil(this.drop_timer/COIN_DROP_INTERVAL) > Mathf.Ceil(drop_timer_prev/COIN_DROP_INTERVAL)) {

			this.score_control.subDropScore();
			this.coin_creator.createDroppedCoin(this.transform.position);
		}
	}
}
