
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace iRacingTVController
{
	public static class SubtitlePlayback
	{
		public const int SaveToFileIntervalTime = 3;
		public const int SubtitleOverlapMergeThreshold = 5;

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
				if ( !subtitleData.Ignore )
				{
					if ( ( IRSDK.normalizedSession.sessionNumber == subtitleData.SessionNumber ) && ( IRSDK.normalizedData.replayFrameNum >= subtitleData.StartFrame ) && ( IRSDK.normalizedData.replayFrameNum <= subtitleData.EndFrame ) )
					{
						currentSubtitleData = subtitleData;
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

						int index = 0;
						int insertAt = 0;

						foreach ( var subtitleData in subtitleDataList )
						{
							if ( IRSDK.normalizedSession.sessionNumber == subtitleData.SessionNumber )
							{
								if ( ( IRSDK.normalizedData.replayFrameNum >= subtitleData.StartFrame ) && ( IRSDK.normalizedData.replayFrameNum <= subtitleData.EndFrame ) )
								{
									subtitleDataFound = true;
									break;
								}

								if ( ( subtitleData.EndFrame < IRSDK.normalizedData.replayFrameNum ) && ( ( nearestPreviousSubtitleData == null ) || ( subtitleData.EndFrame > nearestPreviousSubtitleData.EndFrame ) ) )
								{
									nearestPreviousSubtitleData = subtitleData;

									insertAt = index + 1;
								}

								if ( ( subtitleData.StartFrame > IRSDK.normalizedData.replayFrameNum ) && ( ( nearestNextSubtitleData == null ) || ( subtitleData.StartFrame < nearestNextSubtitleData.StartFrame ) ) )
								{
									nearestNextSubtitleData = subtitleData;

									insertAt = index;
								}
							}
							else
							{
								if ( IRSDK.normalizedSession.sessionNumber > subtitleData.SessionNumber )
								{
									if ( ( nearestPreviousSubtitleData == null ) && ( nearestNextSubtitleData == null ) )
									{
										insertAt = index + 1;
									}
								}
							}

							index++;
						}

						if ( !subtitleDataFound )
						{
							var addNewSubtitleData = true;

							if ( ( nearestPreviousSubtitleData != null ) && ( nearestPreviousSubtitleData.CarIdx == IRSDK.normalizedData.radioTransmitCarIdx ) && ( ( nearestPreviousSubtitleData.EndFrame + SubtitleOverlapMergeThreshold ) >= IRSDK.normalizedData.replayFrameNum ) )
							{
								addNewSubtitleData = false;

								nearestPreviousSubtitleData.EndFrame = IRSDK.normalizedData.replayFrameNum;
							}

							if ( ( nearestNextSubtitleData != null ) && ( nearestNextSubtitleData.CarIdx == IRSDK.normalizedData.radioTransmitCarIdx ) && ( ( nearestNextSubtitleData.StartFrame - SubtitleOverlapMergeThreshold ) <= IRSDK.normalizedData.replayFrameNum ) )
							{
								addNewSubtitleData = false;

								nearestNextSubtitleData.StartFrame = IRSDK.normalizedData.replayFrameNum;
							}

							if ( ( nearestPreviousSubtitleData != null ) && ( nearestNextSubtitleData != null ) && ( nearestPreviousSubtitleData.CarIdx == nearestNextSubtitleData.CarIdx ) && ( ( nearestPreviousSubtitleData.EndFrame + SubtitleOverlapMergeThreshold ) >= nearestNextSubtitleData.StartFrame ) )
							{
								nearestPreviousSubtitleData.EndFrame = nearestNextSubtitleData.EndFrame;

								subtitleDataList.Remove( nearestNextSubtitleData );

								MainWindow.Instance.Subtitles_ListView.Items.Remove( nearestNextSubtitleData );
							}

							if ( addNewSubtitleData )
							{
								var subtitleData = new SubtitleData()
								{
									CarIdx = IRSDK.normalizedData.radioTransmitCarIdx,
									SessionNumber = IRSDK.normalizedSession.sessionNumber,
									StartFrame = IRSDK.normalizedData.replayFrameNum,
									EndFrame = IRSDK.normalizedData.replayFrameNum,
									Text = string.Empty,
									Ignore = false
								};

								subtitleDataList.Insert( insertAt, subtitleData );

								MainWindow.Instance.Subtitles_ListView.Items.Insert( insertAt, subtitleData );

								MainWindow.Instance.Subtitles_ListView.ScrollIntoView( subtitleData );
							}
							else
							{
								MainWindow.Instance.Subtitles_ListView.Items.Refresh();
							}

							saveToFileQueued = true;
						}
					}
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
				filePath = string.Empty;
				saveToFileQueued = false;

				subtitleDataList.Clear();

				MainWindow.Instance.Subtitles_ListView.Items.Clear();
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

				MainWindow.Instance.Subtitles_ListView.Items.Clear();

				if ( File.Exists( filePath ) )
				{
					LogFile.Write( $"Loading subtitles from {filePath}...\r\n" );

					var xmlSerializer = new XmlSerializer( subtitleDataList.GetType() );

					var fileStream = new FileStream( filePath, FileMode.Open );

					subtitleDataList = (List<SubtitleData>) ( xmlSerializer.Deserialize( fileStream ) ?? throw new Exception() );

					fileStream.Close();

					foreach ( var subtitleData in subtitleDataList )
					{
						MainWindow.Instance.Subtitles_ListView.Items.Add( subtitleData );
					}
				}
				else
				{
					LogFile.Write( $"Subtitles file {filePath} does not exist.\r\n" );
				}
			}
		}

		public static void Clear()
		{
			subtitleDataList.Clear();

			MainWindow.Instance.Subtitles_ListView.Items.Clear();

			var subtitlesFilePath = GetFilePath();

			File.Delete( subtitlesFilePath );
		}
	}
}
