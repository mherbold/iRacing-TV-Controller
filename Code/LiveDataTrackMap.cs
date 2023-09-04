
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataTrackMap
	{
		public const int MaxNumCars = 64;

		public bool show;

		public int trackID;

		[JsonInclude] public float width;
		[JsonInclude] public float height;

		[JsonInclude] public Vector3 startFinishLine = Vector3.zero;

		[JsonInclude] public List<Vector3>? drawVectorList;

		[JsonInclude] public LiveDataTrackMapCar[] liveDataTrackMapCars = new LiveDataTrackMapCar[ MaxNumCars ];

		public LiveDataTrackMap()
		{
			for ( int carIndex = 0; carIndex < liveDataTrackMapCars.Length; carIndex++ )
			{
				liveDataTrackMapCars[ carIndex ] = new LiveDataTrackMapCar();
			}
		}
	}
}
