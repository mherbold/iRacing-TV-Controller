
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace iRacingTVController
{
	public static class SubtitlePlayback
	{
		public const int SubtitleOverlapMergeThreshold = 5;

		public static List<SubtitleData> subtitleDataList = new();

		public static string subtitlesFilePath = string.Empty;

		public static float saveSubtitlesTimeRemaining = 0;
		public static bool saveSubtitlesQueued = false;

		public static bool subtitlesWereImported = false;

		public static SubtitleData? GetCurrentSubtitle()
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

		public static string GetSubtitlesFilePath()
		{
			return $"{Program.documentsFolder}Subtitles\\{IRSDK.normalizedSession.sessionId}-{IRSDK.normalizedSession.subSessionId}.xml";
		}

		public static void Update()
		{
			if ( IRSDK.isConnected )
			{
				if ( !subtitlesWereImported )
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

								saveSubtitlesQueued = true;
							}
						}
					}
				}

				saveSubtitlesTimeRemaining = Math.Max( 0, saveSubtitlesTimeRemaining - Program.deltaTime );

				if ( saveSubtitlesQueued && ( saveSubtitlesTimeRemaining <= 0 ) )
				{
					saveSubtitlesQueued = false;
					saveSubtitlesTimeRemaining = 1;

					SaveSubtitles();
				}
			}
			else
			{
				subtitlesFilePath = string.Empty;
				saveSubtitlesQueued = false;
				subtitlesWereImported = false;

				subtitleDataList.Clear();

				MainWindow.Instance.Subtitles_ListView.Items.Clear();
			}
		}

		public static void Clear()
		{
			subtitlesWereImported = false;

			subtitleDataList.Clear();

			MainWindow.Instance.Subtitles_ListView.Items.Clear();

			var subtitlesFilePath = GetSubtitlesFilePath();

			File.Delete( subtitlesFilePath );
		}

		public static void SaveSubtitles()
		{
			if ( subtitleDataList.Count > 0 )
			{
				subtitlesFilePath = GetSubtitlesFilePath();

				var xmlSerializer = new XmlSerializer( subtitleDataList.GetType() );

				var streamWriter = new StreamWriter( subtitlesFilePath );

				xmlSerializer.Serialize( streamWriter, subtitleDataList );

				streamWriter.Close();
			}
		}

		public static void LoadSubtitles()
		{
			var newSubtitlesFilePath = GetSubtitlesFilePath();

			if ( subtitlesFilePath != newSubtitlesFilePath )
			{
				subtitlesFilePath = newSubtitlesFilePath;
				saveSubtitlesQueued = false;
				subtitlesWereImported = false;

				subtitleDataList.Clear();

				MainWindow.Instance.Subtitles_ListView.Items.Clear();

				if ( File.Exists( subtitlesFilePath ) )
				{
					var xmlSerializer = new XmlSerializer( subtitleDataList.GetType() );

					var fileStream = new FileStream( subtitlesFilePath, FileMode.Open );

					subtitleDataList = (List<SubtitleData>) ( xmlSerializer.Deserialize( fileStream ) ?? throw new Exception() );

					fileStream.Close();

					foreach ( var subtitleData in subtitleDataList )
					{
						MainWindow.Instance.Subtitles_ListView.Items.Add( subtitleData );

						if ( subtitleData.CarIdx == 0 )
						{
							subtitlesWereImported = true;
						}
					}
				}
			}
		}

		public static bool Import()
		{
			var chatLogFilePath = $"{Program.documentsFolderSTT}ChatLogs\\{IRSDK.normalizedSession.sessionId}-{IRSDK.normalizedSession.subSessionId}.csv";

			if ( File.Exists( chatLogFilePath ) )
			{
				subtitleDataList.Clear();

				MainWindow.Instance.Subtitles_ListView.Items.Clear();

				var streamReader = File.OpenText( chatLogFilePath );
				var startFrame = 0;

				while ( true )
				{
					var line = streamReader.ReadLine();

					if ( line == null )
					{
						break;
					}

					var match = Regex.Match( line, "([^,]*),([^,]*),([^,]*),([^,]*)(,([^,]*))?(,\"([^\"]*)\")?" );

					if ( match.Success )
					{
						var sessionNumber = int.Parse( match.Groups[ 1 ].Value );
						var frameNumber = int.Parse( match.Groups[ 2 ].Value );
						var eventId = int.Parse( match.Groups[ 3 ].Value );

						if ( eventId == 5 )
						{
							if ( startFrame == 0 )
							{
								startFrame = frameNumber;
							}
						}
						else if ( eventId == 6 )
						{
							if ( startFrame > 0 )
							{
								var text = match.Groups[ 8 ].Value;

								var subtitleData = new SubtitleData()
								{
									CarIdx = 0,
									SessionNumber = sessionNumber,
									StartFrame = startFrame,
									EndFrame = frameNumber,
									Text = text,
									Ignore = false
								};

								subtitleDataList.Add( subtitleData );

								startFrame = 0;
							}
						}
					}
				}

				if ( subtitleDataList.Count > 0 )
				{
					subtitlesWereImported = true;

					foreach ( var subtitleData in subtitleDataList )
					{
						MainWindow.Instance.Subtitles_ListView.Items.Add( subtitleData );
					}
				}
				else
				{
					subtitlesWereImported = false;
				}

				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
