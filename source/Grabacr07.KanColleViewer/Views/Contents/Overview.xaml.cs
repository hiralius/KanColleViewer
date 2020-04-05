using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Grabacr07.KanColleViewer.Views.Contents
{
	/// <summary>
	/// Overview.xaml の相互作用ロジック
	/// </summary>
	public partial class Overview
	{
		public Overview()
		{
			InitializeComponent();
		}

		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var sv = sender as Control;

			e.Handled = true;
			var newEventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
			{
				RoutedEvent = UIElement.MouseWheelEvent,
				Source = sender
			};
			((UIElement)sv.Parent).RaiseEvent(newEventArg);
		}
	}
}
