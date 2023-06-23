
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
		public const int MinimumCameraSwitchWaitTicks = 150; // TODO change to setting
		public const int PostChatCameraSwitchWaitTicks = 60; // TODO change to setting

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

		public static int currentCameraGroupNumber = 0;
		public static int currentCameraNumber = 0;
		public static int currentCameraCarIdx = 0;

		public static readonly List<Message> messageBuffer = new();

		public static void Update()
		{
			isConnected = iRacingSdk.IsConnected();

			if ( isConnected )
			{
				data = iRacingSdk.GetSerializedData().Data;

				if ( ( session == null ) || ( iRacingSdk.Header.SessionInfoUpdate != sessionInfoUpdate ) )
				{
					session = iRacingSdk.GetSerializedSessionInfo();

					normalizedSession.SessionUpdate();

					normalizedData.SessionUpdate();

					sessionInfoUpdate = iRacingSdk.Header.SessionInfoUpdate;
				}

				if ( data.SessionNum != normalizedSession.sessionNumber )
				{
					normalizedSession.SessionChange();

					normalizedData.SessionChange();
				}

				if ( data.SessionNum >= 0 )
				{
					normalizedData.Update();
				}
			}
			else if ( wasConnected )
			{
				normalizedData.Reset();
				normalizedSession.Reset();
			}

			wasConnected = isConnected;
		}

		public static void AddMessage( BroadcastMessageTypes msg, int var1, int var2, int var3 )
		{
			messageBuffer.Add( new Message( msg, var1, var2, var3 ) );
		}

		public static void SendMessages()
		{
			cameraSwitchWaitTicksRemaining--;

			if ( ( normalizedData.camGroupNumber != currentCameraGroupNumber ) || ( normalizedData.camCameraNumber != currentCameraNumber ) || ( normalizedData.camCarIdx != currentCameraCarIdx ) )
			{
				currentCameraGroupNumber = normalizedData.camGroupNumber;
				currentCameraNumber = normalizedData.camCameraNumber;
				currentCameraCarIdx = normalizedData.camCarIdx;

				cameraSwitchWaitTicksRemaining = MinimumCameraSwitchWaitTicks;
			}

			sendMessageWaitTicksRemaining--;

			if ( sendMessageWaitTicksRemaining <= 0 )
			{
				if ( messageBuffer.Count > 0 )
				{
					var message = messageBuffer.First();

					messageBuffer.RemoveAt( 0 );

					LogFile.Write( $"Sending message to iRacing: {message.msg}, {message.var1}, {message.var2}, {message.var3}\r\n" );

					iRacingSdk.BroadcastMessage( message.msg, message.var1, message.var2, message.var3 );

					sendMessageWaitTicksRemaining = (int) Math.Ceiling( 60.0f / Settings.editor.iracingCommandRateLimit );
				}
				else
				{
					/*
					if ( Director.isEnabled )
					{
						if ( ( cameraSwitchWaitTicksRemaining <= 0 ) && ( ( currentCameraCarIdx != targetCameraCarIdx ) || ( currentCameraGroupNumber != targetCameraGroupNumber ) ) )
						{
							var normalizedCar = normalizedSession.FindNormalizedCarByCarIdx( targetCameraCarIdx );

							var carNumberRaw = normalizedCar?.carNumberRaw ?? 0;

							iRacingSdk.BroadcastMessage( BroadcastMessageTypes.CamSwitchNum, carNumberRaw, targetCameraGroupNumber, 0 );

							LogFile.Write( $"Sending message to iRacing: {BroadcastMessageTypes.CamSwitchNum}, {carNumberRaw}, {targetCameraGroupNumber}, 0\r\n" );

							sendMessageWaitTicksRemaining = Settings.data.MinimumCommandRate;
							cameraSwitchWaitTicksRemaining = MinimumCameraSwitchWaitTicks;
						}
					}
					*/
				}
			}
		}
	}
}
