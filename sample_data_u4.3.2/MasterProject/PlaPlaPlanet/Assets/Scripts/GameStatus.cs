using UnityEngine;
using System.Collections;

public class GameStatus : MonoBehaviour {

	// 수리를 했을 때 상승하는 수리도.
	public static float		GAIN_REPARIMENT_IRON = 0.15f;		//!< 철.
	public static float		GAIN_REPARIMENT_PLANT = 0.05f;		//!< 오이.
	
	// 운반할 때 소모 되는 체력 양(1초당).
	public static float		CONSUME_SATIETY_IRON  = 0.15f/2.0f;		//!< 철.
	public static float		CONSUME_SATIETY_APPLE = 0.1f/2.0f;		//!< 사과.
	public static float		CONSUME_SATIETY_PLANT = 0.1f/2.0f;		//!< 식물.
	public static float		CONSUME_SATIETY_ALWAYS	= 0.03f;	//!< 살아있을 뿐.
	
	// 사과를 먹었을 때 회복하는 체력.
	public static float		REGAIN_SATIETY_APPLE = 0.6f;
	public static float		REGAIN_SATIETY_PLANT = 0.2f;


	// 장작으로 썼을 때 회복되는 불의 양.
	public static float		REGAIN_FIRE_APPLE	= 0.3f;
	public static float		REGAIN_FIRE_PLANT	= 0.7f;

	// 자동으로 감소하는 모닥불.
	public static float		FIRE_ALWAYS			= 0.05f/2.0f;

	// ゲーーージ.
	public Texture	texture_gauge_sita = null;			// 아래.
	public Texture	texture_gauge_ue   = null;			// 위.
	public Texture	texture_gauge_waku = null;			// 테두리.

	// ================================================================ //
	public float	repairment = 0.0f;		// 수리도(0.0f ～ 1.0f).
	public float	satiety    = 1.0f;		// 포만도(체력).
	public float	fire		= 1.0f;		// 모닥불.

	// 효과음용--.
	public float	past_repairment = 0.0f;		// 수리도(0.0f ～ 1.0f).
	public float	past_satiety    = 1.0f;		// 포만도.
	public float	past_fire		= 1.0f;		// 모닥불.



	public float	repairment_bak = 0.0f;

	public GUIStyle guistyle;

	public Texture 	icon_repariment	= null;
	public Texture 	icon_fire		 = null;
	public Texture 	icon_satiety	 = null;


	//반창고----.
	public GameObject[]		bansoco;

	// 로켓 모션---.
	private Animation 	animation;		// (rocket)motion--.

	// sound-----.
	private SoundControl	sound_control;

	// 모닥불----.
	private GameObject		fire_object;



	void	Start()
	{
		this.guistyle.fontSize = 32;
		this.animation = GameObject.Find("rocket").transform.FindChild("rocket_model").gameObject.GetComponentInChildren<Animation>();		//motion

		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();

		this.fire_object = GameObject.Find("Fire").gameObject;
	}
	
	void	Update()
	{
		this.alwaysFire();

		// 모닥불이 작아져간다---.
		float fire_size = 0.7f *this.fire;
		fire_size = Mathf.Clamp(fire_size, 0.3f, 0.7f);
		this.fire_object.transform.localScale = Vector3.one *fire_size;

		// 효과음을 울린다--.
		// 모닥불이 0.3f이하가 되면----.
		if((this.past_fire >0.3f)&&(this.fire <=0.3f)){
			this.sound_control.SoundPlay(Sound.SOUND.FIRE_M);
		}
		if((this.past_repairment < 0.7f)&&(this.repairment >=0.7f)){
			this.sound_control.SoundPlay(Sound.SOUND.SHIP_M);
		}
		if((this.past_satiety >0.3f)&&(this.satiety <=0.3f)){
			this.sound_control.SoundPlay(Sound.SOUND.STOMACH);
		}

		this.past_fire		= this.fire;
		this.past_repairment= this.repairment;
		this.past_satiety	= this.satiety;

		// 디버그 조작(C키로 바로 게임 클리어).
		if(Input.GetKeyDown(KeyCode.C)) {
			this.repairment = 1.0f;
		}
	}

