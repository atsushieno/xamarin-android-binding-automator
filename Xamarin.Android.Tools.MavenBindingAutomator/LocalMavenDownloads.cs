using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class LocalMavenDownloads
	{
		public string BaseDirectory { get; set; }
		
		public IList<Entry> Entries { get; private set; } = new List<Entry> ();

		public class Entry
		{
			public Entry (PackageReference package, PomComponentKind kind, string localFile)
			{
				Package = package;
				ComponentKind = kind;
				LocalFile = localFile;
			}
			public PackageReference Package { get; set; }
			public PomComponentKind ComponentKind { get; set; }
			public string LocalFile { get; set; }
		}
	}
}
