
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataLeaderboard
	{
		public const int MaxNumPlaces = 63;

		public bool show = false;
		public bool showSplitter = false;

		public LiveDataPlace[] liveDataPlaces = new LiveDataPlace[ MaxNumPlaces ];

		public LiveDataLeaderboard()
		{
			for ( int placeIndex = 0; placeIndex < liveDataPlaces.Length; placeIndex++ )
			{
				liveDataPlaces[ placeIndex ] = new LiveDataPlace();
			}
		}
	}
}
