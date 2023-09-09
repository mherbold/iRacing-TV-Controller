
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

using Windows.Devices.Enumeration;

namespace iRacingTVController
{
	public static class SpeechToText
	{
		public enum State
		{
			Starting,
			Started,
			Stopping,
			Stopped
		}

		public static string currentCognitiveServicesKey = string.Empty;
		public static string currentCognitiveServicesRegion = string.Empty;
		public static string currentLanguage = string.Empty;
		public static string currentAudioCaptureDeviceId = string.Empty;

		public static SpeechRecognizer? speechRecognizer = null;

		public static Mutex mutex = new();

		public static SimTime startSimTime = SimTime.zero;
		public static SimTime stopSimTime = SimTime.zero;

		public static string recognizingString = string.Empty;

		public static List<DetectedSpeech> detectedSpeechList = new();

		public static State state = State.Stopped;

		public static List<SimTime> startSimTimeList = new();
		public static List<SimTime> stopSimTimeList = new();

		public static SortedDictionary<string, string> FindAll()
		{
			LogFile.Write( "Finding all audio capture devices...\r\n" );

			SortedDictionary<string, string> audioCaptureDevices = new();

			var deviceInformationList = Task.Run( async () => await DeviceInformation.FindAllAsync( DeviceClass.AudioCapture ) ).Result;

			foreach ( var deviceInformation in deviceInformationList )
			{
				if ( !audioCaptureDevices.ContainsKey( deviceInformation.Name ) )
				{
					audioCaptureDevices.Add( deviceInformation.Name, deviceInformation.Id );

					LogFile.Write( $"...found {deviceInformation.Name}\r\n" );
				}
			}

			return audioCaptureDevices;
		}

		public static void Initialize()
		{
			if ( !Settings.editor.editorSpeechToTextEnabled )
			{
				Shutdown();
				return;
			}

			LogFile.Write( "Initializing Microsoft speech recognizer...\r\n" );

			if ( Settings.editor.editorSpeechToTextCognitiveServiceKey == string.Empty )
			{
				LogFile.Write( "...the cognitive service key has not been set!\r\n" );
				return;
			}

			if ( Settings.editor.editorSpeechToTextCognitiveServiceRegion == string.Empty )
			{
				LogFile.Write( "...the cognitive service region has not been set!\r\n" );
				return;
			}

			if ( Settings.editor.editorSpeechToTextLanguage == string.Empty )
			{
				LogFile.Write( "...the speech-to-text language has not been set!\r\n" );
				return;
			}

			if ( Settings.editor.editorSpeechToTextAudioCaptureDeviceId == string.Empty )
			{
				LogFile.Write( "...the audio capture device has not been selected!\r\n" );
				return;
			}

			if ( currentCognitiveServicesKey == Settings.editor.editorSpeechToTextCognitiveServiceKey )
			{
				if ( currentCognitiveServicesRegion == Settings.editor.editorSpeechToTextCognitiveServiceRegion )
				{
					if ( currentLanguage == Settings.editor.editorSpeechToTextLanguage )
					{
						if ( currentAudioCaptureDeviceId == Settings.editor.editorSpeechToTextAudioCaptureDeviceId )
						{
							LogFile.Write( "No changes to speech-to-text settings, keeping current audio capture device.\r\n" );
							return;
						}
					}
				}
			}

			Shutdown();

			try
			{
				var speechConfig = SpeechConfig.FromSubscription( Settings.editor.editorSpeechToTextCognitiveServiceKey, Settings.editor.editorSpeechToTextCognitiveServiceRegion );

				speechConfig.SpeechRecognitionLanguage = Settings.editor.editorSpeechToTextLanguage;

				speechConfig.SetProfanity( Settings.editor.editorSpeechToTextPotatoFilterEnabled ? ProfanityOption.Masked : ProfanityOption.Raw );

				speechConfig.SetProperty( PropertyId.Speech_LogFilename, $"{Program.documentsFolder}iRacing-TV-STT.log" );

				var match = Regex.Match( Settings.editor.editorSpeechToTextAudioCaptureDeviceId, @"({[^#]*})" );

				if ( match.Success )
				{
					var deviceId = match.Groups[ 1 ].Value;

					var audioConfig = AudioConfig.FromMicrophoneInput( deviceId );

					speechRecognizer = new SpeechRecognizer( speechConfig, audioConfig );

					speechRecognizer.SessionStarted += ( s, e ) =>
					{
						LogFile.Write( "Speech recognizer session started.\r\n" );

						state = State.Started;
					};

					speechRecognizer.SessionStopped += ( s, e ) =>
					{
						LogFile.Write( "Speech recognizer session stopped.\r\n" );

						recognizingString = string.Empty;

						state = State.Stopped;
					};

					speechRecognizer.SpeechStartDetected += ( s, e ) =>
					{
						LogFile.Write( "Speech recognizer speech start detected.\r\n" );
					};

					speechRecognizer.SpeechEndDetected += ( s, e ) =>
					{
						LogFile.Write( "Speech recognizer speech end detected.\r\n" );
					};

					speechRecognizer.Recognizing += ( s, e ) =>
					{
						LogFile.Write( $"Speech recognizer recognizing speech: {e.Result.Text}\r\n" );

						recognizingString = e.Result.Text;
					};

					speechRecognizer.Recognized += ( s, e ) =>
					{
						if ( e.Result.Reason == ResultReason.RecognizedSpeech )
						{
							LogFile.Write( $"Speech recognizer recognized speech: {e.Result.Text}\r\n" );

							recognizingString = string.Empty;

							detectedSpeechList.Add( new DetectedSpeech() { startSimTime = startSimTime, stopSimTime = stopSimTime, recognizedString = e.Result.Text } );
						}
						else
						{
							LogFile.Write( $"Speech recognizer unexpected condition: {e.Result.Reason}\r\n" );
						}
					};

					speechRecognizer.Canceled += ( s, e ) =>
					{
						LogFile.Write( $"Speech recognizer canceled: {e.Reason}, {e.ErrorCode}, {e.ErrorDetails}\r\n" );
					};

					currentCognitiveServicesKey = Settings.editor.editorSpeechToTextCognitiveServiceKey;
					currentCognitiveServicesRegion = Settings.editor.editorSpeechToTextCognitiveServiceRegion;
					currentLanguage = Settings.editor.editorSpeechToTextLanguage;
					currentAudioCaptureDeviceId = Settings.editor.editorSpeechToTextAudioCaptureDeviceId;

					LogFile.Write( "Speech recognizer initialized.\r\n" );
				}
				else
				{
					LogFile.Write( "Audio capture device could not be initialized - capture device ID seems to be in an incorrect format.\r\n" );
				}
			}
			catch ( Exception exception )
			{
				LogFile.Write( $"\r\nException thrown:\r\n\r\n{exception.Message}\r\n\r\n" );
			}
		}

