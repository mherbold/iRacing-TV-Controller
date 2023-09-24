
using System;
using System.Collections.Generic;

using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController
{
	public static class EventLog
	{
		public static List<LiveDataEventLogMessage> messages = new List<LiveDataEventLogMessage>();

		public static void Reset()
		{
			messages.Clear();
		}

		public static void SessionUpdate()
		{
			var sessionTime = Program.GetTimeString( IRSDK.normalizedData.sessionTime, false );

			foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
			{
				if ( !normalizedCar.includeInLeaderboard )
				{
					continue;
				}

				if ( ( normalizedCar.activeIncidentPoints > 0 ) && ( normalizedCar.activeIncidentTimer == 0 ) )
				{
					string incidentType = "Unknown incident";

					if ( IRSDK.normalizedSession.isDirtTrack )
					{
						switch ( normalizedCar.activeIncidentPoints )
						{
							case 1: incidentType = "Wheels off the racing surface"; break;
							case 2: incidentType = "Loss of control / contact with other object / heavy contact with another driver"; break;
						}
					}
					else
					{
						switch ( normalizedCar.activeIncidentPoints )
						{
							case 1: incidentType = "Wheels off the racing surface"; break;
							case 2: incidentType = "Loss of control / contact with other object"; break;
							case 4: incidentType = "Heavy contact with another driver"; break;
						}
					}

					var liveDataEventLogMessage = new LiveDataEventLogMessage()
					{
						sessionTime = sessionTime,
						carNumber = normalizedCar.carNumber,
						driverName = normalizedCar.displayedName,
						position = $"P{normalizedCar.displayedPosition}",
						type = "Incident",
						text = $"{normalizedCar.activeIncidentPoints}x - {incidentType}"
					};

					messages.Add( liveDataEventLogMessage );
				}
			}

			if ( messages.Count > 100 )
			{
				messages.RemoveRange( 0, messages.Count - 100 );
			}
		}

		public static void Update()
		{
			var sessionTime = Program.GetTimeString( IRSDK.normalizedData.sessionTime, false );

			foreach ( SessionFlags sessionFlag in Enum.GetValues( typeof( SessionFlags ) ) )
			{
				if ( ( ( IRSDK.normalizedData.sessionFlags & (uint) sessionFlag ) != 0 ) && ( ( IRSDK.normalizedData.sessionFlagsLastFrame & (uint) sessionFlag ) ) == 0 )
				{
					if ( ( sessionFlag != SessionFlags.GreenHeld ) && ( ( sessionFlag < SessionFlags.Black ) || ( sessionFlag > SessionFlags.StartHidden ) ) )
					{
						var liveDataEventLogMessage = new LiveDataEventLogMessage()
						{
							sessionTime = sessionTime,
							carNumber = string.Empty,
							driverName = string.Empty,
							position = string.Empty,
							type = sessionFlag.ToString(),
							text = string.Empty
						};

						messages.Add( liveDataEventLogMessage );
					}
				}

				foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
				{
					if ( !normalizedCar.includeInLeaderboard )
					{
						continue;
					}

					if ( ( normalizedCar.sessionFlags & (uint) sessionFlag ) != ( normalizedCar.sessionFlagsLastFrame & (uint) sessionFlag ) )
					{
						string? text = null;

						var raised = ( normalizedCar.sessionFlags & (uint) sessionFlag ) != 0;

						switch ( sessionFlag )
						{
							case SessionFlags.Black:
								text = raised ? "Got a black flag" : "Cleared a black flag";
								break;
							case SessionFlags.Disqualify:
								text = raised ? "Got disqualified" : "Cleared disqualification";
								break;
							case SessionFlags.Furled:
								text = raised ? "Got a furled black flag" : "Cleared a furled black flag";
								break;
							case SessionFlags.Repair:
								text = raised ? "Got meatball flag" : "Cleared meatball flag";
								break;
						}

						if ( text != null )
						{
							var liveDataEventLogMessage = new LiveDataEventLogMessage()
							{
								sessionTime = sessionTime,
								carNumber = normalizedCar.carNumber,
								driverName = normalizedCar.displayedName,
								position = $"P{normalizedCar.displayedPosition}",
								type = "Black Flag",
								text = text
							};

							messages.Add( liveDataEventLogMessage );
						}
					}
				}
			}

			foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
			{
				if ( !normalizedCar.includeInLeaderboard )
				{
					if ( normalizedCar.isPaceCar && ( normalizedCar.isOnPitRoad != normalizedCar.wasOnPitRoad ) )
					{
						var liveDataEventLogMessage = new LiveDataEventLogMessage()
						{
							sessionTime = sessionTime,
							carNumber = string.Empty,
							driverName = normalizedCar.displayedName,
							position = string.Empty,
							type = "Pace Car",
							text = normalizedCar.isOnPitRoad ? "Entered pit road" : "Left pit road"
						};

						messages.Add( liveDataEventLogMessage );
					}

					continue;
				}

				if ( normalizedCar.isOnPitRoad != normalizedCar.wasOnPitRoad )
				{
					var liveDataEventLogMessage = new LiveDataEventLogMessage()
					{
						sessionTime = sessionTime,
						carNumber = normalizedCar.carNumber,
						driverName = normalizedCar.displayedName,
						position = $"P{normalizedCar.displayedPosition}",
						type = (normalizedCar.isOnPitRoad) ? "Pit In" : "Pit Out",
						text = string.Empty
					};

					messages.Add( liveDataEventLogMessage );
				}

				if ( normalizedCar.bestLapTime != normalizedCar.bestLapTimeLastFrame )
				{
					var liveDataEventLogMessage = new LiveDataEventLogMessage()
					{
						sessionTime = sessionTime,
						carNumber = normalizedCar.carNumber,
						driverName = normalizedCar.displayedName,
						position = $"P{normalizedCar.displayedPosition}",
						type = "Best Lap",
						text = Program.GetTimeString( normalizedCar.bestLapTime, true )
					};

					if ( normalizedCar.bestLapTime == IRSDK.normalizedData.bestLapTime )
					{
						liveDataEventLogMessage.text += " overall";
					}
					else
					{
						liveDataEventLogMessage.text += " personal";
					}

					messages.Add( liveDataEventLogMessage );
				}
			}

			if ( messages.Count > 100 )
			{
				messages.RemoveRange( 0, messages.Count - 100 );
			}
		}
	}
}