	void	OnGUI()
	{
		// icon.
		GUI.DrawTexture(new Rect(80, 8, icon_satiety.width, icon_satiety.height), icon_satiety);
		GUI.DrawTexture(new Rect(260, 8, icon_fire.width, icon_fire.height), icon_fire);
		GUI.DrawTexture(new Rect(440, 8, icon_repariment.width, icon_repariment.height), icon_repariment);
		
		//float	y = 20.0f;
		//GUI.Label(new Rect(80+48, y, 200.0f, 20.0f), (this.satiety*100.0f).ToString("000"), guistyle);
		//GUI.Label(new Rect(280+48, y, 200.0f, 20.0f), (this.fire*100.0f).ToString("000"), guistyle);
		//GUI.Label(new Rect(440+48, y, 200.0f, 20.0f), (this.repairment*100.0f).ToString("000"), guistyle);

		// 게이지.
		this.draw_gauge(120.0f, 24.0f, this.satiety, true);
		this.draw_gauge(300.0f, 24.0f, this.fire, true);
		this.draw_gauge(480.0f, 24.0f, 1.0f -this.repairment, true);
	}


	// 게이지 표시.
	protected void	draw_gauge(float x, float y, float length, bool shake)
	{
		Texture	sita = this.texture_gauge_sita;
		Texture	ue   = this.texture_gauge_ue;
		Texture	waku = this.texture_gauge_waku;

		if(shake && length <= 0.3f){
			x += Random.Range(-2,2);
			y += Random.Range(-2,2);
		}

		GUI.DrawTexture(new Rect(x, y, sita.width,      sita.height), sita);
		GUI.DrawTexture(new Rect(x, y, ue.width*length, ue.height),   ue);
		GUI.DrawTexture(new Rect(x, y, waku.width,      waku.height), waku);
	}


	// ================================================================ //
	
	// 수리도를 증감한다.
	public void		addRepairment(float add)
	{
		this.repairment_bak = this.repairment;
		this.repairment = Mathf.Clamp01(this.repairment + add);

		// 반창고가 떨어진다.
		if (this.repairment >= 0.2f && this.repairment_bak < 0.2f) {
			this.bansoco[0].GetComponent<bansocoControl>().getoff();
		}
		if (this.repairment >= 0.4f && this.repairment_bak < 0.4f) {
			this.bansoco[1].GetComponent<bansocoControl>().getoff();
		}
		if (this.repairment >= 0.6f && this.repairment_bak < 0.6f) {
			this.bansoco[2].GetComponent<bansocoControl>().getoff();
		}
		if (this.repairment >= 0.8f && this.repairment_bak < 0.8f) {
			this.bansoco[3].GetComponent<bansocoControl>().getoff();
		}
		if (this.repairment >= 1.0f && this.repairment_bak < 1.0f) {
			this.bansoco[4].GetComponent<bansocoControl>().getoff();
		}

		this.animation.Play("01_repair");

	}
	
	// 포만도를 증감한다.
	public void		addSatiety(float add)
	{
		this.satiety = Mathf.Clamp01(this.satiety + add);
	}
	
	// 이동만 해도 배가 고파진다.
	public void	alwaysSatiety()
	{
		this.satiety = Mathf.Clamp01(this.satiety - CONSUME_SATIETY_ALWAYS *Time.deltaTime);
	}

	// 모닥불을 증감한다.
	public void addFire(float add)
	{
		this.fire = Mathf.Clamp01(this.fire +add);
	}

	// 모닥불이 줄어 간다.
	public void	alwaysFire()
	{
		this.fire = Mathf.Clamp01(this.fire - FIRE_ALWAYS *Time.deltaTime);
	}
	

	// 클리어 했는가？-------------.
	public bool		isGameClear()
	{
		bool	is_clear = false;
		if(this.repairment >= 1.0f) {
			is_clear = true;
		}
		return(is_clear);
	}
	
	
	// 죽었는가？-----.
	public bool		isGameOver()
	{
		bool	is_over = false;
		if(this.satiety <= 0.0f){
			is_over = true;
		}
		if(this.fire <=0.0f){
			is_over = true;
		}
		return(is_over);
	}









}
