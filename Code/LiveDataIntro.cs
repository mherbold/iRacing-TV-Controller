﻿
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataIntro
	{
		public bool show = false;

		public LiveDataIntroDriver[] liveDataIntroDrivers = new LiveDataIntroDriver[ LiveData.MaxNumDrivers ];

		public LiveDataIntro()
		{
			for ( int liveDataIntroDriverIndex = 0; liveDataIntroDriverIndex < liveDataIntroDrivers.Length; liveDataIntroDriverIndex++ )
			{
				liveDataIntroDrivers[ liveDataIntroDriverIndex ] = new LiveDataIntroDriver();
			}
		}
	}
}
