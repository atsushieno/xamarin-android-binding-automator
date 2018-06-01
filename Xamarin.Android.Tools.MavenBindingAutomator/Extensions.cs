using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	internal static class Extensions
	{
		internal static void Unzip (string sourceZip, string outputDirectory)
		{
			using (var zip = Xamarin.Tools.Zip.ZipArchive.Open (sourceZip, FileMode.Open))
				zip.ExtractAll (outputDirectory);
		}

		internal static int Exec (Logger logger, string command, string arguments)
		{
			DateTime start = DateTime.Now;
			var info = new ProcessStartInfo (command, arguments);
			var proc = System.Diagnostics.Process.Start (info);
			proc.WaitForExit ();
			logger.Log (LogRecord.CommandFinishedWithinNMilliseconds, command, arguments, (proc.ExitTime - start).TotalMilliseconds);
			return proc.ExitCode;
		}

		internal static StringBuilder AppendOption (this StringBuilder cmd, params string [] options)
		{
			return cmd.Append (" ").Append (string.Concat (options));
		}
	}
}
