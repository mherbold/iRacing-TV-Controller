
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using Aydsko.iRacingData;
using Aydsko.iRacingData.Tracks;

namespace iRacingTVController
{
	public static class DataApi
	{
		public static string username = string.Empty;
		public static string password = string.Empty;

		public static IDataClient? dataClient = null;

		public static IReadOnlyDictionary<string, TrackAssets>? trackAssetsDictionary = null;

		public static void Initialize()
		{
			if ( ( username != Settings.editor.iracingAccountUsername ) || ( password != Settings.editor.iracingAccountPassword ) )
			{
				dataClient = null;

				if ( ( Settings.editor.iracingAccountUsername != string.Empty ) && ( Settings.editor.iracingAccountPassword != string.Empty ) )
				{
					var serviceCollection = new ServiceCollection();

					serviceCollection.AddIRacingDataApi( options =>
					{
						options.UserAgentProductName = Program.AppName;
						options.UserAgentProductVersion = typeof( Program ).Assembly.GetName().Version;
					} );

					var serviceProvider = serviceCollection.BuildServiceProvider();

					dataClient = serviceProvider.GetRequiredService<IDataClient>();

					dataClient.UseUsernameAndPassword( Settings.editor.iracingAccountUsername, Settings.editor.iracingAccountPassword );
				}

				username = Settings.editor.iracingAccountUsername;
				password = Settings.editor.iracingAccountPassword;
			}
		}

		public static string? GetTrackMapLayerUrl( int trackID, string layerName )
		{
			GetTrackAssetsDictionary();

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

		public static void GetTrackAssetsDictionary()
		{
			if ( dataClient != null )
			{
				if ( trackAssetsDictionary == null )
				{
					try
					{
						trackAssetsDictionary = Task.Run( async () => await dataClient.GetTrackAssetsAsync() ).Result.Data;
					}
					catch ( Exception exception )
					{
						MessageBox.Show( MainWindow.Instance, $"Could not connect to iRacing Data API to get track assets dictionary.\r\n\r\n{exception.Message}", "iRacing Data API Error", MessageBoxButton.OK, MessageBoxImage.Information );
					}
				}
			}
		}

		public static string? DownloadTrackAsset( string url )
		{
			try
			{
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
	}
}
