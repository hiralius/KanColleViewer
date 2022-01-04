using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grabacr07.KanColleViewer.Models;

namespace Grabacr07.KanColleViewer.Plugins
{
	internal class BalloonNotifier : NotifierBase
	{
		private NotifyIcon notifyIcon;
		private EventHandler activatedAction;

		public override bool IsSupported => !Toast.IsSupported;

		protected override void InitializeCore()
		{
			const string iconUri = "pack://application:,,,/KanColleViewer;Component/Assets/app.ico";

			if (!Uri.TryCreate(iconUri, UriKind.Absolute, out Uri uri))
			{
				return;
			}

			System.Windows.Resources.StreamResourceInfo streamResourceInfo = System.Windows.Application.GetResourceStream(uri);
			if (streamResourceInfo == null)
			{
				return;
			}

			System.Windows.Application.Current.Dispatcher.Invoke(() =>
			{
				using (System.IO.Stream stream = streamResourceInfo.Stream)
				{
					notifyIcon = new NotifyIcon
					{
						Text = ProductInfo.Title,
						Icon = new Icon(stream),
						Visible = true,
					};
				}
			});
		}

		protected override void NotifyCore(string header, string body, Action activated, Action<Exception> failed)
		{
			if (notifyIcon == null)
			{
				return;
			}

			System.Windows.Application.Current.Dispatcher.Invoke(() =>
			{
				if (activated != null)
				{
					notifyIcon.BalloonTipClicked -= activatedAction;

					activatedAction = (sender, args) => activated();
					notifyIcon.BalloonTipClicked += activatedAction;
				}

				notifyIcon.ShowBalloonTip(1000, header, body, ToolTipIcon.None);
			});
		}

		public override void Dispose()
		{
			notifyIcon?.Dispose();
		}
	}
}
