using System;
using NUnit.Framework;

namespace Xamarin.Android.Tools.MavenBindingAutomator.Tests
{
	[TestFixture]
	public class RepositoryTest
	{
		[Test]
		public void FromGradleSpecifier ()
		{
			var pr1 = Repository.FromGradleSpecifier ("org.hamcrest:hamcrest-core:1.3");
			Assert.AreEqual ("org.hamcrest", pr1.GroupId, "#1.1");
			Assert.AreEqual ("hamcrest-core", pr1.ArtifactId, "#1.2");
			Assert.AreEqual ("1.3", pr1.Version, "#1.3");
			var pr2 = Repository.FromGradleSpecifier ("com.android.support:appcompat-v7:25.1.1");
			Assert.AreEqual ("com.android.support", pr2.GroupId, "#2.1");
			Assert.AreEqual ("appcompat-v7", pr2.ArtifactId, "#2.2");
			Assert.AreEqual ("25.1.1", pr2.Version, "#2.3");
		}

		[Test]
		public void IsAndroidSdkComponent ()
		{
			Assert.IsTrue (Repository.IsAndroidSdkComponent ("com.android.support"), "#1");
			Assert.IsTrue (Repository.IsAndroidSdkComponent ("com.android.databinding"), "#2");
			Assert.IsTrue (Repository.IsAndroidSdkComponent ("com.google.android.gms"), "#3");
			Assert.IsTrue (Repository.IsAndroidSdkComponent ("com.google.firebase"), "#4");
			Assert.IsTrue (Repository.IsAndroidSdkComponent ("com.google.android.support"), "#5");
			Assert.IsFalse (Repository.IsAndroidSdkComponent ("com.foo.bar"), "#6");
		}
	}
}
