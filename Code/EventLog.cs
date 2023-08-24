
using System.Collections.Generic;

namespace iRacingTVController
{
	public static class EventLog
	{
		public static List<string> messages = new List<string>();

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

					messages.Add( $"{sessionTime}: Car #{normalizedCar.carNumber}, {normalizedCar.abbrevName}, P{normalizedCar.displayedPosition}: Incident - {normalizedCar.activeIncidentPoints}x - {incidentType}" );
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

			foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
			{
				if ( !normalizedCar.includeInLeaderboard )
				{
					continue;
				}

				if ( normalizedCar.isOnPitRoad && !normalizedCar.wasOnPitRoad )
				{
					messages.Add( $"{sessionTime}: Car #{normalizedCar.carNumber}, {normalizedCar.abbrevName}, P{normalizedCar.displayedPosition}: Entered pit road" );
				}

				if ( !normalizedCar.isOnPitRoad && normalizedCar.wasOnPitRoad )
				{
					messages.Add( $"{sessionTime}: Car #{normalizedCar.carNumber}, {normalizedCar.abbrevName}, P{normalizedCar.displayedPosition}: Left pit road" );
				}

				if ( normalizedCar.isOutOfCar && !normalizedCar.wasOutOfCar )
				{
					messages.Add( $"{sessionTime}: Car #{normalizedCar.carNumber}, {normalizedCar.abbrevName}, P{normalizedCar.displayedPosition}: Jumped out of their car" );
				}

				if ( !normalizedCar.isOutOfCar && normalizedCar.wasOutOfCar )
				{
					messages.Add( $"{sessionTime}: Car #{normalizedCar.carNumber}, {normalizedCar.abbrevName}, P{normalizedCar.displayedPosition}: Jumped into their car" );
				}
			}

			if ( messages.Count > 100 )
			{
				messages.RemoveRange( 0, messages.Count - 100 );
			}
		}
	}
}
