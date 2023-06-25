
using System;
using System.IO;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsDirector
	{
		public string filePath = string.Empty;

		public float switchDelayGeneral = 2;
		public float switchDelayRadioChatter = 1;

		public bool switchDelayGeneral_Overridden = false;
		public bool switchDelayRadioChatter_Overridden = false;

		public string camerasPractice = "tv3";
		public string camerasQualifying = "tv3";
		public string camerasIntro = "scenic";
		public string camerasInside = "roll bar";
		public string camerasClose = "tv1";
		public string camerasMedium = "tv2";
		public string camerasFar = "tv3, spectator";
		public string camerasVeryFar = "spectator, blimp";

		public bool camerasPractice_Overridden = false;
		public bool camerasQualifying_Overridden = false;
		public bool camerasIntro_Overridden = false;
		public bool camerasInside_Overridden = false;
		public bool camerasClose_Overridden = false;
		public bool camerasMedium_Overridden = false;
		public bool camerasFar_Overridden = false;
		public bool camerasVeryFar_Overridden = false;

		public string preferredCarNumber = string.Empty;
		public bool preferredCarLockOnHeatEnabled = false;
		public float preferredCarLockOnMinimumHeat = 0;

		public bool preferredCarNumber_Overridden = false;
		public bool preferredCarLockOnHeatEnabled_Overridden = false;
		public bool preferredCarLockOnMinimumMinimumHeat_Overridden = false;

		public bool switchToTalkingDrivers = true;

		public bool switchToTalkingDrivers_Overridden = false;

		public override string ToString()
		{
			return Path.GetFileNameWithoutExtension( filePath );
		}
	}
}
