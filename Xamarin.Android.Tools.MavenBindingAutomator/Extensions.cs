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
		public static string ToFileSuffix (this PomComponentKind kind, PackageReference pr)
		{
			switch (kind) {
			case PomComponentKind.PomXml: return ".pom";
			case PomComponentKind.Binary: return "." + pr.Packaging;
			case PomComponentKind.JavadocJar: return "-javadoc.jar";
			case PomComponentKind.SourcesJar: return "-sources.jar";
			}
			throw new NotSupportedException ();
		}

		static XElement GetRoot (this XElement e)
		{
			return e.Document != null ? e.Document.Root : e.Parent != null ? e.Parent.GetRoot () : e;
		}

		internal static string Value (this XElement el, string name)
		{
			var isEmptyNS = el.GetRoot ().Name.Namespace.NamespaceName.Length == 0;
			return el.Element (isEmptyNS ? XName.Get (name) : PackageReference.NS.GetName (name))?.Value;
		}

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
