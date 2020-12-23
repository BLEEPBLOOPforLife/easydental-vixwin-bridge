using System;
using System.Diagnostics;
using System.Linq;

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
		private const string vixWinExeLocationRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\VixWin2000";
		private const string vixWinExeLocationRegistryValueName = ""; // (Default) value name.

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
		/// Gets the name of the Easy Dental patient given a patient ID. Throws an <see cref="InvalidOperationException" /> if Easy Dental is not installed or an incompatible Easy Dental version is installed. Throws an <see cref="ArgumentException" /> if the patient ID does not represent a valid patient.
		/// </summary>
		/// <param name="patientId">The patient ID.</param>
		/// <returns>
		/// The name of the patient.
		/// </returns>
		/// <exception cref="InvalidOperationException">Easy Dental is not installed or an incompatible version of Easy Dental is installed.</exception>
		/// <exception cref="ArgumentException">Invalid patient ID.</exception>
		public static string GetEasyDentalPatientNameFromId( int patientId )
		{
			if ( patientId == 0 ) // Common case check.
			{
				throw new ArgumentException( "Invalid patient ID" );
			}

			// TODO: Check if EasyDental is installed. Get name from ID.

			throw new NotImplementedException( );
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
		/// Opens VixWin with an Easy Dental patient ID. Throws an <see cref="InvalidOperationException" /> if Easy Dental or VixWin is not installed or an incompatible version of Easy Dental or VixWin is installed. Throws an <see cref="ArgumentException" /> if the patient ID does not represent a valid patient.
		/// </summary>
		/// <param name="patientId">The Easy Dental patient ID.</param>
		/// <exception cref="InvalidOperationException">Easy Dental or VixWin is not installed or an incompatible version of Easy Dental or VixWin is installed.</exception>
		/// <exception cref="ArgumentException">Invalid patient ID.</exception>
		public static void OpenVixWinWithEasyDentalPatient( int patientId )
		{
			string vixWinExePath = GetVixWinExePath( );
			Process.Start( vixWinExePath, "-I " + patientId + " -N " + "\"" + GetEasyDentalPatientNameFromId( patientId ) + "\"" );
		}
	}
}
