
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using Aydsko.iRacingData;
using Aydsko.iRacingData.Cars;
using Aydsko.iRacingData.Common;
using Aydsko.iRacingData.Member;
using Aydsko.iRacingData.Tracks;

namespace iRacingTVController
{
	public static class DataApi
	{
		public static IDataClient? dataClient = null;

		public static IReadOnlyDictionary<string, TrackAssets>? trackAssetsDictionary = null;
		public static CarClass[]? carClasses = null;
		public static IReadOnlyDictionary<string, CarAssetDetail>? carAssetDetailsDictionary = null;

		public static int? totalRateLimit = null;
		public static int? rateLimitRemaining = null;
		public static DateTimeOffset? rateLimitReset = null;

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

				if ( !GetCarAssetDetails() )
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

						var task = dataClient.GetTrackAssetsAsync();

						task.ConfigureAwait( false );
						task.Wait();

						var dataResponse = task.Result;

						LogFile.Write( "Track assets dictionary fetched!\r\n" );

						totalRateLimit = dataResponse.TotalRateLimit;
						rateLimitRemaining = dataResponse.RateLimitRemaining;
						rateLimitReset = dataResponse.RateLimitReset;

						trackAssetsDictionary = dataResponse.Data;

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

						var task = dataClient.GetCarClassesAsync();

						task.ConfigureAwait( false );
						task.Wait();

						var dataResponse = task.Result;

						LogFile.Write( "Car classes fetched!\r\n" );

						totalRateLimit = dataResponse.TotalRateLimit;
						rateLimitRemaining = dataResponse.RateLimitRemaining;
						rateLimitReset = dataResponse.RateLimitReset;

						carClasses = dataResponse.Data;

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

		public static bool GetCarAssetDetails()
		{
			if ( dataClient != null )
			{
				if ( carAssetDetailsDictionary == null )
				{
					try
					{
						LogFile.Write( "Fetching car asset details...\r\n" );

						var task = dataClient.GetCarAssetDetailsAsync();

						task.ConfigureAwait( false );
						task.Wait();

						var dataResponse = task.Result;

						LogFile.Write( "Car asset details fetched!\r\n" );

						totalRateLimit = dataResponse.TotalRateLimit;
						rateLimitRemaining = dataResponse.RateLimitRemaining;
						rateLimitReset = dataResponse.RateLimitReset;

						carAssetDetailsDictionary = dataResponse.Data;

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

				var task = httpClient.GetStringAsync( url );

				task.ConfigureAwait( false );
				task.Wait();

				var trackAsset = task.Result;

				LogFile.Write( $"Track asset {url} downloaded!\r\n" );

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

		public static string GetCarLogoUrl( string carID )
		{
			if ( carAssetDetailsDictionary != null )
			{
				var carAssetDetails = carAssetDetailsDictionary[ carID ];

				if ( carAssetDetails != null )
				{
					return $"{CarAssetDetail.ImagePathBase}{carAssetDetails.Logo}";
				}
			}

			return string.Empty;
		}

		public static string GetTrackLogoUrl( string trackID )
		{
			if ( trackAssetsDictionary != null )
			{
				var trackAssets = trackAssetsDictionary[ trackID ];

				if ( trackAssets != null )
				{
					return $"{TrackAssets.ImagePathBase}{trackAssets.Logo}";
				}
			}

			return string.Empty;
		}

		public static async Task<MemberProfile?> GetMemberProfileAsync( int customerID )
		{
			MemberProfile? memberProfile = null;

			if ( dataClient != null )
			{
				Stall();

				try
				{
					LogFile.Write( $"Fetching member profile for customer ID {customerID}...\r\n" );

					var cts = new CancellationTokenSource();

					cts.CancelAfter( 2000 );

					var dataResponse = await dataClient.GetMemberProfileAsync( customerID, cts.Token );

					LogFile.Write( $"Member profile for customer ID {customerID} fetched!\r\n" );

					totalRateLimit = dataResponse.TotalRateLimit;
					rateLimitRemaining = dataResponse.RateLimitRemaining;
					rateLimitReset = dataResponse.RateLimitReset;

					LogFile.Write( $"totalRateLimit = {totalRateLimit}, rateLimitRemaining = {rateLimitRemaining}, rateLimitReset = {rateLimitReset}\r\n" );

					memberProfile = dataResponse.Data;
				}
				catch ( Exception exception )
				{
					LogFile.Write( $"Exception thrown - {exception.Message}\r\n" );
				}
			}

			return memberProfile;
		}

		public static void Stall()
		{
			if ( rateLimitRemaining <= 5 )
			{
				var timeSpan = rateLimitReset - DateTime.UtcNow;

				if ( timeSpan != null )
				{
					var milliseconds = timeSpan.Value.TotalMilliseconds;

					if ( milliseconds > 0 )
					{
						Thread.Sleep( (int) timeSpan.Value.TotalMilliseconds );
					}
				}
			}
		}
	}
}
