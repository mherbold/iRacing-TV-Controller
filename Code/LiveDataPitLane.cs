
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataPitLane
	{
		public bool show = false;

		public LiveDataPitLaneCar[] liveDataPitLaneCars = new LiveDataPitLaneCar[ LiveData.MaxNumDrivers ];

		public LiveDataPitLane()
		{
			for ( int carIndex = 0; carIndex < liveDataPitLaneCars.Length; carIndex++ )
			{
				liveDataPitLaneCars[ carIndex ] = new LiveDataPitLaneCar();
			}
		}
	}
}
