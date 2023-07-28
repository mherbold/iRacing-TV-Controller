
using System;
using System.Collections.Generic;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataTrackMap
	{
		public const int MaxNumCars = 63;

		public bool show;

		public int trackID;

		public float width;
		public float height;

		public Vector3 startFinishLine = Vector3.zero;

		public List<Vector3>? drawVectorList;

		public LiveDataTrackMapCar[] liveDataTrackMapCars = new LiveDataTrackMapCar[ MaxNumCars ];

		public LiveDataTrackMap()
		{
			for ( int carIndex = 0; carIndex < liveDataTrackMapCars.Length; carIndex++ )
			{
				liveDataTrackMapCars[ carIndex ] = new LiveDataTrackMapCar();
			}
		}
	}
}
