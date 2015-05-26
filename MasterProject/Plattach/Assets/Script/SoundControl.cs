using UnityEngine;
using System.Collections;

public class Sound{
	public enum SOUND{
		NON = -1,

		GRAB = 0,

		SLIDE,
		CLEAR,
		CLICK,
		RESTORE,
		IGNIT1,
		IGNIT2,
		IGNIT3,
		IGNIT4,
		IGNIT5,
		IGNIT6,
		IGNIT7,
		IGNIT8,
		IGNIT9,
		IGNIT10,
		GAME_CLEAR,
		NUM,
	}

	public enum BGM{
		NON = -1,
		BGM1 = 0,
		BGM2,
	}


}

public class SoundControl : MonoBehaviour {

	public AudioClip[]	audioclip;
	public AudioSource[] audiosource;

	public AudioClip[]	bgmclip;
	public AudioSource[] bgmsource;

	private	Sound.BGM	current_bgm = Sound.BGM.NON;			// 재생중 BMG.

	void Start () {
		this.audiosource = new AudioSource[this.audioclip.Length];
		for(int i=0; i<this.audiosource.Length; i++){
			this.audiosource[i]   = this.gameObject.AddComponent<AudioSource>();
			this.audiosource[i].clip = this.audioclip[i];
		}

		this.bgmsource = new AudioSource[this.bgmclip.Length];
		for(int i=0; i<this.bgmsource.Length; i++){
			this.bgmsource[i]   = this.gameObject.AddComponent<AudioSource>();
			this.bgmsource[i].clip = this.bgmclip[i];
			this.bgmsource[i].loop = true;
		}
	}
	

	// 발화음-------------.
	public void ignitSePlay(int count){
		switch(count){
		case 1: this.audiosource[ (int)Sound.SOUND.IGNIT1].Play (); break;
		case 2:	this.audiosource[ (int)Sound.SOUND.IGNIT2].Play (); break;
		case 3: this.audiosource[ (int)Sound.SOUND.IGNIT3].Play (); break;
		case 4: this.audiosource[ (int)Sound.SOUND.IGNIT4].Play (); break;
		case 5: this.audiosource[ (int)Sound.SOUND.IGNIT5].Play (); break;
		case 6: this.audiosource[ (int)Sound.SOUND.IGNIT6].Play (); break;
		case 7: this.audiosource[ (int)Sound.SOUND.IGNIT7].Play (); break;
		case 8: this.audiosource[ (int)Sound.SOUND.IGNIT8].Play (); break;
		default: this.audiosource[ (int)Sound.SOUND.IGNIT9].Play (); break;
		}
	}

	// 지정된 음향효과를 낸다.
	public void playSound(Sound.SOUND s){
		this.audiosource[(int)s].Play ();
	}

	// BGM 재생을 시작한다.
	public void playBgm(Sound.BGM b){
		this.current_bgm = b;
		this.bgmsource[ (int)this.current_bgm].Play();
	}

	// 재생중인 BGM을 멈춘다.
	public void stopBgm(){
		this.bgmsource[ (int)this.current_bgm].Stop();
	}

	// BGM의 루프 플래그를 설정한다.
	public void		setBgmLoopPlay(Sound.BGM bgm, bool is_loop_play)
	{
		this.bgmsource[(int)bgm].loop = is_loop_play;
	}

	// BGM 재생시간을 얻는다.
	public float	getBgmPlayingTime()
	{
		float	time = 0.0f;

		if(this.current_bgm != Sound.BGM.NON) {

			time = this.bgmsource[(int)this.current_bgm].time;
		}

		return(time);
	}

	// BGM 재생시간을 설정한다.
	public void		setBgmPlayingTime(float time)
	{
		if(this.current_bgm != Sound.BGM.NON) {

			this.bgmsource[(int)this.current_bgm].time = time;
		}
	}

	// BGM의 총 시간을 얻는다.
	public float	getBgmTotalTime(Sound.BGM b = Sound.BGM.NON)
	{
		if(b == Sound.BGM.NON) {

			b = this.current_bgm;
		}

		float	time = 0.0f;

		if(b != Sound.BGM.NON) {

			time = this.bgmsource[(int)b].clip.length;
		}

		return(time);
	}

	







}
