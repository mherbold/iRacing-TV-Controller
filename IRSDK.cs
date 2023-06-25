
using System;
using System.Collections.Generic;
using System.Linq;

using irsdkSharp;
using irsdkSharp.Enums;
using irsdkSharp.Serialization;
using irsdkSharp.Serialization.Models.Data;
using irsdkSharp.Serialization.Models.Session;

namespace iRacingTVController
{
	public static class IRSDK
	{
		public static readonly IRacingSDK iRacingSdk = new();

		public static bool isConnected = false;
		public static bool wasConnected = false;

		public static int sessionInfoUpdate = -1;

		public static IRacingSessionModel? session = null;
		public static DataModel? data = null;

		public static NormalizedSession normalizedSession = new();
		public static NormalizedData normalizedData = new();

		public static int sendMessageWaitTicksRemaining = 0;
		public static int cameraSwitchWaitTicksRemaining = 0;

		public static int camCarIdx = 0;
		public static int camGroupNumber = 0;
		public static int camCameraNumber = 0;

		public static bool targetCamEnabled = false;
		public static bool targetCamFastSwitchEnabled = false;
		public static int targetCamCarIdx = 0;
		public static int targetCamGroupNumber = 0;
		public static string targetCamReason = string.Empty;

		public static bool targetReplayStartFrameNumberEnabled = false;
		public static int targetReplayStartFrameNumber = 0;
		public static bool targetReplayStartPlaying = false;

		public static bool targetReplayStopFrameNumberEnabled = false;
		public static int targetReplayStopFrameNumber = 0;

		public static readonly List<Message> messageBuffer = new();

		public static void Update()
		{
			isConnected = iRacingSdk.IsConnected();

			if ( isConnected )
			{
				data = iRacingSdk.GetSerializedData().Data;

				if ( ( session == null ) || ( iRacingSdk.Header.SessionInfoUpdate != sessionInfoUpdate ) )
				{
					sessionInfoUpdate = iRacingSdk.Header.SessionInfoUpdate;

					session = iRacingSdk.GetSerializedSessionInfo();

					normalizedSession.SessionUpdate();

					normalizedData.SessionUpdate();
				}

				if ( data.SessionNum != normalizedSession.sessionNumber )
				{
					normalizedSession.SessionNumberChange();

					normalizedData.SessionNumberChange();
				}

				if ( data.SessionNum >= 0 )
				{
					normalizedData.Update();
				}
			}
			else if ( wasConnected )
			{
				sessionInfoUpdate = -1;

				normalizedSession.Reset();
				normalizedData.Reset();
			}

			wasConnected = isConnected;
		}

		public static void AddMessage( BroadcastMessageTypes msg, int var1, int var2, int var3 )
		{
			messageBuffer.Add( new Message( msg, var1, var2, var3 ) );
		}

		public static void SendMessage( Message message )
		{
			LogFile.Write( $"Sending message to iRacing: {message.msg}, {message.var1}, {message.var2}, {message.var3}\r\n" );

			iRacingSdk.BroadcastMessage( message.msg, message.var1, message.var2, message.var3 );

			sendMessageWaitTicksRemaining = (int) Math.Ceiling( 60.0f / Settings.editor.iracingCommandRateLimit );

			if ( message.msg == BroadcastMessageTypes.CamSwitchNum )
			{
				cameraSwitchWaitTicksRemaining = (int) Math.Round( Settings.director.switchDelayGeneral * 60 );
			}
		}

