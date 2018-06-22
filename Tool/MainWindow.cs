using System;
using Xwt;
using System.Linq;
using System.IO;
using System.Runtime.Remoting.Messaging;
using Xamarin.MavenClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class MainWindow : Window
	{
		public MainState State { get; } = new MainState ();
		public Model Model { get; } = new Model ();
		public Controller Controller { get; private set; }

		public MainWindow ()
		{
			Width = 800;
			Height = 600;

			Controller = new Controller (Model, State);

			MainMenu = BuildMainMenu ();

			var layout = new VBox ();

			// package ID entry
			var packageRow = new HBox ();
			layout.PackStart (packageRow);
			packageRow.PackStart (new Label { Text = "POM" });
			var packageIdEntry = new ComboBoxEntry ();
			Model.PomHistory.Updated += (o, e) => Application.Invoke (() => {
				foreach (var entry in Model.PomHistory.Entries)
					packageIdEntry.Items.Add (entry);
			});
			Action updatePoms = () => State.PomEntry = packageIdEntry.TextEntry.Text;
			packageIdEntry.TextInput += (sender, e) => updatePoms ();
			packageIdEntry.SelectionChanged += (sender, e) => updatePoms ();

			packageRow.PackStart (packageIdEntry, true);

			// Maven download directory entry
			var downloadDirectoryRow = new HBox ();
			layout.PackStart (downloadDirectoryRow);
			downloadDirectoryRow.PackStart (new Label { Text = "Downloads" });
			var downloadDirectoryEntry = new ComboBoxEntry ();
			Model.DownloadDirectoryHistory.Updated += (o, e) => Application.Invoke (() => {
				foreach (var entry in Model.DownloadDirectoryHistory.Entries)
					downloadDirectoryEntry.Items.Add (entry);
			});
			Action updateDownload = () => State.DownloadDirectory = downloadDirectoryEntry.TextEntry.Text;
			downloadDirectoryEntry.TextInput += (sender, e) => updateDownload ();
			downloadDirectoryEntry.SelectionChanged += (sender, e) => updateDownload ();

			downloadDirectoryRow.PackStart (downloadDirectoryEntry, true);

			var downloadDirectoryPickerButton = new Button () { Label = "Choose..." };
			downloadDirectoryPickerButton.Clicked += delegate {
				var chooser = new SelectFolderDialog ("Choose directory to store downloads.") { CanCreateFolders = true };
				if (chooser.Run ())
					downloadDirectoryEntry.TextEntry.Text = chooser.Folder;
			};
			downloadDirectoryRow.PackStart (downloadDirectoryPickerButton);

			// solution directory entry
			var solutionDirectoryRow = new HBox ();
			layout.PackStart (solutionDirectoryRow);
			solutionDirectoryRow.PackStart (new Label { Text = "Projects" });
			var solutionDirectoryEntry = new ComboBoxEntry ();
			Model.SolutionDirectoryHistory.Updated += (o, e) => Application.Invoke (() => {
				foreach (var entry in Model.SolutionDirectoryHistory.Entries)
					solutionDirectoryEntry.Items.Add (entry);
			});
			Action updateSolutionDirectory = () => State.SolutionDirectory = solutionDirectoryEntry.TextEntry.Text;
			solutionDirectoryEntry.TextInput += (sender, e) => updateSolutionDirectory ();
			solutionDirectoryEntry.SelectionChanged += (sender, e) => updateSolutionDirectory ();
			solutionDirectoryRow.PackStart (solutionDirectoryEntry, true);

			var solutionDirectoryPickerButton = new Button () { Label = "Choose..." };
			solutionDirectoryPickerButton.Clicked += delegate {
				var chooser = new SelectFolderDialog ("Choose directory to create projects.") { CanCreateFolders = true };
				if (chooser.Run ())
					solutionDirectoryEntry.TextEntry.Text = chooser.Folder;
			};
			solutionDirectoryRow.PackStart (solutionDirectoryPickerButton);

			// import button
			import_button = new Button { Label = "Import" };
			import_button.Clicked += (sender, e) => {
				// switch to importing state
				import_button.Sensitive = false;
				this.Content.Cursor = CursorType.Wait; // does not seem to make much sense...

				Controller.SaveHistories ();
				Controller.PerformImport ();
			};
			layout.PackStart (import_button);

			var tree = new TreeView ();
			package_field = new DataField<string> ();
			tree_store = new TreeStore (package_field);
			tree.DataSource = tree_store;
			tree.Columns.Add ("Packages", package_field);
			layout.PackStart (tree, true);
			Model.PackageListUpdated += packages => {
				Application.Invoke (() => {
					var nav = tree_store.AddNode ();
					foreach (var package in packages)
						nav.AddChild ().SetValue (package_field, package.ToString ()).MoveToParent ();

					// restore default state
					Content.Cursor = CursorType.Arrow;
					import_button.Sensitive = true;
				});
			};

			Content = layout;

			Application.InvokeAsync (() => Controller.Start ());
		}

		Button import_button;
		TreeStore tree_store;
		DataField<string> package_field;

		Menu BuildMainMenu ()
		{
			var menu = new Menu ();

			var file = new MenuItem { Label = "_File" };
			file.SubMenu = new Menu ();
			menu.Items.Add (file);

			var fileExit = new MenuItem { Label = "E_xit" };
			fileExit.Clicked += (sender, e) => Application.Exit ();
			file.SubMenu.Items.Add (fileExit);

			return menu;
		}
	}

	public class MainState
	{
		public string PomEntry { get; set; }
		public string DownloadDirectory { get; set; }
		public string SolutionDirectory { get; set; }
	}

	public class Controller
	{
		public Controller (Model model, MainState state)
		{
			Model = model;
			State = state;
		}

		public Model Model { get; private set; }
		public MainState State { get; private set; }

		public async Task Start ()
		{
			await Task.Run (() => Model.Start ());
		}

		public async Task SaveHistories ()
		{
			await Task.Run (() => Model.SaveHistories (State.PomEntry, State.DownloadDirectory, State.SolutionDirectory));
		}

		public async Task PerformImport ()
		{
			if (State.PomEntry != null) {
				Model.Options.DownloaderOptions.Poms.Clear ();
				Model.Options.DownloaderOptions.Poms.Add (State.PomEntry);
			}
			if (State.DownloadDirectory != null)
				Model.Options.DownloaderOptions.OutputPath = State.DownloadDirectory;
			if (State.SolutionDirectory != null)
				Model.Options.ProjectCreatorOptions.SolutionDirectory = State.SolutionDirectory;
			await Task.Run (() => Model.PerformImport ());
		}
	}

	public class History
	{
		public History (string historyFile)
		{
			HistoryFile = historyFile;
		}

		public string HistoryFile { get; private set; }

		public event EventHandler Updated;
		public IList<string> Entries { get; } = new List<string> ();
		public async Task LoadAsync ()
		{
			await Task.Run (() => {
				Entries.Clear ();
				using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly ())
					if (store.FileExists (HistoryFile))
						using (var file = store.OpenFile (HistoryFile, FileMode.Open, FileAccess.Read))
							foreach (var line in new StreamReader (file).ReadToEnd ().Split ('\n'))
								Entries.Add (line.Trim ());
				Updated (this, EventArgs.Empty);
			});
		}

		public void Save (string entry)
		{
			using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly ())
			using (var file = store.OpenFile (HistoryFile, FileMode.OpenOrCreate, FileAccess.Write))
			using (var writer = new StreamWriter (file)) {
				writer.WriteLine (entry);
				foreach (string item in Entries)
					if (entry != item && !string.IsNullOrWhiteSpace (item))
						writer.WriteLine (item);
			}
		}
	}

	public class Model
	{
		public History PomHistory { get; } = new History ("pom_history.txt");
		public History DownloadDirectoryHistory { get; } = new History ("download_directory_history.txt");
		public History SolutionDirectoryHistory { get; } = new History ("solution_directory_history.txt");

		public void Start ()
		{
			PomHistory.LoadAsync ();
			DownloadDirectoryHistory.LoadAsync ();
			SolutionDirectoryHistory.LoadAsync ();
		}

		public MavenBindingAutomatorOptions Options { get; set; }

		public void SaveHistories (string packageId, string downloadDirectory, string solutionDirectory)
		{
			PomHistory.Save (packageId);
			DownloadDirectoryHistory.Save (downloadDirectory);
			SolutionDirectoryHistory.Save (solutionDirectory);
		}

		public event Action<IEnumerable<PackageReference>> PackageListUpdated;

		public void PerformImport ()
		{
			var d = new MavenDownloader ();
			var packages = d.FlattenAllPackageReferences (Options.DownloaderOptions);
			PackageListUpdated (packages);

			if (!string.IsNullOrEmpty (Options.DownloaderOptions.OutputPath)) {
				var downloaderResults = new MavenDownloader.Results ();
				downloaderResults.Downloads.BaseDirectory = Options.DownloaderOptions.OutputPath ?? Directory.GetCurrentDirectory ();
				d.Download (Options.DownloaderOptions, downloaderResults, packages);


				// create project to build
				var c = new BindingProjectCreator ();
				var cr = c.Process (Options.ProjectCreatorOptions, downloaderResults.Downloads);

				// build project
				var b = new BindingProjectBuilder ();
				b.Process (Options.ProjectBuilderOptions, cr.Projects);
			}
		}
	}
}
