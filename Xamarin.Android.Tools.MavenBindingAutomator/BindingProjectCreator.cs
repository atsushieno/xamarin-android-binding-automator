using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.ProjectTools;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class BindingProjectCreator
	{
		public class Options
		{
			public string SolutionDirectory { get; set; }
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
			
			foreach (var g in downloads.Entries.GroupBy (e => $"{e.Package.GroupId}:{e.Package.ArtifactId}:{e.Package.Version}")) {
				var proj = new XamarinAndroidBindingProject () { ProjectName = g.Key.Replace (':', '_') };
				foreach (var d in g) {
					if (d.ComponentKind == PomComponentKind.Binary)
						proj.Jars.Add (d.Package.Packaging == "jar" ? (BuildItem)new AndroidItem.EmbeddedJar (d.LocalFile) : new AndroidItem.LibraryProjectZip (d.LocalFile));
					else if (d.ComponentKind == PomComponentKind.JavadocJar)
						proj.OtherBuildItems.Add (new BuildItem ("JavaDocJar", d.LocalFile));
				}
				var dir = Path.Combine (options.SolutionDirectory, proj.ProjectName);
				if (Directory.Exists (dir))
					Directory.Delete (dir, true);
				proj.Populate (dir);
				result.Projects.Add (proj);
			}

			return result;
		}
	}
}
