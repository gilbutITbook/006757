using UnityEngine;
using System.Collections;

public class unitoControl : MonoBehaviour {

	public enum STEP{
		NONE = -1,
		IDLE = 0,
		MOVE,
		JUMPUP,
		JUMPDOWN,
		DIED,
		REPAIR,
		EAT,
		PICKUP,
		PUT,
		WINCE,
	};
	private STEP step;
	private STEP next_step;


	private Animation unito_motion;		// motion.

	public float step_timer =0.0f;


	public float REPAIR_TIME = 2.8f;

	private SoundControl sound_control = null;
	private float	count_repair_se = 0.2f;
	private float	REPAIR_SE_TIMING = 0.5f;

	private float 	count_repair_eff = 0.5f;
	private float   REPAIR_EFF_TIMING = 0.5f;

	public VanishEffectControl	effect_control = null;		// 발화 시 효과를 만드는 클래스.

	public GameObject hana_prefab = null;

	public Transform	unitoAtama = null;

	void Start () {
		this.unito_motion = this.transform.GetComponentInChildren<Animation>();		//motion.
		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();

		this.effect_control = GameObject.Find("GameRoot").gameObject.GetComponent<VanishEffectControl>();
		this.count_repair_eff = REPAIR_EFF_TIMING;
		this.count_repair_se = REPAIR_SE_TIMING;
		next_step = STEP.IDLE;



		// --------------------------------------------.
		// 가끔 머리에 꽃을!.
		int rnd = Random.Range(0, 9);
		if(rnd > 5){
			GameObject go = GameObject.Instantiate(this.hana_prefab) as GameObject;
			go.transform.parent = unitoAtama;
			go.transform.position = unitoAtama.position;
			go.transform.localScale = Vector3.one;
		}


		// --------------------------------------------.
		// 2014.06.16.
		// 서서히 완성판 구현도 막바지에 이르렀다.
		// 원고 수정도 끝나고 표지도 정해졌다.
		// 발매일이 가까워지면서 조금은 두근두근 거리는 시간을 보내는 중.
		// 이 책을 사주셔서 정말 기쁘고 뭔가 하나라도 얻는 게 있으면 좋겠다.
		// --------------------------------------------.

	}
	
	void Update () {
		step_timer += Time.deltaTime;


		while(this.next_step != STEP.NONE) {
			//Debug.Log(this.next_step);
			this.step      = this.next_step;
			this.next_step = STEP.NONE;
			this.count_repair_eff = REPAIR_EFF_TIMING;
			this.count_repair_se = REPAIR_SE_TIMING;
			switch(step){
			case STEP.IDLE:		this.unito_motion.CrossFade("01_Idle", 0.1f);		break;
			case STEP.MOVE:		this.unito_motion.CrossFade("02_Move", 0.1f);		break;
			case STEP.JUMPUP:	this.unito_motion.CrossFade("03_jumpup", 0.1f);		break;
			case STEP.JUMPDOWN:	this.unito_motion.CrossFade("04_jumpdown", 0.1f);	break;
			case STEP.DIED:		this.unito_motion.CrossFade("05_died", 0.1f);		break;
			case STEP.REPAIR:	this.unito_motion.CrossFade("07_repair", 0.1f);		break;
			case STEP.EAT:		this.unito_motion.CrossFade("08_eat", 0.1f);		break;
			case STEP.PICKUP:	this.unito_motion.CrossFade("09_pickup", 0.1f);		break;
			case STEP.PUT:		this.unito_motion.CrossFade("10_put", 0.1f);		break;
			case STEP.WINCE:	this.unito_motion.CrossFade("11_wince", 0.1f);		break;
			}
			step_timer = 0.0f;
		}


		switch(this.step){
		case STEP.REPAIR:
			if(step_timer >this.REPAIR_TIME){
				next_step = STEP.IDLE;
			}

			count_repair_se -= Time.deltaTime;
			if(count_repair_se <0.0f){
				count_repair_se = REPAIR_SE_TIMING;
				this.sound_control.playSound(Sound.SOUND.RESTORE);
			}


			count_repair_eff -= Time.deltaTime;
			if(count_repair_eff <0.0f){
				count_repair_eff = REPAIR_EFF_TIMING;
				Vector3 eff_pos = GameObject.Find("tonkachi").transform.position;
				this.effect_control.repairEffect(eff_pos);
			}

			break;
		}

	}

	public void actionUnito(STEP s){
		next_step = s;
	}

	public void clear(){
		this.transform.Rotate(new Vector3(0, 90, 0));
		this.actionUnito(unitoControl.STEP.PICKUP);
	}


}
