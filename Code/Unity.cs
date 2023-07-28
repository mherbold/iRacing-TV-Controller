
using System;
using System.Text.RegularExpressions;

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

			public static Vector2 operator +( Vector2 a, Vector2 b )
			{
				return new Vector2( a.x + b.x, a.y + b.y );
			}

			public static Vector2 operator -( Vector2 a, Vector2 b )
			{
				return new Vector2( a.x - b.x, a.y - b.y );
			}

			public static Vector2 operator *( Vector2 a, float d )
			{
				return new Vector2( a.x * d, a.y * d );
			}

			public static Vector2 operator /( Vector2 a, float d )
			{
				return new Vector2( a.x / d, a.y / d );
			}

			public static float Distance( Vector2 a, Vector2 b )
			{
				float dx = a.x - b.x;
				float dy = a.y - b.y;

				return (float) Math.Sqrt( dx * dx + dy * dy );
			}

			public Vector2 normalized
			{
				get
				{
					Vector2 result = new Vector2( x, y );

					result.Normalize();

					return result;
				}
			}

			public float magnitude
			{
				get
				{
					return (float) Math.Sqrt( x * x + y * y );
				}
			}

			public void Normalize()
			{
				float num = magnitude;

				if ( num > 1E-05f )
				{
					x /= num;
					y /= num;
				}
				else
				{
					x = 0;
					y = 0;
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
		public class Vector3
		{
			public float x;
			public float y;
			public float z;

			public Vector3()
			{
				x = 0;
				y = 0;
				z = 0;
			}

			public Vector3( float x, float y, float z )
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}

			public static Vector3 zero
			{
				get
				{
					return new Vector3( 0, 0, 0 );
				}
			}
		}

		[Serializable]
		public class Vector4
		{
			public float x;
			public float y;
			public float z;
			public float w;

			public Vector4()
			{
				x = 0;
				y = 0;
				z = 0;
				w = 0;
			}

			public Vector4( float x, float y, float z, float w )
			{
				this.x = x;
				this.y = y;
				this.z = z;
				this.w = w;
			}

			public static Vector4 zero
			{
				get
				{
					return new Vector4( 0, 0, 0, 0 );
				}
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

			public Color( string hex )
			{
				var match = Regex.Match( hex, @"([\da-f]{2})([\da-f]{2})([\da-f]{2})", RegexOptions.IgnoreCase );

				if ( match.Success )
				{
					r = int.Parse( match.Groups[ 1 ].Value, System.Globalization.NumberStyles.HexNumber ) / 255.0f;
					g = int.Parse( match.Groups[ 2 ].Value, System.Globalization.NumberStyles.HexNumber ) / 255.0f;
					b = int.Parse( match.Groups[ 3 ].Value, System.Globalization.NumberStyles.HexNumber ) / 255.0f;
					a = 1;
				}
				else
				{
					r = 0;
					g = 0;
					b = 0;
					a = 0;
				}
			}

			public override string ToString()
			{
				var r = (int) ( this.r * 255 );
				var g = (int) ( this.g * 255 );
				var b = (int) ( this.b * 255 );

				return $"{r:X2}{g:X2}{b:X2}";
			}

			public static Color white
			{
				get
				{
					return new Color( 1f, 1f, 1f, 1f );
				}
			}

			public static Color black
			{
				get
				{
					return new Color( 0f, 0f, 0f, 1f );
				}
			}

			public static Color Lerp( Color a, Color b, float t )
			{
				t = Math.Clamp( t, 0, 1 );

				return new Color( a.r + ( b.r - a.r ) * t, a.g + ( b.g - a.g ) * t, a.b + ( b.b - a.b ) * t, a.a + ( b.a - a.a ) * t );
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
