using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Win32;

namespace EasyDentalVixWinBridge
{
	/// <summary>
	/// The class that interfaces with Easy Dental and VixWin.
	/// </summary>
	public static class EasyDentalVixWinHelper
	{
		private const string easyDentalProcessName = "Ezd.Shell";
		private const string patientIdRegistryKeyPath = @"SOFTWARE\Easy Dental Systems, Inc.\Easy Dental\SELPAT";
		private const string patientIdRegistryValueName = "CPID";
		private const string odbcConnectionString = "DSN=EZD2011;DBQ=.;SERVER=NotTheServer";
		private const string selectPatientByIdQueryString = "\f_I@ICT p.PatID, p.LastName, p.FirstName, p.MI FROM PAT_DAT p WHERE p.PatID="; // \f_I@ICT = SELECT
		private const string vixWinExeLocationRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\VixWin2000";
		private const string vixWinExeLocationRegistryValueName = ""; // (Default) value name.
		private static int currentPatientId;
		private static Process currentVixWinInstance;

		/// <summary>
		/// Checks if Easy Dental is currently open.
		/// </summary>
		/// <returns>
		/// If Easy Dental is currently open.
		/// </returns>
		public static bool IsEasyDentalOpen( )
		{
			return Process.GetProcessesByName( easyDentalProcessName ).Any( );
		}

		/// <summary>
		/// Gets the ID of the patient that is currently open in Easy Dental. Throws an <see cref="InvalidOperationException" /> if Easy Dental is not installed or an incompatible Easy Dental version is installed. If the patient ID is 0, no patient is selected or Easy Dental is not open.
		/// </summary>
		/// <returns>
		/// The ID of the patient that is currently open in Easy Dental. If the patient ID is 0, no patient is selected or Easy Dental is not open.
		/// </returns>
		/// <exception cref="InvalidOperationException">Easy Dental is not installed or an incompatible version of Easy Dental is installed.</exception>
		public static int GetEasyDentalSelectedPatientId( )
		{
			RegistryKey patientIdRegistryKey = Registry.CurrentUser.OpenSubKey( patientIdRegistryKeyPath );

			if ( patientIdRegistryKey == null )
			{
				throw new InvalidOperationException( "Easy Dental is not installed or an incompatible version of Easy Dental is installed." );
			}

			if ( !IsEasyDentalOpen( ) )
			{
				patientIdRegistryKey.Close( );

				return 0;
			}

			int patientId = ( int ) patientIdRegistryKey.GetValue( patientIdRegistryValueName );
			patientIdRegistryKey.Close( );

			return patientId;
		}

		/// <summary>
		/// Gets the name of the Easy Dental patient given a patient ID. Throws an <see cref="InvalidOperationException" /> if Easy Dental is not installed, an incompatible version of Easy Dental is installed, or the database is down. Throws an <see cref="ArgumentException" /> if the patient ID does not represent a valid patient.
		/// </summary>
		/// <param name="patientId">The patient ID.</param>
		/// <returns>
		/// The name of the patient.
		/// </returns>
		/// <exception cref="InvalidOperationException">Easy Dental is not installed, an incompatible version of Easy Dental is installed, or the database is down.</exception>
		/// <exception cref="ArgumentException">Invalid patient ID.</exception>
		public static async Task<string> GetEasyDentalPatientNameFromIdAsync( int patientId )
		{
			if ( patientId == 0 ) // Common case check.
			{
				throw new ArgumentException( "Invalid patient ID" );
			}

			try
			{
				using ( OdbcConnection testConnection = new OdbcConnection( odbcConnectionString ) )
				{
					OdbcCommand command = new OdbcCommand( selectPatientByIdQueryString + patientId, testConnection );
					await testConnection.OpenAsync( );

					using ( DbDataReader dataReader = await command.ExecuteReaderAsync( ) )
					{
						await dataReader.ReadAsync( );

						if ( !dataReader.HasRows )
						{
							throw new ArgumentException( "Invalid patient ID" );
						}

						string lastName = ( string ) dataReader[ 1 ];
						string firstName = ( string ) dataReader[ 2 ];
						string middleInitial = ( string ) ( dataReader[ 3 ] == DBNull.Value ? "" : dataReader[ 3 ] );
						string fullName = lastName + ", " + firstName + " " + middleInitial;
						fullName = fullName.Trim( ); // Middle initial is optional, so trim away last space if there is no middle initial.

						return fullName;
					}
				}
			} catch ( Exception )
			{
				throw new InvalidOperationException( "Easy Dental is not installed, an incompatible version of Easy Dental is installed, or the database is down" );
			}
		}

