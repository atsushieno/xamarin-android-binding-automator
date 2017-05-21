using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public abstract class Repository
	{
		public static PackageReference FromGradleSpecifier (string spec)
		{
			var arr = spec.Split (':');
			if (arr.Length != 3)
				throw new FormatException ("The value is not a correct Gradle specifier: " + spec);
			return new PackageReference { GroupId = arr [0], ArtifactId = arr [1], Version = arr [2] };
		}

		public static bool IsAndroidSdkComponent (string groupId)
		{
			switch (groupId) {
			case "com.android.databinding":
			case "com.android.support":
			case "com.google.firebase":
			case "com.google.android.gms":
			case "com.google.android.support":
			case "com.google.android.wearable":
				return true;
			}
			return false;
		}

		public static bool IsAndroidArchitectureComponent (string groupId)
		{
			return groupId.StartsWith ("android.arch.", StringComparison.Ordinal);
		}

		public virtual bool ShouldSkipDownload (PackageReference pr)
		{
			return false;
		}

		public virtual PackageReference RetrievePomContent (PackageReference pr, MavenDownloader.Options options)
		{
			options = options ?? new MavenDownloader.Options ();
			var pomUrl = BuildDownloadUrl (pr, PomComponentKind.PomXml);
			options.LogMessage ("Downloading pom: " + pomUrl);
			var pom = XElement.Load (pomUrl);
			return PackageReference.Load (pom);
		}

		public virtual bool CanTryDownloading (PackageReference pr)
		{
			return !IsAndroidSdkComponent (pr.GroupId);
		}

		public abstract string BaseUrl { get; }

		/// <summary>
		/// constructs a download URL, or throw <see cref="T:RepositoryDownloadException" />
		/// </summary>
		/// <returns>The download URL.</returns>
		/// <param name="pkg">Package.</param>
		/// <param name="kind">Pom component kind.</param>
		public virtual string BuildDownloadUrl (PackageReference pkg, PomComponentKind kind)
		{
			return BuildDownloadUrl (BaseUrl, pkg, kind);
		}

		public static string BuildDownloadUrl (string baseUrl, PackageReference pkg, PomComponentKind kind)
		{
			return string.Concat ($"{baseUrl}{pkg.GroupId?.Replace ('.', '/')}/{pkg.ArtifactId}/{pkg.Version}/{pkg.ArtifactId}-{pkg.Version}{kind.ToFileSuffix (pkg)}");
		}

		public virtual async Task<Stream> GetStreamAsync (PackageReference pkg, PomComponentKind kind, MavenDownloader.Options options)
		{
			var pr = RetrievePomContent (pkg, options);
			var url = BuildDownloadUrl (pr, kind);
			options.LogMessage ($"Downloading {url} ...");
			return await GetStreamFromUrlAsync (url);
		}

		public virtual async Task<Stream> GetStreamFromUrlAsync (string url)
		{
			var hc = new HttpClient ();
			return await hc.GetStreamAsync (url);
		}
	}

	public class RepositoryDownloadException : Exception
	{
		public RepositoryDownloadException ()
			: base ("Failed to download from repository.")
		{
		}

		public RepositoryDownloadException (string message) : base (message)
		{
		}

		public RepositoryDownloadException (string message, Exception inner) : base (message, inner)
		{
		}

		public RepositoryDownloadException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
		}
	}

	public class LocalAndroidSdkRepository : Repository
	{
		string android_sdk;

		public LocalAndroidSdkRepository (string androidSdkPath)
		{
			if (!Directory.Exists (androidSdkPath))
				throw new ArgumentException ($"Specified Android SDK Path \"{androidSdkPath}\" does not exist.");
			this.android_sdk = androidSdkPath;
		}

		public override bool CanTryDownloading (PackageReference pr)
		{
			return IsAndroidSdkComponent (pr.GroupId);
		}

		public override Task<Stream> GetStreamAsync (PackageReference pkg, PomComponentKind kind, MavenDownloader.Options options)
		{
			string basePath = $"{android_sdk}/extras/android/m2repository/{pkg.GroupId.Replace ('.', '/')}/{pkg.ArtifactId}";
			string file = kind == PomComponentKind.PomXml ? 
			                                      Path.Combine (basePath, "maven-metadata.xml") :
			                                      Path.Combine (basePath, pkg.Version, $"{pkg.ArtifactId}-{pkg.Version}{kind.ToFileSuffix (pkg)}");
			options.LogMessage ($"Retrieving file from {file} ...");
			return Task.FromResult ((Stream) File.OpenRead (file));
		}

		public override Task<Stream> GetStreamFromUrlAsync (string url)
		{
			try {
				return Task.FromResult ((Stream)File.OpenRead (new Uri (url).LocalPath));
			} catch (IOException ex) {
				throw new RepositoryDownloadException ($"Failed to read Android SDK component \"{url}\" : {ex}");
			}
		}

		public override string BaseUrl {
			get { return $"file://{android_sdk}/extras/android/m2repository/"; }
		}

		public override string BuildDownloadUrl (PackageReference pkg, PomComponentKind kind)
		{
			string basePath = BaseUrl + $"{pkg.GroupId.Replace ('.', '/')}/{pkg.ArtifactId}";
			if (kind == PomComponentKind.PomXml)
				return $"{basePath}/maven-metadata.xml";
			else
				return $"{basePath}/{pkg.Version}/{pkg.ArtifactId}-{pkg.Version}{kind.ToFileSuffix (pkg)}";
		}
	}

	public class OfficialMavenRepository : Repository
	{
		public override string BaseUrl { get; } = "https://search.maven.org/remotecontent?filepath=";
	}

	public class JCenterRepository : Repository
	{
		public override string BaseUrl { get; } = "https://dl.bintray.com/content/bintray/jcenter/";
	}

	public class GoogleRepository : Repository
	{
		public override string BaseUrl { get; } = "https://maven.google.com/";

		public override bool CanTryDownloading (PackageReference pr)
		{
			if (IsAndroidArchitectureComponent (pr.GroupId))
				return true;
			switch (pr.GroupId) {
			case "com.android.support":
				return true;
			}
			return false;
		}

		public override bool ShouldSkipDownload (PackageReference pr)
		{
			return pr.DeclaredPackaging == null;
		}
	}
}
