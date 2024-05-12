
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Windows.Devices.Enumeration;

using NAudio.CoreAudioApi;

namespace iRacingTVController
{
	public static class PushToTalk
	{
		public static string currentAudioRenderDeviceId = string.Empty;

		private static MMDevice? audioRenderDevice = null;

		public static SortedDictionary<string, string> FindAll()
		{
			LogFile.Write( "Finding all audio render devices...\r\n" );

			SortedDictionary<string, string> audioRenderDevices = new();

			var deviceInformationList = Task.Run( async () => await DeviceInformation.FindAllAsync( DeviceClass.AudioRender ) ).Result;

			foreach ( var deviceInformation in deviceInformationList )
			{
				if ( !audioRenderDevices.ContainsKey( deviceInformation.Name ) )
				{
					audioRenderDevices.Add( deviceInformation.Name, deviceInformation.Id );

					LogFile.Write( $"...found {deviceInformation.Name}\r\n" );
				}
			}

			return audioRenderDevices;
		}

		public static void Initialize()
		{
			if ( !Settings.editor.editorPushToTalkMuteEnabled )
			{
				Shutdown();
				return;
			}

			LogFile.Write( "Initializing audio render device (to mute during push-to-talk)...\r\n" );

			if ( currentAudioRenderDeviceId == Settings.editor.editorPushToTalkAudioRenderDeviceId )
			{
				LogFile.Write( "No changes to push-to-talk settings, keeping current audio render device.\r\n" );
				return;
			}

			Shutdown();

			var match = Regex.Match( Settings.editor.editorPushToTalkAudioRenderDeviceId, @"({[^#]*})" );

			if ( match.Success )
			{
				var deviceId = match.Groups[ 1 ].Value;

				var deviceEnumerator = new MMDeviceEnumerator();

				try
				{
					audioRenderDevice = deviceEnumerator.GetDevice( deviceId );

					currentAudioRenderDeviceId = Settings.editor.editorPushToTalkAudioRenderDeviceId;

					LogFile.Write( "Audio render device initialized.\r\n" );
				}
				catch ( Exception )
				{
					LogFile.Write( "Previously used audio render device could not be found.\r\n" );
				}
			}
			else
			{
				LogFile.Write( "Audio render device could not be initialized - capture device ID seems to be in an incorrect format.\r\n" );
			}
		}

		public static void Shutdown()
		{
			audioRenderDevice?.Dispose();

			audioRenderDevice = null;
		}

		public static void Update()
		{
			if ( audioRenderDevice != null )
			{
				if ( IRSDK.normalizedData.isTalking )
				{
					if ( !audioRenderDevice.AudioEndpointVolume.Mute )
					{
						LogFile.Write( "Muting audio render device.\r\n" );

						audioRenderDevice.AudioEndpointVolume.Mute = true;
					}
				}
				else
				{
					if ( audioRenderDevice.AudioEndpointVolume.Mute )
					{
						LogFile.Write( "Un-muting audio render device.\r\n" );

						audioRenderDevice.AudioEndpointVolume.Mute = false;
					}
				}
			}
		}
	}
}