		/// <summary>
		/// Gets the path of VixWin.exe. Throws an <see cref="InvalidOperationException" /> if VixWin is not installed or an incompatible version of VixWin is installed.
		/// </summary>
		/// <returns>
		/// The path of VixWin.exe.
		/// </returns>
		/// <exception cref="InvalidOperationException">VixWin is not installed or an incompatible version of VixWin is installed.</exception>
		private static string GetVixWinExePath( )
		{
			RegistryKey vixWinExeLocationRegistryKey = Registry.LocalMachine.OpenSubKey( vixWinExeLocationRegistryKeyPath );

			if ( vixWinExeLocationRegistryKey == null )
			{
				throw new InvalidOperationException( "VixWin is not installed or an incompatible version of VixWin is installed." );
			}

			string vixWinExePath = ( string ) vixWinExeLocationRegistryKey.GetValue( vixWinExeLocationRegistryValueName );
			vixWinExeLocationRegistryKey.Close( );

			return vixWinExePath;
		}

		/// <summary>
		/// Starts a watchdog that checks if the patient selection was changed in Easy Dental or if Easy Dental was closed.
		/// </summary>
		private static async void StartPatientChangeWatchdog( )
		{
			while ( true )
			{
				await Task.Delay( 1500 );

				if ( currentVixWinInstance.HasExited )
				{
					return;
				}

				if ( !IsEasyDentalOpen( ) )
				{
					while ( !currentVixWinInstance.CloseMainWindow( ) ) // Keep trying to close VixWin until it closes.
					{
						await Task.Delay( 1500 );
					}

					return;
				}

				int newPatientId = GetEasyDentalSelectedPatientId( );

				if ( newPatientId == 0 )
				{
					while ( !currentVixWinInstance.CloseMainWindow( ) ) // Keep trying to close VixWin until it closes.
					{
						await Task.Delay( 1500 );
					}

					return;
				}

				if ( currentPatientId != newPatientId )
				{
					OpenVixWinWithEasyDentalPatient( newPatientId );
				}
			}
		}

		/// <summary>
		/// Opens VixWin with an Easy Dental patient ID. Throws an <see cref="InvalidOperationException" /> if Easy Dental or VixWin is not installed, an incompatible version of Easy Dental or VixWin is installed, or the Easy Dental database is down. Throws an <see cref="ArgumentException" /> if the patient ID does not represent a valid patient.
		/// </summary>
		/// <param name="patientId">The Easy Dental patient ID.</param>
		/// <exception cref="InvalidOperationException">Easy Dental or VixWin is not installed, an incompatible version of Easy Dental or VixWin is installed, or the Easy Dental database is down.</exception>
		/// <exception cref="ArgumentException">Invalid patient ID.</exception>
		public static async void OpenVixWinWithEasyDentalPatient( int patientId )
		{
			string vixWinExePath = GetVixWinExePath( );
			Process vixWin = Process.Start( vixWinExePath, "-I " + patientId + " -N " + "\"" + await GetEasyDentalPatientNameFromIdAsync( patientId ) + "\"" );

			if ( vixWin == null || vixWin.HasExited )
			{
				return;
			}

			currentPatientId = patientId;
			currentVixWinInstance = vixWin;
			StartPatientChangeWatchdog( );
		}
	}
}
