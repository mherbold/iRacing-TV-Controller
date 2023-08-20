
using System;
using System.Text.Json.Serialization;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataTrackMapCar
	{
		[JsonInclude] public bool show = false;
		public bool showHighlight = false;

		[JsonInclude] public Vector3 offset = Vector3.zero;
	}
}
