using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;

namespace Grabacr07.KanColleViewer.Plugins
{
	[Export(typeof(IPlugin))]
	[Export(typeof(ITool))]
	[ExportMetadata("Guid", "6EDE38C8-412D-4A73-8FE3-A9D20EB9F0D2")]
	[ExportMetadata("Title", "WindowsNotifier")]
	[ExportMetadata("Description", "Windows OS の機能 (トースト通知・バルーン通知) を使用して通知します。")]
	[ExportMetadata("Version", "2.1")]
	[ExportMetadata("Author", "@Grabacr07")]
	public class WindowsNotifier : IPlugin, ITool
	{
		/// <summary>
		/// このプラグインがあるフォルダに
		/// \アセンブリ名.xaml
		/// を繋げたもの（デフォルトはこれ）
		/// </summary>
		private static string FileName => "NotifySettings.txt";
		private static string FilePath { get; } = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
			 FileName);

		private SettingsViewModel viewModel;
		public Settings Settings;

		string ITool.Name => "WindowsNotifier";

		object ITool.View => new SettingsView { DataContext = this.viewModel, };

		public void Initialize()
		{
			this.Settings = new Settings(FilePath);
			Toast.Collections = Settings.Collections;

			this.viewModel = new SettingsViewModel
			{
				Settings = Settings.Collections
			};
		}
	}
}
