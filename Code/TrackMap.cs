
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

using SvgPathProperties;
using SvgPathProperties.Base;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public static class TrackMap
	{
		public const int numVectors = 10000;

		public const double tangentEpsilon = 0.005;

		public const int minimumDrawVectorSpacing = 10;
		public const int maximumDrawVectorSpacing = 250;

		public static int trackID = 0;
		public static bool initialized = false;

		public static Vector3[] fullVectorList = new Vector3[ numVectors ];

		public static List<Vector3> drawVectorList = new();

		public static int startFinishOffset = 0;

		public static float width = 0;
		public static float height = 0;

		public static void Initialize()
		{
			if ( trackID == IRSDK.normalizedSession.trackID )
			{
				return;
			}

			trackID = IRSDK.normalizedSession.trackID;

			initialized = false;

			LogFile.Write( "Building track map...\r\n" );

			drawVectorList.Clear();

			var startFinishPoint = GetStartFinishPoint();

			if ( startFinishPoint == null )
			{
				LogFile.Write( "Start/Finish point could not be determined.\r\n" );

				return;
			}

			var rawSvgUrl = DataApi.GetTrackMapLayerUrl( trackID, "active" );

			if ( rawSvgUrl == null )
			{
				LogFile.Write( "Active track map URL could not be determined.\r\n" );

				return;
			}

			var rawSvg = DataApi.DownloadTrackAsset( rawSvgUrl );

			if ( rawSvg == null )
			{
				LogFile.Write( "Active track map could not be fetched.\r\n" );

				return;
			}

			var splitSvgPathStringList = GetSplitSvgPathStringListFromRawSvg( rawSvg );

			if ( splitSvgPathStringList.Count == 0 )
			{
				LogFile.Write( "Could not find a valid path in the active track map.\r\n" );

				return;
			}

			var longestLength = double.NegativeInfinity;

			var minX = 0.0f;
			var minY = 0.0f;
			var maxX = 0.0f;
			var maxY = 0.0f;

			foreach ( var splitSvgPathString in splitSvgPathStringList )
			{
				var svgPath = new SvgPath( splitSvgPathString );

				var length = svgPath.Length;

				if ( longestLength > length )
				{
					continue;
				}

				longestLength = length;

				drawVectorList.Clear();

				var stepLength = length / numVectors;

				var point = svgPath.GetPointAtLength( 0 );

				var vector = new Vector3( (float) point.X, (float) point.Y, 0.0f );

				var activeTangent = svgPath.GetTangentAtLength( 0 );

				minX = vector.x;
				minY = vector.y;
				maxX = vector.x;
				maxY = vector.y;

				fullVectorList[ 0 ] = vector;

				drawVectorList.Add( vector );

				startFinishOffset = 0;

				var dSFX = vector.x - startFinishPoint.x;
				var dSFY = vector.y - startFinishPoint.y;

				var startFinishDistance = dSFX * dSFX + dSFY * dSFY;

				var spacing = 0;

				Vector3? lastVector = null;
				Point? lastTangent = null;

				for ( var i = 1; i < numVectors; i++ )
				{
					point = svgPath.GetPointAtLength( i * stepLength );

					vector = new Vector3( (float) point.X, (float) point.Y, 0.0f );

					var tangent = svgPath.GetTangentAtLength( i * stepLength );

					var dTX = tangent.X - activeTangent.X;
					var dTY = tangent.Y - activeTangent.Y;

					var dT = dTX * dTX + dTY * dTY;

					if ( ( spacing >= maximumDrawVectorSpacing ) || ( ( spacing >= minimumDrawVectorSpacing ) && ( dT >= tangentEpsilon ) ) )
					{
						if ( ( lastVector != null ) && ( lastTangent != null ) )
						{
							drawVectorList.Add( lastVector );

							activeTangent = (Point) lastTangent;
						}
						else
						{
							drawVectorList.Add( vector );

							activeTangent = tangent;
						}

						spacing = 0;

						lastVector = null;
						lastTangent = null;
					}
					else
					{
						spacing++;

						lastVector = vector;
						lastTangent = tangent;
					}

					fullVectorList[ i ] = vector;

					minX = Math.Min( minX, vector.x );
					minY = Math.Min( minY, vector.y );
					maxX = Math.Max( maxX, vector.x );
					maxY = Math.Max( maxY, vector.y );

					dSFX = vector.x - startFinishPoint.x;
					dSFY = vector.y - startFinishPoint.y;

					var newStartFinishDistance = dSFX * dSFX + dSFY * dSFY;

					if ( startFinishDistance > newStartFinishDistance )
					{
						startFinishDistance = newStartFinishDistance;

						startFinishOffset = i;
					}
				}
			}

			drawVectorList.RemoveAt( drawVectorList.Count - 1 );

			foreach ( var v in drawVectorList )
			{
				Debug.WriteLine( $"{v.x} {v.y}" );
			}

			width = maxX - minX;
			height = maxY - minY;

			var scale = 1 / width;

			for ( var i = 0; i < numVectors; i++ )
			{
				fullVectorList[ i ].x -= minX;
				fullVectorList[ i ].y -= minY;

				fullVectorList[ i ].x *= scale;
				fullVectorList[ i ].y *= scale;

				fullVectorList[ i ].y = -fullVectorList[ i ].y;
			}

			LogFile.Write( "Track map built!\r\n" );

			initialized = true;
		}

		public static Vector2? GetStartFinishPoint()
		{
			var rawSvgUrl = DataApi.GetTrackMapLayerUrl( trackID, "startfinish" );

			if ( rawSvgUrl == null )
			{
				LogFile.Write( "Start/finish track map URL could not be determined.\r\n" );

				return null;
			}

			var rawSvg = DataApi.DownloadTrackAsset( rawSvgUrl );

			if ( rawSvg == null )
			{
				LogFile.Write( "Start/finish track map could not be fetched.\r\n" );

				return null;
			}

			var splitSvgPathStringList = GetSplitSvgPathStringListFromRawSvg( rawSvg );

			if ( splitSvgPathStringList.Count == 0 )
			{
				LogFile.Write( "Could not find a valid path in the start/finish track map.\r\n" );

				return null;
			}

			var shortestLength = double.PositiveInfinity;
			var averagePoint = Vector2.zero;

			foreach ( var splitSvgPathString in splitSvgPathStringList )
			{
				var svgPath = new SvgPath( splitSvgPathString );

				var length = svgPath.Length;

				if ( shortestLength < length )
				{
					continue;
				}

				shortestLength = length;
				averagePoint = Vector2.zero;

				var stepLength = length / 100;

				var minX = double.PositiveInfinity;
				var minY = double.PositiveInfinity;
				var maxX = double.NegativeInfinity;
				var maxY = double.NegativeInfinity;

				for ( var i = 0; i < 100; i++ )
				{
					var point = svgPath.GetPointAtLength( i * stepLength );

					averagePoint.x += (float) point.X;
					averagePoint.y += (float) point.Y;

					minX = Math.Min( minX, point.X );
					minY = Math.Min( minY, point.Y );
					maxX = Math.Max( maxX, point.X );
					maxY = Math.Max( maxY, point.Y );
				}

				averagePoint.x /= 100;
				averagePoint.y /= 100;
			}

			return averagePoint;
		}

		public static List<string> GetSplitSvgPathStringListFromRawSvg( string rawSvg )
		{
			var splitSvgPathStringList = new List<string>();

			var matches = Regex.Matches( rawSvg, @" d=""([^""]*)""" );

			foreach ( Match match in matches )
			{
				var rawSvgPathString = match.Groups[ 1 ].Value;

				var rawSvgPath = new SvgPath( rawSvgPathString );

				rawSvgPathString = rawSvgPath.ToString();

				var splitSvgPathStrings = rawSvgPathString.Split( new char[] { 'M', 'Z' } );

				foreach ( var splitSvgPathString in splitSvgPathStrings )
				{
					if ( splitSvgPathString.Trim().Length > 0 )
					{
						var svgPathString = $"M{splitSvgPathString}Z";

						splitSvgPathStringList.Add( svgPathString );
					}
				}
			}

			return splitSvgPathStringList;
		}

		public static Vector3 GetPosition( float lapDistPct )
		{
			if ( initialized )
			{
				if ( !Settings.overlay.trackMapReverse )
				{
					lapDistPct = 1.0f - lapDistPct;
				}

				int index = ( (int) Math.Round( numVectors * lapDistPct ) + startFinishOffset + Settings.overlay.trackMapStartFinishOffset ) % numVectors;

				return fullVectorList[ index ];
			}
			else
			{
				return Vector3.zero;
			}
		}
	}
}
