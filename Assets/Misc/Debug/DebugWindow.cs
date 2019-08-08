using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class dbwin {

	// ================================================================ //

	public class Item {

		public Item(string label)
		{
			this.label = label;
			this.size  = new Vector2(this.label.Length*14 + 20, 20);
		}

		public virtual void	onGUI(float x, float y)
		{
		}
		public virtual void	execute()
		{
		}
		
		public Vector2	size  = new Vector2(100, 20);
		public string	label = "";

		public object	user_data;
	};

	// ウインドウのボタン.
	public class Button : Item {

		public delegate	void	Func();

		public Button(string label) : base(label)
		{
			this.on_press = () => {};
		}
	
		public Button	setOnPress(Func func)
		{
			this.on_press = func;

			return(this);
		}

		public Button	setUserData(object user_data)
		{
			this.user_data = user_data;
			
			return(this);
		}

		public override void	onGUI(float x, float y)
		{
			if(GUI.Button(new Rect(x, y, this.size.x, this.size.y), this.label)) {

				this.on_press();
			}
		}

		// ---------------------------------------------------------------- //

		public Func		on_press;			// 押された瞬間に呼ばれるメソッド.
	};

	// ウインドウのチェックボックス.
	public class CheckBox : Item {
	
		public delegate	void	Func(bool is_checked);

		public CheckBox(string label, bool initial_value) : base(label)
		{
			this.is_checked = initial_value;
			this.on_changed = (bool is_checked) => {};
		}

		// 「チェック状態が変わった瞬間に呼ばれるメソッド」.
		public CheckBox	setOnChanged(Func func)
		{
			this.on_changed = func;

			return(this);
		}

		public CheckBox	setUserData(object user_data)
		{
			this.user_data = user_data;
			
			return(this);
		}
		
		public override void	onGUI(float x, float y)
		{
			bool	checked_next = GUI.Toggle(new Rect(x, y, this.label.Length*14, 20), this.is_checked, this.label);

			if(checked_next != this.is_checked) {

				this.is_checked = checked_next;
				this.on_changed(this.is_checked);
			}
		}

		// ---------------------------------------------------------------- //

		public Func		on_changed;			// チェック状態が変わった瞬間に呼ばれるメソッド.
		public bool		is_checked;
	};

	// ウインドウのスライダー.
	public class Slider : Item {
		
		public delegate	void	IdleFunc(Slider slider);
		public delegate	void	Func(Slider slider, float new_value);
		
		public Slider(string label, float initial_value, float min, float max) : base(label)
		{
			this.value = initial_value;
			this.min   = min;
			this.max   = max;

			this.on_changing = (Slider slider, float new_value) => {};
			this.on_idle     = (Slider slider) => {};

			this.size.y = 30.0f;
		}

		public Slider	setOnIdle(IdleFunc func)
		{
			this.on_idle = func;
			
			return(this);
		}

		// 「値が変わっている間呼ばれるメソッド」.
		public Slider	setOnChanging(Func func)
		{
			this.on_changing = func;
			
			return(this);
		}
		
		public Slider	setUserData(object user_data)
		{
			this.user_data = user_data;
			
			return(this);
		}

		public override void	execute()
		{
			this.on_idle(this);
		}
		public override void	onGUI(float x, float y)
		{
			float	effective_value = Mathf.Lerp(this.min, this.max, this.value);

			string	label_string = this.label + " " + effective_value.ToString("f2");

			GUI.Label(new Rect(x, y, label_string.Length*14, 20.0f), label_string);

			float	next_value = GUI.HorizontalSlider(new Rect(x, y + 20.0f, 100.0f, 20.0f), this.value, this.min, this.max);

			if(next_value != this.value) {

				this.value = next_value;
				this.on_changing(this, this.value);
			}
		}
		
		// ---------------------------------------------------------------- //
		
		public Func			on_changing;		// 値が変わっている間呼ばれるメソッド.
		public IdleFunc		on_idle;

		public float	min;
		public float	max;
		public float	value;
	};

	// ウインドウのセレクトボタン（排他的な、複数のボタン）.
	public class Selector : Item {

		public delegate	void	Func(int selection);
		
		public Selector(string label, string[] texts, int value) : base(label)
		{
			this.texts = texts;
			this.value = value;
			
			this.on_changed = (int new_value) => {};
			
			this.size.y = 20.0f;

			this.whole_width = 0;

			foreach(var text in this.texts) {

				this.whole_width += text.Length*14;
			}
		}

		// 「値が変わっている間呼ばれるメソッド」.
		public Selector	setOnChanged(Func func)
		{
			this.on_changed = func;
			
			return(this);
		}

		public override void	onGUI(float x, float y)
		{
			int		new_value = GUI.SelectionGrid(new Rect(x, y, this.whole_width, 20.0f), this.value, this.texts, this.texts.Length);
					
			if(new_value != this.value) {
				
				this.value = new_value;
				this.on_changed(this.value);
			}
		}
		
		// ---------------------------------------------------------------- //
		

		public Func		on_changed;		// 値が変わったときに呼ばれるメソッド.
		public string[]	texts;
		public int		value;
		public int		whole_width;
	};

	// ================================================================ //
	// ウインドウのベースクラス.

	public class WindowBase {

		public WindowBase(int id, string title)
		{
			this.title    = title;
			this.win_rect = new Rect(20, 20, 200, 300);
			this.id       = id;
	
			this.is_active = false;
		}

		public virtual void	execute() {}
		public virtual void	onGUI() {}

		public void	setPosition(float x, float y)
		{
			this.win_rect.x = x;
			this.win_rect.y = y;
		}

		// ---------------------------------------------------------------- //

		public string	title;
		public Rect		win_rect;
		public int		id;
		public bool		is_active;
	}

	// ================================================================ //
	// アイテムウインドウ.

	public class Window : WindowBase {
	
		public Window(int id, string title) : base(id, title)
		{
		}
	
		public override void	execute()
		{
			foreach(var item in this.items) {

				item.execute();
			}
		}

		public override void	onGUI()
		{
			float	x, y;
	
			x = 10.0f;
			y = 20.0f;
	
			foreach(var item in this.items) {

				item.onGUI(x, y);

				y += item.size.y + 5.0f;
			}

			this.win_rect.height = y;
		}

		// ボタンをつくる.
		public Button	createButton(string label)
		{
			var		button = new Button(label);

			this.items.Add(button);

			if(button.size.x + 20.0f > this.win_rect.width) {

				this.win_rect.width = button.size.x + 20.0f;
			}

			return(button);
		}

		// チェックボックスをつくる.
		public CheckBox	createCheckBox(string label, bool initial_value)
		{
			var		check_box = new CheckBox(label, initial_value);

			this.items.Add(check_box);

			return(check_box);
		}

		// スライダーをつくる.
		public Slider	createSlider(string label, float initial_value, float min, float max)
		{
			var		slider = new Slider(label, initial_value, min, max);
			
			this.items.Add(slider);
			
			return(slider);
		}

		// セレクターをつくる.
		public Selector	createSelector(string label, string[] texts, int value)
		{
			var		selector = new Selector(label, texts, value);
			
			this.items.Add(selector);
			
			return(selector);
		}

		public void		close()
		{
			this.is_active = false;

			dbwin.root().setActiveWindow(null);
		}

		// ---------------------------------------------------------------- //

		public List<Item>		items     = new List<Item>();
	}

	// ================================================================ //
	// コンソール.

	public class Console : WindowBase {

		public Console(int id, string title) : base(id, title)
		{
			this.win_rect.width = 600.0f;
		}

		private const float	text_top      = 20.0f;
		private const float	line_height   = 22.0f;
		private const float	button_height = 20.0f;

		public override void	onGUI()
		{
			float	x, y;
			float	w, h;

			// 最大表示行数.

			int		disp_line_count = Mathf.FloorToInt((this.win_rect.height - (text_top + button_height + 20.0f))/line_height);

			if(disp_line_count > this.lines.Count) {

				disp_line_count = this.lines.Count;
			}

			// テキストを表示する.

			x = 10.0f;
			y = text_top;
			
			for(int i = this.lines.Count - disp_line_count;i < this.lines.Count;i++) {

				var		line = this.lines[i];

				GUI.Label(new Rect(x, y, line.Length*20, line_height), line);
				y += line_height;
			}

			// コピーボタン.
			// Windows のクリップボードにテキストをコピーする.

			w = 100.0f;
			h = button_height;
			x = 10.0f;
			y = this.win_rect.height - (h + 10.0f);

			if(GUI.Button(new Rect(x, y, w, h), "コピー")) {

				string	all_texts = "";

				foreach(var line in this.lines) {

					all_texts += line + "\n";
				}
				ClipboardHelper.clipBoard = all_texts;
			}
		}

		// プリント.
		public void	print(string text)
		{
			if(this.lines.Count >= MAX_LINES) {
	
				this.lines.RemoveRange(0, 1);
			}
	
			this.lines.Add(text);
		}

		// ---------------------------------------------------------------- //

		protected List<string>	lines = new List<string>();

		protected static int	MAX_LINES = 100;
	}

	// ================================================================ //

	public static DebugWindow	root()
	{
		return(DebugWindow.get());
	}
	public static dbwin.Console	console()
	{
		return(DebugWindow.get().console);
	}
};

