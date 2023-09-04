
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataSteamVr
	{
		public bool enabled = false;
		public float width = 1.0f;
		public Vector3 position = Vector3.zero;
		public float curvature = 0.25f;
	}
}
