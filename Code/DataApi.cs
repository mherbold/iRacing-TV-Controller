
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using Aydsko.iRacingData;
using Aydsko.iRacingData.Tracks;
using Aydsko.iRacingData.Common;

namespace iRacingTVController
{
	public static class DataApi
	{
		public static IDataClient? dataClient = null;

		public static IReadOnlyDictionary<string, TrackAssets>? trackAssetsDictionary = null;
		public static CarClass[]? carClasses = null;

		public static void Initialize( bool showMessageBoxOnSuccess )
		{
			dataClient = null;
			trackAssetsDictionary = null;
			carClasses = null;

			if ( ( Settings.editor.iracingAccountUsername != string.Empty ) && ( Settings.editor.iracingAccountPassword != string.Empty ) )
			{
				LogFile.Write( "Connecting to iRacing data API...\r\n" );

				var serviceCollection = new ServiceCollection();

				serviceCollection.AddIRacingDataApi( options =>
				{
					options.UserAgentProductName = Program.AppName;
					options.UserAgentProductVersion = typeof( Program ).Assembly.GetName().Version;
				} );

				var serviceProvider = serviceCollection.BuildServiceProvider();

				dataClient = serviceProvider.GetRequiredService<IDataClient>();

				dataClient.UseUsernameAndPassword( Settings.editor.iracingAccountUsername, Settings.editor.iracingAccountPassword );

				if ( !GetTrackAssetsDictionary() )
				{
					return;
				}

				if ( !GetCarClasses() )
				{
					return;
				}

				if ( showMessageBoxOnSuccess )
				{
					MessageBox.Show( MainWindow.Instance, $"All is good - we were able to connect to the iRacing Data API and retrieve the data we needed.", "iRacing Data API Connected Successfully!", MessageBoxButton.OK, MessageBoxImage.Information );
				}
			}
		}

		public static bool GetTrackAssetsDictionary()
		{
			if ( dataClient != null )
			{
				if ( trackAssetsDictionary == null )
				{
					try
					{
						LogFile.Write( "Fetching track assets dictionary...\r\n" );

						trackAssetsDictionary = Task.Run( async () => await dataClient.GetTrackAssetsAsync() ).Result.Data;

						return true;
					}
					catch ( Exception exception )
					{
						MessageBox.Show( MainWindow.Instance, $"Could not connect to iRacing Data API to get the track assets dictionary.\r\n\r\n{exception.Message}", "iRacing Data API Error", MessageBoxButton.OK, MessageBoxImage.Information );
					}
				}
			}

			return false;
		}

		public static bool GetCarClasses()
		{
			if ( dataClient != null )
			{
				if ( carClasses == null )
				{
					try
					{
						LogFile.Write( "Fetching car classes...\r\n" );

						carClasses = Task.Run( async () => await dataClient.GetCarClassesAsync() ).Result.Data;

						return true;
					}
					catch ( Exception exception )
					{
						MessageBox.Show( MainWindow.Instance, $"Could not connect to iRacing Data API to get the car classes.\r\n\r\n{exception.Message}", "iRacing Data API Error", MessageBoxButton.OK, MessageBoxImage.Information );
					}
				}
			}

			return false;
		}

		public static string? GetTrackMapLayerUrl( int trackID, string layerName )
		{
			if ( trackAssetsDictionary != null )
			{
				foreach ( var trackAssetsManifestData in trackAssetsDictionary )
				{
					if ( trackAssetsManifestData.Value.TrackId == trackID )
					{
						switch ( layerName.ToLower() )
						{
							case "active":
								return trackAssetsManifestData.Value.TrackMap + trackAssetsManifestData.Value.TrackMapLayers.Active;

							case "startfinish":
								return trackAssetsManifestData.Value.TrackMap + trackAssetsManifestData.Value.TrackMapLayers.StartFinish;
						}

						break;
					}
				}
			}

			return null;
		}

		public static string? DownloadTrackAsset( string url )
		{
			try
			{
				LogFile.Write( $"Downloading track asset {url}...\r\n" );

				var httpClient = new HttpClient();

				var trackAsset = Task.Run( async () => await httpClient.GetStringAsync( url ) ).Result;

				return trackAsset;
			}
			catch ( Exception exception )
			{
				MessageBox.Show( MainWindow.Instance, $"Could not get track asset.\r\n\r\n{exception.Message}", "iRacing Data API Error", MessageBoxButton.OK, MessageBoxImage.Information );

				return null;
			}
		}

		public static CarClass? GetCarClass( int classID )
		{
			if ( carClasses != null )
			{
				foreach ( var carClass in carClasses )
				{
					if ( carClass.CarClassId == classID )
					{
						return carClass;
					}
				}
			}

			return null;
		}
	}
}
