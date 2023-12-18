
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Windows.Networking;

namespace iRacingTVController
{
	public class FontPaths
	{
		public static readonly string globalFontsFolder = Environment.GetFolderPath( Environment.SpecialFolder.Fonts );
		public static readonly string localFontsFolder = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) + "\\Microsoft\\Windows\\Fonts\\";

		public static SortedDictionary<string, string> FindAll()
		{
			SortedDictionary<string, string> fontPaths = new();

			FindInFolder( globalFontsFolder, fontPaths );
			FindInFolder( localFontsFolder, fontPaths );

			return fontPaths;
		}

		public static void FindInFolder( string path, SortedDictionary<string, string> fontPaths )
		{
			LogFile.Write( $"Scanning for fonts in {path}.\r\n" );

			var fontsInDirectory = Directory.EnumerateFiles( path );

			foreach ( var fontPath in fontsInDirectory )
			{
				var typeFaces = Fonts.GetTypefaces( fontPath );

				if ( typeFaces.Count == 0 )
				{
					continue;
				}

				var typeFace = typeFaces.First();

				var fontName = typeFace.FontFamily.Source.Split( "#" ).Last();

				if ( typeFace.Stretch.ToString() != "Normal" )
				{
					fontName += $" {typeFace.Stretch}";
				}

				if ( typeFace.Style.ToString() != "Normal" )
				{
					fontName += $" {typeFace.Style}";
				}

				if ( typeFace.Weight.ToString() != "Normal" )
				{
					fontName += $" {typeFace.Weight}";
				}

				if ( !fontPaths.ContainsKey( fontName ) && !fontPaths.ContainsValue( fontPath ) )
				{
					fontPaths[ fontName ] = fontPath;
				}
			}
		}
	}
}