// 管理クラス.
public class DebugWindow : MonoBehaviour {


	public static int	GUI_DEPTH = -1;

	protected class WindowResist {

		public	dbwin.WindowBase	window;
		public	Rect				hot_button_rect;
	};

	protected	List<WindowResist>	windows = new List<WindowResist>();
	public		dbwin.Console		console;
	protected	WindowResist		active_window = null;

	protected int	next_window_id = 100;		// 次に作るウインドウのユニークな id.

	public bool	is_active = true;

	// ================================================================ //
	// MonoBehaviour からの継承.

	void	Awake()
	{
	}
	void	Start()
	{
	}
	
	void	Update()
	{
		if(this.active_window != null) {

			this.active_window.window.execute();
		}
	}

	void	OnGUI()
	{
#if UNITY_EDITOR

		do {


			if(!this.is_active) {

				break;
			}

			GUI.depth = GUI_DEPTH;
	
			// ウインドウ選択ボタン.
	
			float	x, y;
		
			x = 10.0f;
			y = Screen.height - 20.0f;
	
			foreach(var resist in this.windows) {
	
				if(GUI.Button(new Rect(x, y, 100, 20), resist.window.title)) {
	
					if(resist == this.active_window) {
	
						this.active_window = null;
	
					} else {
	
						this.active_window = resist;
					}
				}
				x += 100.0f;
			}

			//
	
			if(this.active_window != null) {
	
				var		window = this.active_window.window;

				window.win_rect = GUI.Window(window.id, window.win_rect, debug_window_function, window.title);
			}

		} while(false);
#endif
	}

