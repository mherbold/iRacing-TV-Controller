
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataTrainer
	{
		public Vector3[]? drawVectorListA = null;
		public Vector3[]? drawVectorListB = null;

		public string message = string.Empty;
	}
}
