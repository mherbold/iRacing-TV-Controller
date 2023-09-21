
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataChyron
	{
		public bool show = false;

		public string driverNameText = string.Empty;
		public string speedLabelText = string.Empty;
		public string speedText = string.Empty;
		public string gearLabelText = string.Empty;
		public string gearText = string.Empty;
		public string rpmLabelText = string.Empty;
		public string rpmText = string.Empty;
		public string ratingLabelText = string.Empty;
		public string ratingText = string.Empty;
		public string licenseLabelText = string.Empty;
		public string licenseText = string.Empty;
		public string hometownLabelText = string.Empty;
		public string hometownText = string.Empty;
		public string randomLabelText = string.Empty;
		public string randomText = string.Empty;

		public int carIdx = -1;
	}
}
