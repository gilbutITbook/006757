using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 입모양 데이터.
public class KuchiPakuData {
	
	public KuchiPakuData()
	{
		this.line_number = -1;
		this.time        = 0.0f;
		this.mouth_type  = CloudControl.MOUTH_TYPE.CLOSE;
	}

	public int						line_number;	// 텍스트 파일의 행 번호.
	public float					time;			// [sec] 시간.
	public CloudControl.MOUTH_TYPE	mouth_type;		// 입의 상태.
};

public class CloudRoot : MonoBehaviour {

	private List<KuchiPakuData>		kuchi_paku_datas = null;		// 입모양 데이터.
	public TextAsset				kuchi_paku_text  = null;		// 입모양 데이터의 텍스트.

	private SoundControl	sound_control = null;
	private CameraControl	main_camera   = null;
	private PlayerControl	player        = null;

	public GameObject			kumo01Prefab = null;
	public GameObject			kumo02Prefab = null;

	private class Cloud {

		public GameObject	cloud;				// 구름 게임 오브젝트.
		public float		speed;				// 속도 비율.
	};
	private List<Cloud>		clouds;

	private static float	DEPTH      = 15.0f;						// 깊이.
	private static float	CLIP_RANGE = 16.0f;						// 화면 좌우의 크기.
	private static float	Y_MAX =  6.0f;							// 최대 높이.
	private static float	Y_MIN = -6.0f;							// 최소 높이.

	// ---------------------------------------------------------------- //

	public bool		is_kuchi_paku_scene = false;			// true ... 입모양 데이터 작성용 씬 false ... 일반 게임.
	public bool		is_recording_mode   = false;			// true ... 데이터 작성 모드        false ... 데이터 재생 모드.

	private CloudControl.MOUTH_TYPE	mouth_type = CloudControl.MOUTH_TYPE.NONE;

	private string kuchi_paku_record = "";

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
		this.kuchi_paku_datas = new List<KuchiPakuData>();

		this.loadKuchiPakuData(this.kuchi_paku_text);

		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();
		this.main_camera   = GameObject.Find("Main Camera").GetComponent<CameraControl>();
		this.player        = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

		this.clouds = new List<Cloud>();

