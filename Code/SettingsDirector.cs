
using System;
using System.IO;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsDirector
	{
		public enum CameraType
		{
			Practice,
			Qualifying,
			Intro,
			Inside,
			Close,
			Medium,
			Far,
			VeryFar,
			AutoCam
		}

		public string filePath = string.Empty;

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

		public float switchDelayDirector = 5;
		public float switchDelayIracing = 2;
		public float switchDelayRadioChatter = 1;
		public float switchDelayNotInRace = 10;

		public bool switchDelayDirector_Overridden = false;
		public bool switchDelayIracing_Overridden = false;
		public bool switchDelayRadioChatter_Overridden = false;
		public bool switchDelayNotInRace_Overridden = false;

		public float heatCarLength = 4.91f;
		public float heatFalloff = 20.0f;
		public float heatBias = 0.5f;

		public bool heatCarLength_Overridden = false;
		public bool heatFalloff_Overridden = false;
		public bool heatBias_Overridden = false;

		public string preferredCarNumber = string.Empty;
		public bool preferredCarLockOnEnabled = false;
		public float preferredCarLockOnMinimumHeat = 0;

		public bool preferredCarNumber_Overridden = false;
		public bool preferredCarLockOnEnabled_Overridden = false;
		public bool preferredCarLockOnMinimumHeat_Overridden = false;

		public bool rule1_Enabled = true;
		public CameraType rule1_Camera = CameraType.Close;
		public bool rule2_Enabled = true;
		public CameraType rule2_Camera = CameraType.AutoCam;
		public bool rule3_Enabled = true;
		public CameraType rule3_Camera = CameraType.Medium;
		public bool rule4_Enabled = true;
		public CameraType rule4_Camera = CameraType.Medium;
		public bool rule5_Enabled = true;
		public CameraType rule5_Camera = CameraType.Far;
		public bool rule6_Enabled = true;
		public CameraType rule6_Camera = CameraType.Close;
		public bool rule7_Enabled = true;
		public CameraType rule7_Camera = CameraType.Medium;
		public bool rule8_Enabled = true;
		public CameraType rule8_Camera = CameraType.Medium;
		public bool rule9_Enabled = true;
		public CameraType rule9_Camera = CameraType.VeryFar;
		public bool rule10_Enabled = true;
		public CameraType rule10_Camera = CameraType.Medium;
		public bool rule11_Enabled = true;
		public CameraType rule11_Camera = CameraType.Intro;
		public bool rule12_Enabled = true;
		public CameraType rule12_Camera = CameraType.AutoCam;
		public bool rule13_Enabled = true;
		public CameraType rule13_Camera = CameraType.AutoCam;

		public bool rules_Overridden = false;

		public float autoCamInsideMinimum = 2.5f;
		public float autoCamInsideMaximum = 12.0f;
		public float autoCamCloseMaximum = 10.0f;
		public float autoCamMediumMaximum = 20.0f;
		public float autoCamFarMaximum = 30.0f;

		public bool autoCam_Overridden = false;

		public override string ToString()
		{
			return Path.GetFileNameWithoutExtension( filePath );
		}
	}
}
