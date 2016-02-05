using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public static float		MOVE_AREA_RADIUS = 15.0f;
	public static float		MOVE_SPEED		 = 7.0f;

	private GameObject	closest_item = null;
	private GameObject	carried_item = null;		//! 운반중인 아이템.
	public ItemRoot	item_root = null;

	private struct Key {
		public bool		up;
		public bool		down;
		public bool		right;
		public bool		left;
		public bool		pick;
		public bool		action;
	};
	private	Key		key;

	public enum STEP {
		NONE = -1,
		MOVE = 0,			// 식물.
		REPAIRING,			// 로켓 수리.
		EATING,				// 식사중.
		FIRE,				// 불에 태우낟.
		PLANT,				// 심는다.
		PICKUP,				// 줍는다.
		THROW,
		DEAD,
		CLEAR,
		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	private GameObject	closest_event = null;
	private EventRoot	event_root = null;
	private GameObject	rocket_model = null;





	private GameStatus	game_status = null;			// status
	Animation animation;		// motion
	private SoundControl sound_control	 = null;

	private static float	SE_RUN_TIMING = 0.3f;
	private static float	SE_REPAIR_TIMING = 0.5f;
	private float			se_run_count = 0.0f;
	private float			se_repair_count = 0.5f;

	private float  			REPAIR_EFF_TIMING = 0.5f;
	private float 			count_repair_eff = 0.5f;
	private VanishEffectControl	effect_control = null;

	private bool			vo_miss_flag = true;



	// 버튼 조작 정보 표시-------.
	public Texture 		info_z_get;
	public Texture 		info_z_put;
	public Texture 		info_z_plant;
	public Texture 		info_x_eat;
	public Texture 		info_x_fire;
	public Texture 		info_x_repair;

	public Texture 		info_musha;
	public Texture 		info_repair;
	public Texture 		info_fire;


	// info계se용----------------.
	private GameObject	closest_item_prev = null;
	private GameObject	carried_item_prev = null;		//! 운반 중인 아이템.



	void Start () {
		this.step      = STEP.NONE;
		this.next_step = STEP.MOVE;
		this.item_root    = GameObject.Find("GameRoot").GetComponent<ItemRoot>();
	
		this.event_root   = GameObject.Find("GameRoot").GetComponent<EventRoot>();
		this.rocket_model = GameObject.Find("rocket").transform.FindChild("rocket_model").gameObject;

		this.game_status  = GameObject.Find("GameRoot").GetComponent<GameStatus>();		// status
		this.animation = this.transform.GetComponentInChildren<Animation>();		//motion
		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();	// sound.


		this.effect_control = GameObject.Find("GameRoot").GetComponent<VanishEffectControl>();
	}
	
	void Update () {
		this.get_input();
		this.step_timer += Time.deltaTime;

		float	eat_time		= 1.0f;
		float	repair_time		= 1.0f;	
		float 	fire_time		=1.0f;
		float	action_time		= 0.3f;

		// 변화대기.
		if(this.next_step == STEP.NONE) {
			switch(this.step) {
			case STEP.MOVE:
				do {
					if(!this.key.action) {
						break;
					}

					if(this.closest_event != null) {
						if(! this.is_event_ignitable()) {
							break;
						}
						Event.TYPE	ignitable_event = this.event_root.getEventType(this.closest_event);
						switch(ignitable_event) {
						case Event.TYPE.ROCKET:
							this.next_step = STEP.REPAIRING;
							break;
						// fire.
						case Event.TYPE.FIRE:
							this.next_step = STEP.FIRE;
							break;
						}
						break;
					}
					if(this.carried_item != null) {
						Item.TYPE	carried_item_type = this.item_root.getItemType(this.carried_item);
						switch(carried_item_type) {
						case Item.TYPE.APPLE:
						case Item.TYPE.PLANT:
							this.next_step = STEP.EATING;
							break;
						}
					}


				} while(false);
				break;

			case STEP.EATING:
				if(this.step_timer > eat_time) {
					this.next_step = STEP.MOVE;
				}
				break;

			case STEP.REPAIRING:
				if(this.step_timer > repair_time) {
					this.next_step = STEP.MOVE;
				}
				break;

			case STEP.FIRE:
				if(this.step_timer > fire_time) {
					this.next_step = STEP.MOVE;
				}
				break;
			case STEP.PICKUP:
				if(this.step_timer > action_time) {
					this.next_step = STEP.MOVE;
				}
				break;
			case STEP.THROW:
				if(this.step_timer > action_time) {
					this.next_step = STEP.MOVE;
				}
				break;
			case STEP.PLANT:
				if(this.step_timer > action_time) {
					this.next_step = STEP.MOVE;
				}
				break;
			}
		}
		

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.
		while(this.next_step != STEP.NONE) {

			Debug.Log(next_step);

			this.step      = this.next_step;
			this.next_step = STEP.NONE;
			switch(this.step) {
			case STEP.MOVE:

				break;
			case STEP.EATING:
				if(this.carried_item != null) {
					this.game_status.addSatiety(this.item_root.getRegainSatiety(this.carried_item));	// status

					GameObject.Destroy(this.carried_item);
					this.carried_item = null;
					this.animation.CrossFade("08_eat", 0.1f);		//motion
					this.sound_control.SoundPlay(Sound.SOUND.EAT);				// sound.
				}
				break;


			case STEP.REPAIRING:
				if(this.carried_item != null) {
					this.game_status.addRepairment(this.item_root.getGainRepairment(this.carried_item));	// status

					GameObject.Destroy(this.carried_item);
					this.carried_item = null;
					this.closest_event = null;
					this.animation.CrossFade("07_repair", 0.1f);		//motion.
					//this.sound_control.SoundPlay(Sound.SOUND.RESTORE);			// sound.
				}
				break;

			case STEP.FIRE:
				if(this.carried_item != null) {
					this.game_status.addFire(this.item_root.getRegainFire(this.carried_item));	// status
					
					GameObject.Destroy(this.carried_item);
					this.carried_item = null;
					this.closest_event = null;
					this.animation.CrossFade("07_repair", 0.1f);		//motion.
					this.sound_control.SoundPlay(Sound.SOUND.BURN);			// sound.
				}
				break;


			case STEP.PICKUP:
				this.animation.CrossFade("09_pickup", 0.1f);		//motion.
				break;
			case STEP.THROW:
				this.animation.CrossFade("10_put", 0.1f);		//motion.
				break;
			case STEP.PLANT:
				this.animation.CrossFade("10_put", 0.1f);		//motion.
				break;
			case STEP.DEAD:
				this.animation.CrossFade("05_died", 0.1f);		//motion.
				this.sound_control.SoundPlay(Sound.SOUND.VO_MISS);
				break;
			case STEP.CLEAR:
				this.animation.CrossFade("09_pickup", 0.1f);		//motion.
				break;
			}
			this.step_timer = 0.0f;
		}


		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.
		switch(this.step) {
		case STEP.MOVE:
			this.move_control();
			this.pick_or_drop_control();
			this.game_status.alwaysSatiety();		// 아무것도 하지 않아도 배가 고프다.

			break;
		case STEP.REPAIRING:
			// this.rocket_model.transform.localRotation *= Quaternion.AngleAxis(360.0f/10.0f*Time.deltaTime, Vector3.up);

			// se-.
			this.se_repair_count += Time.deltaTime;
			if(this.se_repair_count >SE_REPAIR_TIMING){
				this.se_repair_count = 0.0f;
				this.sound_control.SoundPlay(Sound.SOUND.RESTORE);
			}

			// effect--.
			this.count_repair_eff += Time.deltaTime;
			if(this.count_repair_eff >REPAIR_EFF_TIMING){
				this.count_repair_eff = 0.0f;
				this.effect_control.repairEffect(GameObject.Find("tonkachi").transform.position);
			}

			break;
		
		case STEP.DEAD:
			if(this.vo_miss_flag){
				if(this.step_timer >= 1.0f){
					this.sound_control.SoundPlay(Sound.SOUND.MISS_JINGLE);
					this.vo_miss_flag = false;
					Debug.Log("miss");
				}
			}
			break;
		}

		// 멋대로 빙글빙들 돌지 않도록 회전 속도를 강제로 0으로 한다.
		this.rigidbody.angularVelocity = Vector3.zero;
	}


	// 키 입력으로 이동.
	private void	move_control()
	{
		Vector3		move_vector = Vector3.zero;
		Vector3		position    = this.transform.position;

		bool		is_moved = false;
		if(this.key.right) {
			move_vector += Vector3.right;
			is_moved = true;
		}
		if(this.key.left) {
			move_vector += Vector3.left;
			is_moved = true;
		}
		if(this.key.up) {
			move_vector += Vector3.forward;
			is_moved = true;
		}
		if(this.key.down) {
			move_vector += Vector3.back;
			is_moved = true;
		}
		move_vector.Normalize();
		move_vector *= MOVE_SPEED*Time.deltaTime;
		position += move_vector;
		position.y = 0.0f;

		// 이동범위＝섬 위＝MOVE_AREA_RADIUS부터 밖으로 나가지 않게 한다.
		if(position.magnitude > MOVE_AREA_RADIUS) {
			position.Normalize();
			position *= MOVE_AREA_RADIUS;
		}
		position.y = this.transform.position.y;
		this.transform.position = position;
		// 조금씩 진행방향으로 턴한다.
		if(move_vector.magnitude > 0.01f) {
			Quaternion	q = Quaternion.LookRotation(move_vector, Vector3.up);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, q, 0.2f);
		}

		// status
		if(is_moved) {
			float	consume = this.item_root.getConsumeSatiety(this.carried_item);
			this.game_status.addSatiety(-consume*Time.deltaTime);
			this.animation.CrossFade("02_Move", 0.1f);		//motion

			// se-.
			this.se_run_count += Time.deltaTime;
			if(this.se_run_count >SE_RUN_TIMING){
				this.se_run_count = 0.0f;
				this.sound_control.SoundPlay(Sound.SOUND.FOOT);
			}

		}else{
			this.animation.CrossFade("01_Idle", 0.1f);		//motion
		}

	}

	// 키입력을 조사한다.
	private void	get_input()
	{
		this.key.up    = false;
		this.key.down  = false;
		this.key.right = false;
		this.key.left  = false;
		
		this.key.up |= Input.GetKey(KeyCode.UpArrow);
		this.key.up |= Input.GetKey(KeyCode.Keypad8);
		
		this.key.down |= Input.GetKey(KeyCode.DownArrow);
		this.key.down |= Input.GetKey(KeyCode.Keypad2);
		
		this.key.right |= Input.GetKey(KeyCode.RightArrow);
		this.key.right |= Input.GetKey(KeyCode.Keypad6);
		
		this.key.left |= Input.GetKey(KeyCode.LeftArrow);
		this.key.left |= Input.GetKey(KeyCode.Keypad4);
		
		this.key.pick   = Input.GetKeyDown(KeyCode.Z);
		this.key.action = Input.GetKeyDown(KeyCode.X);
	}


	// ----------------catch-------------------
	// 트리거에 히트한 순간만 호출되는 메소드.
	void	OnTriggerStay(Collider other)
	{
		GameObject	other_go = other.gameObject;
		if(other_go.layer == LayerMask.NameToLayer("Item")) {

			if(this.closest_item == null) {
				if(this.is_other_in_view(other_go)) {
					this.closest_item = other_go;	// 주목하고 있는 것은 그것.
				}
			} else if(this.closest_item == other_go) {
				if(!this.is_other_in_view(other_go)) {
					this.closest_item = null;		// 주목하지 않게 된다.
				}
			}
		} else if(other_go.layer == LayerMask.NameToLayer("Event")) {
			
			if(this.closest_event == null) {
				if(this.is_other_in_view(other_go)) {
					this.closest_event = other_go;
				}
			} else if(this.closest_event == other_go) {
				if(!this.is_other_in_view(other_go)) {
					this.closest_event = null;
				}
			}
		}

	}

	// 트리거로부터 떨어진 순간만 호출되는 메소드.
	void	OnTriggerExit(Collider other)
	{
		if(this.closest_item == other.gameObject) {
			this.closest_item = null;
		}
	}

	void	OnGUI()
	{
		float	x = 20.0f;
		float	y = Screen.height - 40.0f;

		Rect	rect_info_x = new Rect(220.0f,	Screen.height -40.0f, info_x_eat.width, info_x_eat.height);
		Rect	rect_info_z = new Rect(20.0f,	Screen.height -40.0f, info_z_get.width, info_z_get.height);


		if(this.carried_item != null) {
			Item.TYPE	carried_item_type = this.item_root.getItemType(this.carried_item);
			switch(carried_item_type) {
			case Item.TYPE.APPLE:
				// GUI.Label(new Rect(x, y, 200.0f, 20.0f), "Z:심는다");
				GUI.DrawTexture(rect_info_z, info_z_plant);
				GUI.DrawTexture(rect_info_x, info_x_eat);
				break;
			case Item.TYPE.IRON:
				GUI.DrawTexture(rect_info_z, info_z_put);
				break;
			case Item.TYPE.PLANT:
				// GUI.Label(new Rect(x, y, 200.0f, 20.0f), "Z:버린다");
				GUI.DrawTexture(rect_info_x, info_x_eat);
				GUI.DrawTexture(rect_info_z, info_z_put);
				break;
			}
		} else {
			if(this.closest_item != null) {
				// GUI.Label(new Rect(x, y, 200.0f, 20.0f), "Z:주으면 피로하다");
				GUI.DrawTexture(rect_info_z, info_z_get);
			}
		}


		if(this.closest_item_prev != this.closest_item){
			this.sound_control.SoundPlay(Sound.SOUND.INFO);
		}
		if(this.carried_item_prev != this.carried_item){
			this.sound_control.SoundPlay(Sound.SOUND.INFO);
		}

		this.closest_item_prev = this.closest_item;
		this.carried_item_prev = this.carried_item;




		switch(this.step) {
		case STEP.EATING:
			// GUI.Label(new Rect(x+200.0f, y, 200.0f, 20.0f), "우걱우걱우물우물……");
			GUI.DrawTexture(rect_info_x, info_musha);
			break;
		case STEP.REPAIRING:
			// GUI.Label(new Rect(x+200.0f, y, 200.0f, 20.0f), "수리중");
			GUI.DrawTexture(rect_info_x, info_repair);
			break;
		case STEP.FIRE:
			GUI.DrawTexture(rect_info_x, info_fire);
			break;
		}


		if(this.is_event_ignitable()) {
			string	message = this.event_root.getIgnitableMessage(this.closest_event);
			GUI.Label(new Rect(x+200.0f, y, 200.0f, 20.0f), "X:" + message);

			switch( this.event_root.getEventType(this.closest_event)){
			case Event.TYPE.ROCKET:
				GUI.DrawTexture(rect_info_x, info_x_repair);
				break;
			case Event.TYPE.FIRE:
				GUI.DrawTexture(rect_info_x, info_x_fire);
				break;
			}

		}
	}


	// 아이템을 버리거나 줍는다.
	private void	pick_or_drop_control()
	{
		do {
			if(!this.key.pick) {
				break;
			}
			if(this.carried_item == null) {
				// 아이템을 가지고 있지 않을 때.
				// 가까이에 아이템이 없으면 아무것도 하지 않는다.
				if(this.closest_item == null) {
					break;
				}
				this.carried_item = this.closest_item;
				this.carried_item.transform.parent = this.transform;
				this.carried_item.transform.localPosition = Vector3.up*2.0f;
				this.closest_item = null;
				this.sound_control.SoundPlay(Sound.SOUND.GET);			// sound.
				this.next_step = STEP.PICKUP;

			} else {
				// 아이템을 가지고 있을 때.
				Item.TYPE	carried_item_type = this.item_root.getItemType(this.carried_item);
				switch(carried_item_type) {
				case Item.TYPE.APPLE:
					GameObject.Destroy(this.carried_item);
					this.item_root.plantTree(this.gameObject.transform.position);		// 나무를 심는다---.
					this.sound_control.SoundPlay(Sound.SOUND.PLANT);
					this.next_step = STEP.PLANT;
					break;
				case Item.TYPE.IRON:
				case Item.TYPE.PLANT:
					this.carried_item.transform.localPosition = Vector3.forward*1.0f;
					this.carried_item.transform.parent = null;

					// 곧 바로 다시 주울 수 있도록 closest_item에 설정해 둔다.
					// (버리고 나서 한 발도 움직이지 않고도 다시 주울 수 있도록).
					this.closest_item = this.carried_item;
					this.carried_item = null;
					this.sound_control.SoundPlay(Sound.SOUND.THROW);			// sound.
					this.next_step = STEP.THROW;
					break;
				}
			}

		} while(false);
	}





	// 다른 오브젝트(아이템/이벤트 영역)를 주울 수 있는 범위(자신의 정면)에 있는가?.
	private bool	is_other_in_view(GameObject other)
	{
		bool	ret = false;
		do {
			Vector3		heading = this.transform.TransformDirection(Vector3.forward);
			Vector3		to_other = other.transform.position - this.transform.position;
			heading.y = 0.0f;
			to_other.y = 0.0f;
			heading.Normalize();
			to_other.Normalize();
			// '다른 오브젝트의 방향'과 '자신이 향한 방향'의 차이가 45도 이내?.
			float		dp = Vector3.Dot(heading, to_other);
			if(dp < Mathf.Cos(45.0f)) {
				break;
			}
			ret = true;
		} while(false);
		return(ret);
	}


	// event---------------------------
	private bool	is_event_ignitable()
	{
		bool	ret = false;
		do {
			if(this.closest_event == null) {
				break;
			}
			Item.TYPE	carried_item_type = this.item_root.getItemType(this.carried_item);
			if(!this.event_root.isEventIgnitable(carried_item_type, this.closest_event)) {
				break;
			}
			ret = true;
		} while(false);
		return(ret);
	}




	// gamestatus--------------.
	public void stateControl(PlayerControl.STEP s)
	{
		this.next_step = s;
		this.step = s;
		Debug.Log("test");
	}





}
