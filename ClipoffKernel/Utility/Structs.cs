using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Clipoff
    {
	[StructLayout(LayoutKind.Sequential), Serializable()]
	public struct SIZE {
		public int width;
		public int height;
		public SIZE(Size size) : this(size.Width, size.Height) {
			
		}
		public SIZE(int width, int height) {
			this.width = width;
			this.height = height;
		}
		public Size ToSize() {
			return new Size(width, height);
		}
	}
	[StructLayout(LayoutKind.Sequential), Serializable()]
	public struct POINT {
		public int X;
		public int Y;

		public POINT(int x, int y) {
			this.X = x;
			this.Y = y;
		}
		public POINT(Point point) {
			this.X = point.X;
			this.Y = point.Y;
		}

		public static implicit operator System.Drawing.Point(POINT p) {
			return new System.Drawing.Point(p.X, p.Y);
		}

		public static implicit operator POINT(System.Drawing.Point p) {
			return new POINT(p.X, p.Y);
		}

		public Point ToPoint() {
			return new Point(X, Y);
		}

		override public string ToString() {
			return X + "," + Y;
		}
	}

	[StructLayout(LayoutKind.Sequential), Serializable()]
	public struct RECT {
		private int _Left;
		private int _Top;
		private int _Right;
		private int _Bottom;

		public RECT(RECT rectangle)
			: this(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom) {
		}
		public RECT(Rectangle rectangle)
			: this(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom) {
		}
		public RECT(int Left, int Top, int Right, int Bottom) {
			_Left = Left;
			_Top = Top;
			_Right = Right;
			_Bottom = Bottom;
		}

		public int X {
			get {
				return _Left;
			}
			set {
				_Left = value;
			}
		}
		public int Y {
			get {
				return _Top;
			}
			set {
				_Top = value;
			}
		}
		public int Left {
			get {
				return _Left;
			}
			set {
				_Left = value;
			}
		}
		public int Top {
			get {
				return _Top;
			}
			set {
				_Top = value;
			}
		}
		public int Right {
			get {
				return _Right;
			}
			set {
				_Right = value;
			}
		}
		public int Bottom {
			get {
				return _Bottom;
			}
			set {
				_Bottom = value;
			}
		}
		public int Height {
			get {
				return _Bottom - _Top;
			}
			set {
				_Bottom = value - _Top;
			}
		}
		public int Width {
			get {
				return _Right - _Left;
			}
			set {
				_Right = value + _Left;
			}
		}
		public Point Location {
			get {
				return new Point(Left, Top);
			}
			set {
				_Left = value.X;
				_Top = value.Y;
			}
		}
		public Size Size {
			get {
				return new Size(Width, Height);
			}
			set {
				_Right = value.Width + _Left;
				_Bottom = value.Height + _Top;
			}
		}

		public static implicit operator Rectangle(RECT Rectangle) {
			return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
		}
		public static implicit operator RECT(Rectangle Rectangle) {
			return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
		}
		public static bool operator ==(RECT Rectangle1, RECT Rectangle2) {
			return Rectangle1.Equals(Rectangle2);
		}
		public static bool operator !=(RECT Rectangle1, RECT Rectangle2) {
			return !Rectangle1.Equals(Rectangle2);
		}

		public override string ToString() {
			return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
		}

		public override int GetHashCode() {
			return ToString().GetHashCode();
		}

		public bool Equals(RECT Rectangle) {
			return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
		}

		public Rectangle ToRectangle() {
			return new Rectangle(Left, Top, Width, Height);
		}
		public override bool Equals(object Object) {
			if (Object is RECT) {
				return Equals((RECT)Object);
			} else if (Object is Rectangle) {
				return Equals(new RECT((Rectangle)Object));
			}

			return false;
		}
	}

	/// <summary>
	/// The structure for the WindowInfo
	/// See: http://msdn.microsoft.com/en-us/library/windows/desktop/ms632610%28v=vs.85%29.aspx
	/// </summary>
	[StructLayout(LayoutKind.Sequential), Serializable()]
	public struct WindowInfo {
		public uint cbSize;
		public RECT rcWindow;
		public RECT rcClient;
		public uint dwStyle;
		public uint dwExStyle;
		public uint dwWindowStatus;
		public uint cxWindowBorders;
		public uint cyWindowBorders;
		public ushort atomWindowType;
		public ushort wCreatorVersion;
		// Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
		public WindowInfo(Boolean? filler) : this() {
			cbSize = (UInt32)(Marshal.SizeOf(typeof(WindowInfo)));
		}
	}

	/// <summary>
	/// Contains information about the placement of a window on the screen.
	/// </summary>
	[StructLayout(LayoutKind.Sequential), Serializable()]
	public struct WindowPlacement {
		/// <summary>
		/// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
		/// <para>
		/// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
		/// </para>
		/// </summary>
		public int Length;

		/// <summary>
		/// Specifies flags that control the position of the minimized window and the method by which the window is restored.
		/// </summary>
		public WindowPlacementFlags Flags;

		/// <summary>
		/// The current show state of the window.
		/// </summary>
		public ShowWindowCommand ShowCmd;

		/// <summary>
		/// The coordinates of the window's upper-left corner when the window is minimized.
		/// </summary>
		public POINT MinPosition;

		/// <summary>
		/// The coordinates of the window's upper-left corner when the window is maximized.
		/// </summary>
		public POINT MaxPosition;

		/// <summary>
		/// The window's coordinates when the window is in the restored position.
		/// </summary>
		public RECT NormalPosition;

		/// <summary>
		/// Gets the default (empty) value.
		/// </summary>
		public static WindowPlacement Default {
			get {
				WindowPlacement result = new WindowPlacement();
				result.Length = Marshal.SizeOf(result);
				return result;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct CursorInfo {
		public Int32 cbSize;
		public Int32 flags;
		public IntPtr hCursor;
		public POINT ptScreenPos;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IconInfo {
		public bool fIcon;
		public Int32 xHotspot;
		public Int32 yHotspot;
		public IntPtr hbmMask;
		public IntPtr hbmColor;
	}

	[Serializable, StructLayout(LayoutKind.Sequential)]
	public struct SCROLLINFO {
		public int cbSize;
		public int fMask;
		public int nMin;
		public int nMax;
		public int nPage;
		public int nPos;
		public int nTrackPos;
	}

}

