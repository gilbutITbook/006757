using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudControl : MonoBehaviour {

	private CameraControl	main_camera   = null;

	public enum MOUTH_TYPE {

		NONE = -1,

		CLOSE = 0,				// 닫혀있다.
		HALF,					// 열다.
		FULL,					// 활짝 열다.

		NUM,
	};
	public enum EYE_TYPE {

		NONE = -1,

		OPEN = 0,		// 열다.
		CLOSE,			// 닫혀있다.

		NUM,
	};

	protected MOUTH_TYPE	mouth_type = MOUTH_TYPE.NONE;
	protected EYE_TYPE		eye_type   = EYE_TYPE.NONE;

	protected List<GameObject>	mouths = null;
	protected List<GameObject>	eyes = null;

	protected CloudRoot		cloud_root = null;
	protected GameObject	face = null;
	protected float			face_center = 0.0f;

	protected float			timer;
	protected float			blink_timer = 0.0f;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.main_camera = GameObject.Find("Main Camera").GetComponent<CameraControl>();

		// ------------------------------------------------------------ //
		// 입과 눈.

		this.mouths = new List<GameObject>();
		this.eyes   = new List<GameObject>();

		this.face = this.transform.FindChild("face").gameObject;
		this.face_center = this.face.transform.localPosition.z;

		// 입.

		Transform	mouth_root = this.face.transform.FindChild("mouth");

		for(int i = 0;i < (int)MOUTH_TYPE.NUM;i++) {

			this.mouths.Add(null);

			Transform	t = mouth_root.FindChild("mouth" + i);

			if(t == null) {

				continue;
			}

			this.mouths[i] = t.gameObject;
		}

		this.change_mouth_type(this.mouth_type);

		// 눈.

		for(int i = 0;i < (int)EYE_TYPE.NUM;i++) {

			this.eyes.Add(null);

			Transform	t = this.face.transform.FindChild("eye" + i);

			if(t == null) {

				continue;
			}

			this.eyes[i] = t.gameObject;
		}

		this.eye_type    = EYE_TYPE.OPEN;
		this.blink_timer = 0.0f;
		this.change_eye_type();

		// ------------------------------------------------------------ //

		this.cloud_root = GameObject.Find("Game Root").GetComponent<CloudRoot>();

		this.timer = 0.0f;
	}

	void	Update()
	{
		// ------------------------------------------------------------ //
		// 원근법이 심해 얼굴 부분이 움직이는 것처럼 보이므로.

		if(this.main_camera != null) {

			Vector3		camera_direction = this.transform.InverseTransformPoint(this.main_camera.transform.position);
	
			camera_direction.Normalize();
			camera_direction.x *= this.face_center/camera_direction.z;
			camera_direction.y *= this.face_center/camera_direction.z;
			camera_direction.z  = this.face_center;
	
			this.face.transform.localPosition = camera_direction;
		}

		// ------------------------------------------------------------ //

		MOUTH_TYPE	mouth_type = this.cloud_root.getMouthType();

		this.change_mouth_type(mouth_type);

		this.change_eye_type();

		// ------------------------------------------------------------ //

		this.timer += Time.deltaTime;
	}

	protected void		change_mouth_type(MOUTH_TYPE mouth_type)
	{
		if(this.mouth_type != mouth_type) {

			this.mouth_type = mouth_type;
	
			for(int i = 0;i < this.mouths.Count;i++) {
	
				GameObject	mouth = this.mouths[i];
	
				mouth.SetActive((i == (int)this.mouth_type));
			}
		}
	}

	protected void		change_eye_type()
	{
		this.blink_timer -= Time.deltaTime;

		if(this.blink_timer < 0.0f) {

			this.blink_timer = Random.Range(1.0f, 3.0f);
			this.eye_type    = EYE_TYPE.OPEN;

		} else if(this.blink_timer < 0.25f) {

			this.eye_type = EYE_TYPE.CLOSE;

		}

		for(int i = 0;i < this.eyes.Count;i++) {
	
			GameObject	eye = this.eyes[i];
	
			eye.SetActive((i == (int)this.eye_type));
		}
	}
}
