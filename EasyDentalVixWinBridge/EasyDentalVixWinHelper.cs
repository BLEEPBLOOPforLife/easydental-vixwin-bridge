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
		/// <returns>If Easy Dental is currently open.</returns>
		public static bool IsEasyDentalOpen( )
		{
			return Process.GetProcessesByName( easyDentalProcessName ).Any( );
		}

		/// <summary>
		/// Gets the name of the patient that is currently open in Easy Dental. Throws an <see cref="InvalidOperationException"/> if Easy Dental is not installed or an incompatible Easy Dental version is installed. If the patient's name is an empty string, no patient is selected or Easy Dental is not open.
		/// </summary>
		/// <returns>The name of the patient that is currently open in Easy Dental. If the patient's name is an empty string, no patient is selected or Easy Dental is not open.</returns>
		public static string GetEasyDentalPatientName( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		/// Gets the ID of the patient that is currently open in Easy Dental. Throws an <see cref="InvalidOperationException"/> if Easy Dental is not installed or an incompatible Easy Dental version is installed. If the patient ID is 0, no patient is selected or Easy Dental is not open.
		/// </summary>
		/// <returns>The ID of the patient that is currently open in Easy Dental. If the patient ID is 0, no patient is selected or Easy Dental is not open.</returns>
		public static int GetEasyDentalPatientID( )
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
		/// Gets the path of VixWin.exe. Throws an <see cref="InvalidOperationException"/> if VixWin is not installed or an incompatible version of VixWin is installed.
		/// </summary>
		/// <returns>The path of VixWin.exe.</returns>
		public static string GetVixWinExePath( )
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
	}
}