		public static void Shutdown()
		{
			if ( speechRecognizer != null )
			{
				LogFile.Write( "Changes to speech-to-text settings detected. You will likely have to restart iRacing-TV controller due to a bug in Microsoft's cognitive speech services.\r\n" );

				StopAll();

				speechRecognizer.Dispose();

				speechRecognizer = null;
			}
		}

		public static void Start( int sessionNumber, double sessionTime )
		{
			if ( speechRecognizer == null )
			{
				return;
			}

			mutex.WaitOne();

			startSimTimeList.Add( new SimTime( sessionNumber, sessionTime ) );

			LogFile.Write( $"Requesting speech recognizer start. {startSimTimeList.Count}/{stopSimTimeList.Count}\r\n" );

			mutex.ReleaseMutex();
		}

		public static void Stop( int sessionNumber, double sessionTime )
		{
			if ( speechRecognizer == null )
			{
				return;
			}

			mutex.WaitOne();

			var startCount = startSimTimeList.Count + ( ( ( state == State.Starting ) || ( state == State.Started ) ) ? 1 : 0 );

			if ( stopSimTimeList.Count < startCount )
			{
				stopSimTimeList.Add( new SimTime( sessionNumber, sessionTime ) );

				LogFile.Write( $"Requesting speech recognizer stop. {startSimTimeList.Count}/{stopSimTimeList.Count}\r\n" );
			}

			mutex.ReleaseMutex();
		}

		public static void StopAll()
		{
			if ( speechRecognizer == null )
			{
				return;
			}

			mutex.WaitOne();

			startSimTimeList.Clear();
			stopSimTimeList.Clear();

			if ( ( state == State.Starting ) || ( state == State.Started ) )
			{
				LogFile.Write( "Stopping all speech recognition (StopAll).\r\n" );

				state = State.Stopping;

				var task = speechRecognizer.StopContinuousRecognitionAsync();

				mutex.ReleaseMutex();

				task.Wait();
			}
			else
			{
				mutex.ReleaseMutex();
			}
		}

		public static void Update()
		{
			if ( speechRecognizer == null )
			{
				return;
			}

			mutex.WaitOne();

			switch ( state )
			{
				case State.Started:
				{
					if ( stopSimTimeList.Count > 0 )
					{
						stopSimTime = stopSimTimeList[ 0 ];

						stopSimTimeList.RemoveAt( 0 );

						speechRecognizer.StopContinuousRecognitionAsync();

						state = State.Stopping;

						LogFile.Write( $"Stopping the speech recognizer. ({startSimTimeList.Count}/{stopSimTimeList.Count})\r\n" );
					}

					break;
				}

				case State.Stopped:
				{
					if ( startSimTimeList.Count > 0 )
					{
						startSimTime = startSimTimeList[ 0 ];

						startSimTimeList.RemoveAt( 0 );

						speechRecognizer.StartContinuousRecognitionAsync();

						state = State.Starting;

						LogFile.Write( $"Starting the speech recognizer ({startSimTimeList.Count}/{stopSimTimeList.Count}).\r\n" );
					}

					break;
				}
			}

			mutex.ReleaseMutex();
		}

		public static DetectedSpeech? GetNextDetectedSpeech()
		{
			DetectedSpeech? detectedSpeech = null;

			if ( speechRecognizer != null )
			{
				mutex.WaitOne();

				if ( state == State.Stopped )
				{
					if ( detectedSpeechList.Count > 0 )
					{
						detectedSpeech = detectedSpeechList[ 0 ];

						detectedSpeechList.RemoveAt( 0 );
					}
				}

				mutex.ReleaseMutex();
			}

			return detectedSpeech;
		}

		public static string GetRecognizingString()
		{
			mutex.WaitOne();

			var recognizingStringCopy = new string( recognizingString );

			mutex.ReleaseMutex();

			return recognizingStringCopy;
		}
	}
}
