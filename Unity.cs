
using System;

namespace iRacingTVController
{
	public static class Unity
	{
		[Serializable]
		public class Vector2
		{
			public float x;
			public float y;

			public Vector2()
			{
				x = 0;
				y = 0;
			}

			public Vector2( float x, float y )
			{
				this.x = x;
				this.y = y;
			}

			public static Vector2 zero
			{
				get
				{
					return new Vector2( 0, 0 );
				}
			}
		}

		[Serializable]
		public class Vector2Int
		{
			public int x;
			public int y;

			public Vector2Int()
			{
				x = 0;
				y = 0;
			}

			public Vector2Int( int x, int y )
			{
				this.x = x;
				this.y = y;
			}
		}

		[Serializable]
		public class Color
		{
			public float r;
			public float g;
			public float b;
			public float a;

			public Color()
			{
				r = 0;
				g = 0;
				b = 0;
				a = 0;
			}

			public Color( float r, float g, float b, float a )
			{
				this.r = r;
				this.g = g;
				this.b = b;
				this.a = a;
			}

			public static Color white
			{
				get
				{
					return new Color( 1f, 1f, 1f, 1f );
				}
			}
		}

		[Serializable]
		public enum TextAlignmentOptions
		{
			TopLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Top,
			Top = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Top,
			TopRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Top,
			TopJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Top,
			TopFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Top,
			TopGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Top,

			Left = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Middle,
			Center = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Middle,
			Right = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Middle,
			Justified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Middle,
			Flush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Middle,
			CenterGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Middle,

			BottomLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Bottom,
			Bottom = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Bottom,
			BottomRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Bottom,
			BottomJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Bottom,
			BottomFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Bottom,
			BottomGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Bottom,

			BaselineLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Baseline,
			Baseline = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Baseline,
			BaselineRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Baseline,
			BaselineJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Baseline,
			BaselineFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Baseline,
			BaselineGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Baseline,

			MidlineLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Geometry,
			Midline = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Geometry,
			MidlineRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Geometry,
			MidlineJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Geometry,
			MidlineFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Geometry,
			MidlineGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Geometry,

			CaplineLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Capline,
			Capline = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Capline,
			CaplineRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Capline,
			CaplineJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Capline,
			CaplineFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Capline,
			CaplineGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Capline,

			Converted = 0xFFFF
		};

		[Serializable]
		public enum HorizontalAlignmentOptions
		{
			Left = 0x1, Center = 0x2, Right = 0x4, Justified = 0x8, Flush = 0x10, Geometry = 0x20
		}

		[Serializable]
		public enum VerticalAlignmentOptions
		{
			Top = 0x100, Middle = 0x200, Bottom = 0x400, Baseline = 0x800, Geometry = 0x1000, Capline = 0x2000,
		}
	}
}