		for(int i = 0;i < 4;i++) {

			GameObject		go;

			if(i%2 == 0) {

				go = GameObject.Instantiate(this.kumo01Prefab) as GameObject;

			} else {

				go = GameObject.Instantiate(this.kumo02Prefab) as GameObject;
			}

			Cloud			cloud = new Cloud();

			this.clouds.Add(cloud);

			cloud.cloud = go;

			// 다중 스크롤처럼  보이게 안쪽으로 갈수록 속도를 느리게 한다.
			cloud.speed = 0.5f*Mathf.Pow(0.7f, i);

			cloud.cloud.transform.parent        = this.main_camera.transform;
			cloud.cloud.transform.localPosition = new Vector3(CLIP_RANGE*(i*1.0f + 0.0f), 1.0f + i*2.0f, DEPTH + i*1.0f);

			float		s = Mathf.Lerp(1.0f, 0.5f, i/(3.0f - 1.0f));

			Vector3		scale = cloud.cloud.transform.localScale;

			scale.x *= s;
			scale.y *= s;

			cloud.cloud.transform.localScale = scale;
		}
	}

	void	Update()
	{
		// 입모양 데이터 작성 모드.
		if(this.is_kuchi_paku_scene) {

			if(this.is_recording_mode) {
	
				this.update_recording_mode();

			} else {

			}

		} else {

			foreach(Cloud cloud in this.clouds) {

				// 화면 왼쪽 끝으로 사라지면 전방(오른쪽 끝)으로 순간이동.
				if(cloud.cloud.transform.localPosition.x < -CLIP_RANGE) {

					Vector3		p = cloud.cloud.transform.localPosition;

					p.x = CLIP_RANGE;
					p.y = Mathf.InverseLerp(Y_MIN, Y_MAX, p.y);
					p.y += Random.Range(0.25f, 0.75f);
					p.y = Mathf.Repeat(p.y, 1.0f);
					p.y = Mathf.Lerp(Y_MIN, Y_MAX, p.y);

					cloud.cloud.transform.localPosition = p;
				}
				cloud.cloud.transform.Translate(-Vector3.right*cloud.speed*this.player.rigidbody.velocity.x*Time.deltaTime);
			}
		}
	}
	
	void	OnGUI()
	{
		if(this.is_kuchi_paku_scene) {

			this.on_gui_recording();
		}
	}

	// ================================================================ //

	// 입모양 형태를 얻는다.
	public CloudControl.MOUTH_TYPE	getMouthType()
	{
		CloudControl.MOUTH_TYPE		mouth_type;

		if(this.is_recording_mode) {

			mouth_type = this.get_mouth_type_record();

		} else {

			mouth_type = this.get_mouth_type_play();
		}

		return(mouth_type);
	}


	// 입모양 형태를 얻는다.  게임 중.
	private CloudControl.MOUTH_TYPE	get_mouth_type_play()
	{
		// BGM의 재생 시각을 가져온다.
		float	time = this.sound_control.getBgmPlayingTime();

		CloudControl.MOUTH_TYPE	mouth_type = CloudControl.MOUTH_TYPE.CLOSE;

		do {

			int		index = this.get_kuchi_paku_data_index(time);

			if(index < 0) {

				break;
			}

			mouth_type = this.kuchi_paku_datas[index].mouth_type;

		} while(false);

		return(mouth_type);
	}

	// 현재 시각의 입모양 데이터의 인덱스를 얻는다.
	private int		get_kuchi_paku_data_index(float time)
	{
		int		index = -1;

		do {

			if(this.kuchi_paku_datas.Count == 0) {

				break;
			}

			index = this.kuchi_paku_datas.Count - 1;

			for(int i = 0;i < this.kuchi_paku_datas.Count;i++) {
	
				if(time < this.kuchi_paku_datas[i].time) {

					index = i - 1;
					break;
				}
			}

		} while(false);

		return(index);
	}

	// 입 모양 형태를 얻는다.　입 모양 데이터 작성 모드용.
	private CloudControl.MOUTH_TYPE	get_mouth_type_record()
	{
		return(this.mouth_type);
	}

	// ================================================================ //

	// 매 프레임 실행 입모양 데이터 작성 모드용.
	private void	update_recording_mode()
	{
		CloudControl.MOUTH_TYPE	mouth_type = CloudControl.MOUTH_TYPE.CLOSE;

		if(Input.GetMouseButton(0)) {

			mouth_type = CloudControl.MOUTH_TYPE.HALF;
		}
		if(Input.GetMouseButton(1)) {

			mouth_type = CloudControl.MOUTH_TYPE.FULL;
		}

		if(mouth_type != this.mouth_type) {

			this.mouth_type = mouth_type;

			KuchiPakuData	data = new KuchiPakuData();

			data.time       = this.sound_control.getBgmPlayingTime();
			data.mouth_type = this.mouth_type;

			this.kuchi_paku_datas.Add(data);

			this.kuchi_paku_record += data.time.ToString();
			this.kuchi_paku_record += "\t";
			this.kuchi_paku_record += data.mouth_type.ToString().ToLower();
			this.kuchi_paku_record += "\n";
		}
	}

	// 입모양 데이터 작성 씬용 GUI
	private	void	on_gui_recording()
	{
		if(this.is_recording_mode) {

			GUI.TextField(new Rect(10, 10, 400, 400), this.kuchi_paku_record);

		} else {


			// BGM 재생 시각을 얻는다.
			float	time = this.sound_control.getBgmPlayingTime();
	
			do {
	
				int		index = this.get_kuchi_paku_data_index(time);
	
				if(index < 0) {
	
					break;
				}
	
				string			line_text = "";

				int		st, ed;
				int		line_count = 12;

				if(this.kuchi_paku_datas.Count < line_count) {

					st = 0;
					ed = line_count - 1;

				} else {

					if(index < line_count - 1) {

						st = 0;
						ed = line_count - 1;

					} else {

						ed = index + 1;
	
						if(ed >= this.kuchi_paku_datas.Count) {
	
							ed = this.kuchi_paku_datas.Count - 1;
						}
	
						st = ed - (line_count - 1);
					}
				} 

				for(int i = st;i <= ed;i++) {

					KuchiPakuData	data = this.kuchi_paku_datas[i];

					if(i == index) {

						line_text += ">\t";

					} else {

						line_text += "\t";
					}
					line_text += 	  (data.line_number + 1).ToString("d3") + "\t\t" 
									+ data.time.ToString("0.00") + "\t\t" 
									+ data.mouth_type.ToString().ToLower() + "\n";
				}

				GUI.TextField(new Rect(10, 10, 400, 200), line_text);

			} while(false);
		}
	}

	// 레벨 데이터를 텍스트 파일에서 읽어온다.
	public void		loadKuchiPakuData(TextAsset kuchi_paku_text)
	{
		// 텍스트 전체를 한 문자열로.
		string		all_texts = kuchi_paku_text.text;

		// 개행 코드로 구분함으로써,.
		// 텍스트 전체를 한 줄단위 배열로 만든다. .
		string[]	lines = all_texts.Split('\n');

		int			line_number = -1;

		foreach(var line in lines) {

			line_number++;

			if(line == "") {

				continue;
			}

			// 공백으로 구분하여 단어의 배열로 만든다.
			string[]	words = line.Split();

			int				n = 0;
			KuchiPakuData	kuchi_paku_data = new KuchiPakuData();

			kuchi_paku_data.line_number = line_number;

			foreach(var word in words) {

				// "#" 이후는 주석이므로 그 이후는 건너뛴다.
				if(word.StartsWith("#")) {

					break;
				}
				if(word == "") {

					continue;
				}

				switch(n) {

					case 0:		kuchi_paku_data.time       = float.Parse(word);			break;
					case 1:		kuchi_paku_data.mouth_type = this.toMouthType(word);	break;
				}

				n++;
			}

			if(n >= 2) {

				this.kuchi_paku_datas.Add(kuchi_paku_data);

			} else {

				if(n == 0) {

					// 단어가 없다 = 행 전체가 주석이었다.

				} else {

					// 파라미터가 부족하다.
					Debug.LogError("[KuchiPakuData] Out of parameter.\n");
				}
			}
		}

		if(this.kuchi_paku_datas.Count == 0) {

			// 데이터가 하나도 없을 때.

			Debug.LogError("[KuchiPakuData] Has no data.\n");

			// 기본 데이터를 하나 추가해 둔다.
			this.kuchi_paku_datas.Add(new KuchiPakuData());
		}
	}

	// 문자열 →　입모양.
	public CloudControl.MOUTH_TYPE	toMouthType(string str)
	{
		CloudControl.MOUTH_TYPE mouth_type = CloudControl.MOUTH_TYPE.CLOSE;

		switch(str.ToLower()) {

			case "half":	mouth_type = CloudControl.MOUTH_TYPE.HALF;	break;
			case "full":	mouth_type = CloudControl.MOUTH_TYPE.FULL;	break;
		}

		return(mouth_type);
	}
}
