
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataIntro
	{
		public bool show = false;
		public int currentRow = 0;

		public LiveDataIntroDriver[] liveDataIntroDrivers = new LiveDataIntroDriver[ LiveDataLeaderboard.MaxNumPlaces ];

		public LiveDataIntro()
		{
			for ( int placeIndex = 0; placeIndex < liveDataIntroDrivers.Length; placeIndex++ )
			{
				liveDataIntroDrivers[ placeIndex ] = new LiveDataIntroDriver();
			}
		}
	}
}
