
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SvgPathProperties;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public static class TrackMap
	{
		public const int numIterations = 10000;
		public const double tangentEpsilon = 0.05;

		public static int trackID = 0;
		public static bool initialized = false;

		public static Vector3[] fullVectorList = new Vector3[ numIterations ];

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

			var startFinishPoint = GetStartFinishPoint();

			if ( startFinishPoint == null )
			{
				return;
			}

			var rawSvgUrl = DataApi.GetTrackMapLayerUrl( trackID, "active" );

			if ( rawSvgUrl == null )
			{
				return;
			}

			var rawSvg = DataApi.DownloadTrackAsset( rawSvgUrl );

			if ( rawSvg == null )
			{
				return;
			}

			var match = Regex.Match( rawSvg, " d=\"([^\"]*)\"" );

			if ( !match.Success )
			{
				return;
			}

			var rawSvgPathString = match.Groups[ 1 ].Value;

			var rawSvgPath = new SvgPath( rawSvgPathString );

			rawSvgPathString = rawSvgPath.ToString();

			var svgPathStrings = rawSvgPathString.Split( 'Z' );

			svgPathStrings[ 0 ] += 'Z';

			var svgPath = new SvgPath( svgPathStrings[ 0 ] );

			var length = svgPath.Length;
			var stepLength = length / numIterations;

			var point = svgPath.GetPointAtLength( 0 );
			var tangent = svgPath.GetTangentAtLength( 0 );

			var vector = new Vector3( (float) point.X, (float) point.Y, 0.0f );

			var minX = vector.x;
			var minY = vector.y;
			var maxX = vector.x;
			var maxY = vector.x;

			fullVectorList[ 0 ] = vector;

			drawVectorList.Add( vector );

			var lastPointAdded = true;

			startFinishOffset = 0;

			var dSFX = vector.x - startFinishPoint.x;
			var dSFY = vector.y - startFinishPoint.y;

			var startFinishDistance = dSFX * dSFX + dSFY * dSFY;

			for ( var i = 1; i < numIterations; i++ )
			{
				var newTangent = svgPath.GetTangentAtLength( i * stepLength );

				var dTX = newTangent.X - tangent.X;
				var dTY = newTangent.Y - tangent.Y;

				if ( ( dTX * dTX + dTY * dTY ) > tangentEpsilon )
				{
					if ( !lastPointAdded )
					{
						drawVectorList.Add( vector );

						lastPointAdded = true;
					}

					point = svgPath.GetPointAtLength( i * stepLength );

					tangent = newTangent;

					vector = new Vector3( (float) point.X, (float) point.Y, 0.0f );

					drawVectorList.Add( vector );
				}
				else
				{
					point = svgPath.GetPointAtLength( i * stepLength );

					vector = new Vector3( (float) point.X, (float) point.Y, 0.0f );

					lastPointAdded = false;
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

			width = maxX - minX;
			height = maxY - minY;

			var scale = 1 / width;

			for ( var i = 0; i < numIterations; i++ )
			{
				fullVectorList[ i ].x -= minX;
				fullVectorList[ i ].y -= minY;

				fullVectorList[ i ].x *= scale;
				fullVectorList[ i ].y *= scale;

				fullVectorList[ i ].y = -fullVectorList[ i ].y;
			}

			initialized = true;
		}

		public static Vector2? GetStartFinishPoint()
		{
			var rawSvgUrl = DataApi.GetTrackMapLayerUrl( trackID, "startfinish" );

			if ( rawSvgUrl == null )
			{
				return null;
			}

			var rawSvg = DataApi.DownloadTrackAsset( rawSvgUrl );

			if ( rawSvg == null )
			{
				return null;
			}

			var match = Regex.Match( rawSvg, " d=\"([^\"]*)\"" );

            if ( !match.Success )
            {
				return null;
            }

            var rawSvgPathString = match.Groups[ 1 ].Value;

			var rawSvgPath = new SvgPath( rawSvgPathString );

			rawSvgPathString = rawSvgPath.ToString();

			var svgPathStrings = rawSvgPathString.Split( 'Z' );

			svgPathStrings[ 0 ] += 'Z';

			var svgPath = new SvgPath( svgPathStrings[ 0 ] );

			var length = svgPath.Length;
			var stepLength = length / 100;

			var averagePoint = Vector2.zero;

			for ( var i = 0; i < 100; i++ )
			{
				var point = svgPath.GetPointAtLength( i * stepLength );

				averagePoint.x += (float) point.X;
				averagePoint.y += (float) point.Y;
			}

			averagePoint.x /= 100;
			averagePoint.y /= 100;

			return averagePoint;
		}

		public static Vector3 GetPosition( float lapDistPct )
		{
			if ( initialized )
			{
				int index = ( (int) Math.Round( numIterations * ( 1.0f - lapDistPct ) ) + startFinishOffset ) % numIterations;

				return fullVectorList[ index ];
			}
			else
			{
				return Vector3.zero;
			}
		}
	}
}
