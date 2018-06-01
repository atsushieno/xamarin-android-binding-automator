using NUnit.Framework;
using System;
using Xamarin.MavenClient;

namespace Xamarin.Android.Tools.MavenBindingAutomator.Tests
{
	[TestFixture]
	public class MavenBindingAutomatorTest
	{
		[Test]
		public void BuildLocalCachePath ()
		{
			var pr = new PackageReference () {
				GroupId = "com.xamarin.example",
				ArtifactId = "testproject",
				Version = "1.0.0",
			};
			var path1 = MavenDownloader.BuildLocalCachePath ("/foo/bar/baz", pr, PomComponentKind.Binary);
			Assert.AreEqual ("/foo/bar/baz/com.xamarin.example/testproject/1.0.0/testproject-1.0.0.jar", path1, "#1");
			var path2 = MavenDownloader.BuildLocalCachePath ("/foo/bar/baz", pr, PomComponentKind.JavadocJar);
			Assert.AreEqual ("/foo/bar/baz/com.xamarin.example/testproject/1.0.0/testproject-1.0.0-javadoc.jar", path2, "#2");
		}
	}
}
