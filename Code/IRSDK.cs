﻿
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

		public static float sendMessageWaitTimeRemaining = 0;
		public static float cameraSwitchWaitTimeRemaining = 0;

		public static int camCarIdx = 0;
		public static int camGroupNumber = 0;
		public static int camCameraNumber = 0;

		public static bool targetCamEnabled = false;
		public static bool targetCamFastSwitchEnabled = false;
		public static bool targetCamSlowSwitchEnabled = false;
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
					targetCamSlowSwitchEnabled = false;

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

			sendMessageWaitTimeRemaining = 1.0f / Settings.editor.iracingGeneralCommandRateLimit;

			if ( message.msg == BroadcastMessageTypes.CamSwitchNum )
			{
				cameraSwitchWaitTimeRemaining = Settings.director.switchDelayDirector;
			}
		}

		public static void SendMessages()
		{
			if ( isConnected )
			{
				// update wait timers

				sendMessageWaitTimeRemaining = Math.Max( 0, sendMessageWaitTimeRemaining - Program.deltaTime );
				cameraSwitchWaitTimeRemaining = Math.Max( 0, cameraSwitchWaitTimeRemaining - Program.deltaTime );

				// if iracing has switched the camera then we need to reset the camera switch wait ticks

				if ( ( normalizedData.camCarIdx != camCarIdx ) || ( normalizedData.camGroupNumber != camGroupNumber ) || ( normalizedData.camCameraNumber != camCameraNumber ) )
				{
					camCarIdx = normalizedData.camCarIdx;
					camGroupNumber = normalizedData.camGroupNumber;
					camCameraNumber = normalizedData.camCameraNumber;

					if ( cameraSwitchWaitTimeRemaining < Settings.director.switchDelayIracing )
					{
						cameraSwitchWaitTimeRemaining = Settings.director.switchDelayIracing;
					}
				}

				// send the next message in the queue

				if ( sendMessageWaitTimeRemaining <= 0 )
				{
					if ( messageBuffer.Count > 0 )
					{
						var message = messageBuffer.First();

						messageBuffer.RemoveAt( 0 );

						SendMessage( message );
					}
				}

				// send message to pause the replay if target frame number is enabled and we are not paused

				if ( sendMessageWaitTimeRemaining <= 0 )
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

				if ( sendMessageWaitTimeRemaining <= 0 )
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

				if ( sendMessageWaitTimeRemaining <= 0 )
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

				// send message to switch the camera if target camera is enabled and the current camera is not the target camera

				if ( sendMessageWaitTimeRemaining <= 0 )
				{
					if ( targetCamEnabled )
					{
						if ( ( camCarIdx != targetCamCarIdx ) || ( camGroupNumber != targetCamGroupNumber ) )
						{
							if ( ( cameraSwitchWaitTimeRemaining <= 0 ) || targetCamFastSwitchEnabled )
							{
								var normalizedCar = normalizedData.FindNormalizedCarByCarIdx( targetCamCarIdx );

								if ( normalizedCar != null )
								{
									var message = new Message( BroadcastMessageTypes.CamSwitchNum, normalizedCar.carNumberRaw, targetCamGroupNumber, 0 );

									SendMessage( message );

									if ( targetCamSlowSwitchEnabled )
									{
										targetCamSlowSwitchEnabled = false;

										cameraSwitchWaitTimeRemaining = Settings.director.switchDelayNotInRace;
									}
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
			}
			else
			{
				sendMessageWaitTimeRemaining = 0;
				cameraSwitchWaitTimeRemaining = 0;

				targetCamEnabled = false;
				targetReplayStartFrameNumberEnabled = false;
				targetReplayStopFrameNumberEnabled = false;

				messageBuffer.Clear();
			}
		}

		public static int GetCamGroupNumber( SettingsDirector.CameraType cameraType )
		{
			switch ( cameraType )
			{
				case SettingsDirector.CameraType.Practice:
					return GetCamGroupNumber( Settings.director.camerasPractice );

				case SettingsDirector.CameraType.Qualifying:
					return GetCamGroupNumber( Settings.director.camerasQualifying );

				case SettingsDirector.CameraType.Intro:
					return GetCamGroupNumber( Settings.director.camerasIntro );

				case SettingsDirector.CameraType.Inside:
					return GetCamGroupNumber( Settings.director.camerasInside );

				case SettingsDirector.CameraType.Close:
					return GetCamGroupNumber( Settings.director.camerasClose );

				case SettingsDirector.CameraType.Medium:
					return GetCamGroupNumber( Settings.director.camerasMedium );

				case SettingsDirector.CameraType.Far:
					return GetCamGroupNumber( Settings.director.camerasFar );

				case SettingsDirector.CameraType.VeryFar:
					return GetCamGroupNumber( Settings.director.camerasVeryFar );
			}

			return 0;
		}

		public static int GetCamGroupNumber( string cameraGroupNames )
		{
			if ( session != null )
			{
				var selectedCameraGroupList = cameraGroupNames.Split( "," ).ToList().Select( s => s.Trim().ToLower() ).ToList();

				var shuffledSelectedCameraGroupList = selectedCameraGroupList.OrderBy( s => Program.random.Next() ).ToList();

				foreach ( var selectedCameraGroup in shuffledSelectedCameraGroupList )
				{
					foreach ( var group in session.CameraInfo.Groups )
					{
						if ( group.GroupName.ToLower() == selectedCameraGroup )
						{
							return group.GroupNum;
						}
					}
				}
			}

			return 0;
		}

		public static string GetCamGroupName( int camGroupNumber )
		{
			if ( session != null )
			{
				foreach ( var group in session.CameraInfo.Groups )
				{
					if ( group.GroupNum == camGroupNumber )
					{
						return group.GroupName;
					}
				}
			}

			return string.Empty;
		}
	}
}
