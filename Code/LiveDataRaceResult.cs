
using System;
using System.Text.Json.Serialization;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataRaceResult
	{
		public bool show = false;

		public Vector2 backgroundSize = Vector2.zero;

		[JsonInclude] public Color classColor = Color.white;
		[JsonInclude] public string textLayer1 = string.Empty;
		public string textLayer2 = string.Empty;

		[JsonInclude] public LiveDataRaceResultSlot[] liveDataRaceResultSlots = new LiveDataRaceResultSlot[ LiveData.MaxNumDrivers ];

		public LiveDataRaceResult()
		{
			for ( var slotIndex = 0; slotIndex < liveDataRaceResultSlots.Length; slotIndex++ )
			{
				liveDataRaceResultSlots[ slotIndex ] = new LiveDataRaceResultSlot();
			}
		}
	}
}
