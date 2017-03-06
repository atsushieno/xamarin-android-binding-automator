using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class JavaDocumentImporter
	{
		public class Options
		{
			public string XamarinSdk { get; set; }
			public string FullPathToMDoc {
				get { return Path.Combine (XamarinSdk, "lib", "mandroid", "mdoc.exe"); }
			}
			public string FullPathToJavadocToMDoc {
				get { return Path.Combine (XamarinSdk, "lib", "mandroid", "javadoc-to-mdoc.exe"); }
			}

			public IEnumerable<string> ReferenceAssemblies { get; set; }
			public string TargetAssembly { get; set; }
			public string MDocGeneratedDirectory { get; set; }
			public string OutputHtmlDirectory { get; set; }

			public string JavadocJar { get; set; }
			public string TargetJavadocIndex { get; set; }

			public Logger Logger { get; internal set; } = new Logger ();
			public string IntermediateCacheDirectory { get; internal set; } = Directory.GetCurrentDirectory ();
		}

		// batch processing for mdoc update -> javadoc-to-mdoc -> mdoc formatting -> mdoc export
		public void Process (Options options)
		{
			ExtractJavadocJars (options);
			MDocUpdate (options);
			JavadocToMDoc (options);
			MDocPrettyPrint (options);
			MDocExportMSDoc (options);
			MDocExportHtml (options);
		}

		// extract docs from *-javadoc.jar, and leave *-javadoc.jar.stamp
		public void ExtractJavadocJars (Options options)
		{
			if (options.JavadocJar == null)
				return;
			if (!File.Exists (options.JavadocJar)) {
				options.Logger.Log (LogRecord.SpecifiedJavadocJarDoesNotExist, options.JavadocJar);
				return;
			}

			string javadocsDir = Path.Combine (options.IntermediateCacheDirectory, "javadocs", Path.GetFileName (options.JavadocJar));
			string stamp = Path.GetFileName (options.JavadocJar) + ".stamp";
			options.TargetJavadocIndex = Path.Combine (javadocsDir, "index.html");

			if (File.Exists (stamp) && File.GetLastWriteTimeUtc (stamp) > File.GetLastWriteTimeUtc (options.JavadocJar)) {
				options.Logger.Log (LogRecord.SpecifiedJavadocJarIsOlderThanStamp, options.JavadocJar);
				return;
			}

			Extensions.Unzip (options.JavadocJar, javadocsDir);

			// create stamp file to prevent extraneous work in the future.
			File.Create (Path.Combine (javadocsDir, stamp));
		}

		public void MDocUpdate (Options options)
		{
			var refPaths = options.ReferenceAssemblies.Select (Path.GetDirectoryName).Distinct ();
			var cmd = new StringBuilder ();
			cmd.AppendOption ("--debug");
			cmd.AppendOption ("update");
			cmd.AppendOption ("--delete");
			cmd.AppendOption ("--lib=", EscapePath (Path.GetDirectoryName (options.TargetAssembly)));
			foreach (var rp in refPaths)
				cmd.AppendOption ("--lib=", EscapePath (rp));
			cmd.AppendOption ("--out=", EscapePath (options.MDocGeneratedDirectory));
			cmd.AppendOption (" ");
			cmd.AppendOption (EscapePath (Path.GetFullPath (options.TargetAssembly)));
			Extensions.Exec (options.Logger, options.FullPathToMDoc, cmd.ToString ());
		}

		static string EscapePath (string s)
		{
			return s.FirstOrDefault () == '"' && s.LastOrDefault () == '"' ? s : '"' + s + '"';
		}

		public void JavadocToMDoc (Options options)
		{
			var cmd = new StringBuilder ();
			cmd.AppendOption (options.TargetAssembly);
			cmd.AppendOption ("-v=2");
			cmd.AppendOption ("--out=", EscapePath (options.MDocGeneratedDirectory));
			cmd.AppendOption ("--doc-dir=", EscapePath (Path.GetDirectoryName (options.TargetJavadocIndex)));
			Extensions.Exec (options.Logger, options.FullPathToJavadocToMDoc, cmd.ToString ());
		}

		public void MDocPrettyPrint (Options options)
		{
			MDocUpdate (options);
		}

		public void MDocExportMSDoc (Options options)
		{
			if (options.TargetAssembly == null)
				throw new ArgumentException ("TargetAssembly is not specified in the argument JavadocImporter.Options");
			if (options.FullPathToMDoc == null)
				throw new ArgumentException ("Full path to mdoc.exe is not specified in the argument JavadocImporter.Options");
			if (options.MDocGeneratedDirectory == null)
				throw new ArgumentException ("MDocGeneratedDirectory is not specified in the argument JavadocImporter.Options");
			var cmd = new StringBuilder ();
			cmd.AppendOption ("--debug");
			cmd.AppendOption ("export-msxdoc");
			cmd.AppendOption ("--out=", Path.ChangeExtension (options.TargetAssembly, ".xml"));
			cmd.AppendOption (EscapePath (options.MDocGeneratedDirectory));
			Extensions.Exec (options.Logger, options.FullPathToMDoc, cmd.ToString ());
		}

		public void MDocExportHtml (Options options)
		{
			if (options.OutputHtmlDirectory == null)
				return;
			if (options.FullPathToMDoc == null)
				throw new ArgumentException ("Full path to mdoc.exe is not specified in the argument JavadocImporter.Options");
			if (options.MDocGeneratedDirectory == null)
				throw new ArgumentException ("MDocGeneratedDirectory is not specified in the argument JavadocImporter.Options");
			var cmd = new StringBuilder ();
			cmd.AppendOption ("--debug");
			cmd.AppendOption ("export-html");
			cmd.AppendOption ("--out=", EscapePath (options.OutputHtmlDirectory));
			cmd.AppendOption (EscapePath (options.MDocGeneratedDirectory));
			Extensions.Exec (options.Logger, options.FullPathToMDoc, cmd.ToString ());
		}
	}
}