	void	debug_window_function(int id)
	{
		var		resist = this.windows.Find(x => x.window.id == id);

		if(resist.window.id == id) {

			resist.window.onGUI();

			GUI.DragWindow(new Rect(0, 0, 100000, 20));
		}
	}

	// ================================================================ //

	public void	create()
	{
		this.console = new dbwin.Console(900, "console");

		this.resisterWindow(this.console);
	}

	// アクティブなウインドウをセットする.
	public void		setActiveWindow(dbwin.WindowBase window)
	{
		do {

			if(window == null) {

				this.active_window = null;
				break;
			}

			WindowResist	resist = this.windows.Find(x => x.window == window);

			if(resist == null) {

				break;
			}

			if(this.active_window != null) {
	
				this.active_window.window.is_active = false;
				this.active_window = null;
			}

			this.active_window = resist;

		} while(false);
	}

	// ウインドウを作る.
	public dbwin.Window	createWindow(string title)
	{
		var		window = new dbwin.Window(this.next_window_id, title);

		this.next_window_id++;

		this.resisterWindow(window);

		return(window);
	}

	// ウインドウを登録する.
	public void		resisterWindow(dbwin.WindowBase window)
	{
		if(this.windows.Count > 0) {

			dbwin.WindowBase	last_win = this.windows[this.windows.Count - 1].window;

			window.setPosition(last_win.win_rect.x, 20);
		}

		float	x, y;
	
		x = 10.0f + 100.0f*this.windows.Count;
		y = Screen.height - 20.0f;

		WindowResist		resist = new WindowResist();

		resist.window          = window;
		resist.hot_button_rect = new Rect(x, y, 100.0f, 20.0f);

		this.windows.Add(resist);
	}

	// ウインドウをゲットする.
	public dbwin.WindowBase	getWindow(string title)
	{
		dbwin.WindowBase	window = null;

		var		resist = this.windows.Find(x => x.window.title == title);

		if(resist != null) {

			window = resist.window;
		}

		return(window);
	}

	// ウインドウ内の座標？.
	public bool	isOcuppyRect(Vector2 pos)
	{
		bool	ret = false;

		foreach(var resist in this.windows) {

			if(resist == this.active_window) {

				if(resist.window.win_rect.Contains(pos)) {
	
					ret = true;
				}
			}
			if(resist.hot_button_rect.Contains(pos)) {

				ret = true;
			}
		}

		return(ret);
	}

	// ================================================================ //

	protected static DebugWindow	instance = null;

	public static DebugWindow	get()
	{
		if(DebugWindow.instance == null) {

			GameObject	go = new GameObject("DebugWindow");

			DebugWindow.instance = go.AddComponent<DebugWindow>();
			DebugWindow.instance.create();
		}

		return(DebugWindow.instance);
	}

}
