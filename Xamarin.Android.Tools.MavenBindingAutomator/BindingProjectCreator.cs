using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.ProjectTools;
using Xamarin.MavenClient;
using System.Xml.Linq;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class BindingProjectCreator
	{
		public class Options
		{
			public string SolutionDirectory { get; set; }
			public TextWriter LogWriter { get; set; } = Console.Out;

			public void LogMessage (string format, params object [] args)
			{
				LogWriter.WriteLine (format, args);
			}
		}

		public class Result
		{
			public IList<XamarinAndroidBindingProject> Projects { get; private set; } = new List<XamarinAndroidBindingProject> ();
		}

		public Result Process (Options options, LocalMavenDownloads downloads)
		{
			var result = new Result ();

			if (options.SolutionDirectory == null)
				throw new ArgumentException ("Project generation target directory is not set in the project creator options.");
			if (!Directory.Exists (options.SolutionDirectory))
				Directory.CreateDirectory (options.SolutionDirectory);
			string pathPrefix = Path.GetDirectoryName (options.SolutionDirectory) == Path.GetDirectoryName (downloads.BaseDirectory) ? Path.Combine ("..", "..") : downloads.BaseDirectory;

			Func<string, string> pathToFileInProj = local => Path.Combine (pathPrefix, local); 

			foreach (var g in downloads.Entries.GroupBy (e => $"{e.Package.GroupId}:{e.Package.ArtifactId}:{e.Package.Version}")) {
				var proj = new XamarinAndroidBindingProject () { ProjectName = g.Key.Replace (':', '_'), AndroidClassParser = "class-parse" };
				var dir = Path.Combine (options.SolutionDirectory, proj.ProjectName);
				foreach (var d in g) {
					string fp = Path.Combine (dir, d.LocalFile);
					if (!File.Exists (fp)) {
						options.LogMessage ($"Local download file \"{fp}\" does not exist.");
						continue;
					}
					string file = pathToFileInProj (d.LocalFile);
					if (d.ComponentKind == PomComponentKind.Binary)
						proj.Jars.Add (d.Package.Packaging == "jar" ? (BuildItem)new AndroidItem.EmbeddedJar (file) : new AndroidItem.LibraryProjectZip (file));
					else if (d.ComponentKind == PomComponentKind.JavadocJar)
						proj.OtherBuildItems.Add (new BuildItem ("JavaDocJar", file));
					else if (d.ComponentKind == PomComponentKind.PomXml) {
						foreach (PackageReference dep in PackageReference.Load (XElement.Load (file)).Dependencies) {
							var depName = dep.ToString ().Replace (':', '_');
							proj.OtherBuildItems.Add (new BuildItem ("ProjectReference", Path.Combine ("..", depName, depName + ".csproj")));
						}
					}
				}
				if (Directory.Exists (dir))
					Directory.Delete (dir, true);
				proj.Populate (dir);
				result.Projects.Add (proj);
			}

			return result;
		}
	}
}
