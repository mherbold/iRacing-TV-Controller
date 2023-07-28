
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataTrackMapCar
	{
		public bool show = false;
		public bool showHighlight = false;

		public Vector3 offset = Vector3.zero;
	}
}
