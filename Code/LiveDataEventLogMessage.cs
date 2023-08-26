
using System;
using System.Text.Json.Serialization;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataEventLogMessage
	{
		[JsonInclude] public string sessionTime = string.Empty;
		[JsonInclude] public string carNumber = string.Empty;
		[JsonInclude] public string driverName = string.Empty;
		[JsonInclude] public string position = string.Empty;
		[JsonInclude] public string type = string.Empty;
		[JsonInclude] public string text = string.Empty;
	}
}