		public static void SendMessages()
		{
			// reduce wait ticks by one

			sendMessageWaitTicksRemaining--;
			cameraSwitchWaitTicksRemaining--;

			// if iracing has switched the camera then we need to reset the camera switch wait ticks

			if ( ( normalizedData.camCarIdx != camCarIdx ) || ( normalizedData.camGroupNumber != camGroupNumber ) || ( normalizedData.camCameraNumber != camCameraNumber ) )
			{
				camCarIdx = normalizedData.camCarIdx;
				camGroupNumber = normalizedData.camGroupNumber;
				camCameraNumber = normalizedData.camCameraNumber;

				cameraSwitchWaitTicksRemaining = (int) Math.Round( Settings.director.switchDelayGeneral * 60 );
			}

			// send the next message in the queue

			if ( sendMessageWaitTicksRemaining <= 0 )
			{
				if ( messageBuffer.Count > 0 )
				{
					var message = messageBuffer.First();

					messageBuffer.RemoveAt( 0 );

					SendMessage( message );
				}
			}

			// send message to switch the camera if target camera is enabled and the current camera is not the target camera

			if ( sendMessageWaitTicksRemaining <= 0 )
			{
				if ( targetCamEnabled )
				{
					if ( ( camCarIdx != targetCamCarIdx ) || ( camGroupNumber != targetCamGroupNumber ) )
					{
						if ( ( cameraSwitchWaitTicksRemaining <= 0 ) || targetCamFastSwitchEnabled )
						{
							var normalizedCar = normalizedData.FindNormalizedCarByCarIdx( targetCamCarIdx );

							if ( normalizedCar != null )
							{
								var message = new Message( BroadcastMessageTypes.CamSwitchNum, normalizedCar.carNumberRaw, targetCamGroupNumber, 0 );

								SendMessage( message );
							}
						}
					}
					else
					{
						targetCamEnabled = false;
						targetCamFastSwitchEnabled = false;
					}
				}
			}

			// send message to pause the replay if target frame number is enabled and we are not paused

			if ( sendMessageWaitTicksRemaining <= 0 )
			{
				if ( targetReplayStartFrameNumberEnabled )
				{
					if ( normalizedData.replaySpeed != 0 )
					{
						var message = new Message( BroadcastMessageTypes.ReplaySetPlaySpeed, 0, 0, 0 );

						SendMessage( message );
					}
				}
			}

			// send message to change the frame number if target frame number is enabled and we are not on it (also auto-start playing if enabled)

			if ( sendMessageWaitTicksRemaining <= 0 )
			{
				if ( targetReplayStartFrameNumberEnabled )
				{
					if ( normalizedData.replayFrameNum != targetReplayStartFrameNumber )
					{
						var message = new Message( BroadcastMessageTypes.ReplaySetPlayPosition, (int) ReplayPositionModeTypes.Begin, IRacingSDK.LoWord( targetReplayStartFrameNumber ), IRacingSDK.HiWord( targetReplayStartFrameNumber ) );

						SendMessage( message );
					}
					else
					{
						targetReplayStartFrameNumberEnabled = false;

						if ( targetReplayStartPlaying )
						{
							targetReplayStartPlaying = false;

							var message = new Message( BroadcastMessageTypes.ReplaySetPlaySpeed, 1, 0, 0 );

							SendMessage( message );
						}
					}
				}
			}

			// send message to auto-stop replay if enabled

			if ( sendMessageWaitTicksRemaining <= 0 )
			{
				if ( targetReplayStopFrameNumberEnabled )
				{
					if ( normalizedData.replaySpeed != 0 )
					{
						if ( normalizedData.replayFrameNum >= targetReplayStopFrameNumber )
						{
							var message = new Message( BroadcastMessageTypes.ReplaySetPlaySpeed, 0, 0, 0 );

							SendMessage( message );
						}
					}
					else
					{
						targetReplayStopFrameNumberEnabled = false;
					}
				}
			}
		}

		public static int GetCamGroupNumber( string cameraGroupNames )
		{
			if ( session == null )
			{
				return 0;
			}

			var selectedCameraGroupList = cameraGroupNames.Split( "," ).ToList().Select( s => s.Trim().ToLower() ).ToList();

			foreach ( var selectedCameraGroup in selectedCameraGroupList )
			{
				foreach ( var group in session.CameraInfo.Groups )
				{
					if ( group.GroupName.ToLower() == selectedCameraGroup )
					{
						return group.GroupNum;
					}
				}
			}

			return 0;
		}
	}
}
