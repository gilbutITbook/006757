using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 디버그 문자를 화면에 표시하기 위한 클래스.
// OnGUI() 이외에도 디버그 프린트를 사용할 수 있다.
public class DebugPrint : MonoBehaviour {

	private static DebugPrint	instance = null;

	public struct TextItem {

		public int		x, y;
		public string	text;
		public float	lifetime;
	};

	private List<TextItem>	items;
	private int				locate_x, locate_y;

	private static int		CHARA_W = 20;
	private static int		CHARA_H = 20;


	// ------------------------------------------------------------------------ //

	public static DebugPrint	getInstance()
	{
		if(DebugPrint.instance == null) {

			GameObject	go = new GameObject("DebugPrint");

			DebugPrint.instance = go.AddComponent<DebugPrint>();
			DebugPrint.instance.create();

			DontDestroyOnLoad(go);
		}

		return(DebugPrint.instance);
	}

	// 텍스트를 표시한다.
	public static void	print(string text, float lifetime = 0.0f)
	{
		DebugPrint	dp = DebugPrint.getInstance();

		dp.add_text(text, lifetime);
	}

	// 표시 위치를 설정한다.
	public static void	setLocate(int x, int y)
	{
		DebugPrint	dp = DebugPrint.getInstance();

		dp.set_locate(x, y);
	}

	// ------------------------------------------------------------------------ //

	void Start ()
	{
		this.clear();
	}
	
	void Update ()
	{

	}

	void OnGUI()
	{
		GUI.depth = -1;

		// 버퍼에 들어 있는 텍스트를 표시한다.

		int		x, y;

		foreach(var item in this.items) {

			x = item.x*DebugPrint.CHARA_W;
			y = item.y*DebugPrint.CHARA_H;

			GUI.Label(new Rect(x, y, item.text.Length*DebugPrint.CHARA_W, DebugPrint.CHARA_H), item.text);

			y += DebugPrint.CHARA_H;
		}

		// 버퍼를 비운다.

		if(UnityEngine.Event.current.type == UnityEngine.EventType.Repaint) {

			this.clear();
		}
	}

	public void	create()
	{
		this.items = new List<TextItem>();
	}

	// 버퍼를 비운다.
	private void	clear()
	{
		this.locate_x = 10;
		this.locate_y = 10;

		for(int i = 0;i < this.items.Count;i++) {

			TextItem	item = this.items[i];

			if(item.lifetime >= 0.0) {

				item.lifetime -= Time.deltaTime;
	
				this.items[i] = item;
	
				if(item.lifetime <= 0.0f) {
	
					this.items.Remove(this.items[i]);
				}
			}
		}
	}

	// 표시 위치를 설정한다.
	private void	set_locate(int x, int y)
	{
		this.locate_x = x;
		this.locate_y = y;
	}

	// 텍스트를 추가한다.
	private void	add_text(string text, float lifetime = 0.0f)
	{
		TextItem	item;

		item.x        = this.locate_x;
		item.y        = this.locate_y;
		item.text     = text;
		item.lifetime = lifetime;

		this.items.Add(item);

		this.locate_y++;
	}
}
