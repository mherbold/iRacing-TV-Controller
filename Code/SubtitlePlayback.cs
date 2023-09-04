
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace iRacingTVController
{
	public static class SubtitlePlayback
	{
		public const int SaveToFileIntervalTime = 3;
		public const double SubtitleOverlapMergeThreshold = 0.25;

		public static List<SubtitleData> subtitleDataList = new();

		public static string filePath = string.Empty;

		public static float saveToFileTimeRemaining = 0;
		public static bool saveToFileQueued = false;

		public static string GetFilePath()
		{
			return $"{Program.documentsFolder}Subtitles\\{IRSDK.normalizedSession.sessionID}-{IRSDK.normalizedSession.subSessionID}.xml";
		}

		public static SubtitleData? GetCurrentSubtitleData()
		{
			SubtitleData? currentSubtitleData = null;

			foreach ( var subtitleData in subtitleDataList )
			{
				if ( subtitleData.SessionNumber > IRSDK.normalizedSession.sessionNumber )
				{
					break;
				}

				if ( subtitleData.SessionNumber == IRSDK.normalizedSession.sessionNumber )
				{
					if ( subtitleData.StartTime > IRSDK.normalizedData.sessionTime )
					{
						break;
					}

					if ( subtitleData.EndTime > IRSDK.normalizedData.sessionTime )
					{
						if ( !subtitleData.Ignore )
						{
							currentSubtitleData = subtitleData;
						}

						break;
					}
				}
			}

			return currentSubtitleData;
		}

		public static void Update()
		{
			if ( IRSDK.isConnected )
			{
				if ( IRSDK.normalizedData.replaySpeed == 1 )
				{
					if ( IRSDK.normalizedData.radioTransmitCarIdx != -1 )
					{
						bool subtitleDataFound = false;

						SubtitleData? nearestPreviousSubtitleData = null;
						SubtitleData? nearestNextSubtitleData = null;

						foreach ( var subtitleData in subtitleDataList )
						{
							if ( IRSDK.normalizedSession.sessionNumber == subtitleData.SessionNumber )
							{
								if ( ( IRSDK.normalizedData.sessionTime >= subtitleData.StartTime ) && ( IRSDK.normalizedData.sessionTime <= subtitleData.EndTime ) )
								{
									subtitleDataFound = true;
									break;
								}

								if ( ( subtitleData.EndTime < IRSDK.normalizedData.sessionTime ) && ( ( nearestPreviousSubtitleData == null ) || ( subtitleData.EndTime > nearestPreviousSubtitleData.EndTime ) ) )
								{
									nearestPreviousSubtitleData = subtitleData;
								}

								if ( ( subtitleData.StartTime > IRSDK.normalizedData.sessionTime ) && ( ( nearestNextSubtitleData == null ) || ( subtitleData.StartTime < nearestNextSubtitleData.StartTime ) ) )
								{
									nearestNextSubtitleData = subtitleData;
								}
							}
						}

						if ( !subtitleDataFound )
						{
							var addNewSubtitleData = true;

							if ( ( nearestPreviousSubtitleData != null ) && ( nearestPreviousSubtitleData.CarIdx == IRSDK.normalizedData.radioTransmitCarIdx ) && ( ( nearestPreviousSubtitleData.EndTime + SubtitleOverlapMergeThreshold ) >= IRSDK.normalizedData.sessionTime ) )
							{
								addNewSubtitleData = false;

								nearestPreviousSubtitleData.EndTime = IRSDK.normalizedData.sessionTime;
							}

							if ( ( nearestNextSubtitleData != null ) && ( nearestNextSubtitleData.CarIdx == IRSDK.normalizedData.radioTransmitCarIdx ) && ( ( nearestNextSubtitleData.StartTime - SubtitleOverlapMergeThreshold ) <= IRSDK.normalizedData.sessionTime ) )
							{
								addNewSubtitleData = false;

								nearestNextSubtitleData.StartTime = IRSDK.normalizedData.sessionTime;
							}

							if ( addNewSubtitleData )
							{
								var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.radioTransmitCarIdx );

								var subtitleData = new SubtitleData()
								{
									CarIdx = IRSDK.normalizedData.radioTransmitCarIdx,
									Index = 0,
									SessionNumber = IRSDK.normalizedSession.sessionNumber,
									StartTime = IRSDK.normalizedData.sessionTime,
									EndTime = IRSDK.normalizedData.sessionTime,
									CarNumber = normalizedCar?.carNumber ?? "?",
									Text = string.Empty,
									Ignore = false
								};

								subtitleDataList.Add( subtitleData );

								Refresh();

								MainWindow.Instance.Subtitles_ListView.ScrollIntoView( subtitleData );
							}
							else
							{
								if ( ( nearestPreviousSubtitleData != null ) && ( nearestNextSubtitleData != null ) && ( nearestPreviousSubtitleData.CarIdx == nearestNextSubtitleData.CarIdx ) && ( ( nearestPreviousSubtitleData.EndTime + SubtitleOverlapMergeThreshold ) >= nearestNextSubtitleData.StartTime ) )
								{
									nearestPreviousSubtitleData.EndTime = nearestNextSubtitleData.EndTime;

									subtitleDataList.Remove( nearestNextSubtitleData );
								}

								Refresh();
							}

							saveToFileQueued = true;
						}
					}
					else
					{
						SubtitleData? currentSubtitleData = null;

						var currentText = string.Empty;

						while ( true )
						{
							var detectedSpeech = SpeechToText.GetNextDetectedSpeech();

							if ( detectedSpeech == null )
							{
								break;
							}

							if ( currentSubtitleData != null )
							{
								if ( ( detectedSpeech.startSimTime.sessionNumber == currentSubtitleData.SessionNumber ) && ( detectedSpeech.startSimTime.sessionTime >= currentSubtitleData.StartTime ) && ( detectedSpeech.startSimTime.sessionTime <= currentSubtitleData.EndTime ) )
								{
									if ( currentText.Length != 0 )
									{
										currentText += " ";
									}

									currentText += detectedSpeech.recognizedString;

									continue;
								}
								else
								{
									currentSubtitleData.Text = currentText;
								}
							}

							foreach ( var subtitleData in subtitleDataList )
							{
								if ( ( detectedSpeech.startSimTime.sessionNumber == subtitleData.SessionNumber ) && ( detectedSpeech.startSimTime.sessionTime >= subtitleData.StartTime ) && ( detectedSpeech.startSimTime.sessionTime <= subtitleData.EndTime ) )
								{
									currentSubtitleData = subtitleData;

									break;
								}
							}

							currentText = detectedSpeech.recognizedString;
						}

						if ( currentText != string.Empty )
						{
							if ( currentSubtitleData != null )
							{
								currentSubtitleData.Text = currentText;

								saveToFileQueued = true;

								MainWindow.Instance.Subtitles_ListView.Items.Refresh();
							}
						}
					}
				}
				else
				{
					while ( SpeechToText.GetNextDetectedSpeech() != null ) { };
				}

				saveToFileTimeRemaining = Math.Max( 0, saveToFileTimeRemaining - Program.deltaTime );

				if ( saveToFileQueued && ( saveToFileTimeRemaining == 0 ) )
				{
					saveToFileQueued = false;
					saveToFileTimeRemaining = SaveToFileIntervalTime;

					Save();
				}
			}
			else
			{
				while ( SpeechToText.GetNextDetectedSpeech() != null ) { };

				filePath = string.Empty;
				saveToFileQueued = false;

				subtitleDataList.Clear();

				MainWindow.Instance.Subtitles_ListView.Items.Refresh();
			}
		}

		public static void Save()
		{
			if ( subtitleDataList.Count > 0 )
			{
				filePath = GetFilePath();

				var xmlSerializer = new XmlSerializer( subtitleDataList.GetType() );

				try
				{
					var streamWriter = new StreamWriter( filePath );

					xmlSerializer.Serialize( streamWriter, subtitleDataList );

					streamWriter.Close();
				}
				catch ( IOException )
				{
					saveToFileQueued = true;
					saveToFileTimeRemaining = SaveToFileIntervalTime;
				}
			}
		}

		public static void Load()
		{
			var newFilePath = GetFilePath();

			if ( filePath != newFilePath )
			{
				filePath = newFilePath;
				saveToFileQueued = false;

				subtitleDataList.Clear();

				MainWindow.Instance.Subtitles_ListView.Items.Refresh();

				if ( File.Exists( filePath ) )
				{
					LogFile.Write( $"Loading subtitles from {filePath}...\r\n" );

					var xmlSerializer = new XmlSerializer( subtitleDataList.GetType() );

					var fileStream = new FileStream( filePath, FileMode.Open );

					subtitleDataList = (List<SubtitleData>) ( xmlSerializer.Deserialize( fileStream ) ?? throw new Exception() );

					fileStream.Close();
				}
				else
				{
					LogFile.Write( $"Subtitles file {filePath} does not exist.\r\n" );
				}

				Refresh();
			}
		}

		public static void Refresh()
		{
			subtitleDataList.Sort( SubtitleDataComparison );

			var index = 1;

			foreach ( var subtitleData in subtitleDataList )
			{
				subtitleData.Index = index++;
			}

			MainWindow.Instance.Subtitles_ListView.ItemsSource = subtitleDataList;

			MainWindow.Instance.Subtitles_ListView.Items.Refresh();
		}

		public static void Clear()
		{
			var subtitlesFilePath = GetFilePath();

			File.Delete( subtitlesFilePath );

			subtitleDataList.Clear();

			Refresh();
		}

		public static Comparison<SubtitleData> SubtitleDataComparison = delegate ( SubtitleData a, SubtitleData b )
		{
			if ( a.SessionNumber == b.SessionNumber )
			{
				return a.StartTime.CompareTo( b.StartTime );
			}

			return a.SessionNumber.CompareTo( b.SessionNumber );
		};
	}
}
