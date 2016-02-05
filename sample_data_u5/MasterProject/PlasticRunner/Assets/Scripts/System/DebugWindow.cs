using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DebugWindow : MonoBehaviour {

	protected List<string>	lines = new List<string>();

	protected static int	MAX_LINES = 10;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	
	}
	
	void	Update()
	{
	
	}

	public static int	GUI_DEPTH = -1;

	protected Rect	debug_win_rect = new Rect(20, 20, 250, 200);

	void	OnGUI()
	{
		//debug_win_rect = GUI.Window(100, debug_win_rect,  debug_window_function, "console");
	}

	void	debug_window_function(int id)
	{
		GUI.depth = GUI_DEPTH;

		float	x, y;

		x = 10.0f;
		y = 20.0f;

		foreach(var line in this.lines) {

			GUI.Label(new Rect(x, y, line.Length*20, 20), line);
			y += 20.0f;
		}

		GUI.DragWindow(new Rect(0, 0, 100000, 20));
	}

	// ================================================================ //

	public void	create()
	{
		this.lines = new List<string>();
	}

	// 텍스트를 추가한다.
	public void	add_text(string text)
	{
		if(this.lines.Count >= MAX_LINES) {

			this.lines.RemoveRange(0, 1);
		}

		this.lines.Add(text);
	}

	// ================================================================ //

	protected static DebugWindow	instance = null;

	public static DebugWindow	get()
	{
		if(DebugWindow.instance == null) {

			GameObject	go = new GameObject("DebugWindow");

			DebugWindow.instance = go.AddComponent<DebugWindow>();
			DebugWindow.instance.create();

			DontDestroyOnLoad(go);
		}

		return(DebugWindow.instance);
	}

}
