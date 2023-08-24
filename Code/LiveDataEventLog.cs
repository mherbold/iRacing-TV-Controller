﻿
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataEventLog
	{
		[JsonInclude] public List<string> messages = new List<string>();
	}
}
