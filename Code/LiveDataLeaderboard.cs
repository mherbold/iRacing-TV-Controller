
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataLeaderboard
	{
		public const int MaxNumSlots = 63;

		public bool show = false;
		public bool showSplitter = false;

		public Vector2 backgroundSize = Vector2.zero;
		public Vector2 splitterPosition = Vector2.zero;

		public LiveDataLeaderboardSlot[] liveDataLeaderboardSlots = new LiveDataLeaderboardSlot[ MaxNumSlots ];

		public LiveDataLeaderboard()
		{
			for ( int slotIndex = 0; slotIndex < liveDataLeaderboardSlots.Length; slotIndex++ )
			{
				liveDataLeaderboardSlots[ slotIndex ] = new LiveDataLeaderboardSlot();
			}
		}
	}
}
