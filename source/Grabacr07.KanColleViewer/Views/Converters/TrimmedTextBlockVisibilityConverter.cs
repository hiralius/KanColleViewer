using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Grabacr07.KanColleViewer.Views.Converters
{
	public class TrimmedTextBlockVisibilityConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
			{
				return Visibility.Collapsed;
			}

			FrameworkElement textBlock = (FrameworkElement)value;

			textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

			return ((FrameworkElement)value).ActualWidth < ((FrameworkElement)value).DesiredSize.Width
				? Visibility.Visible
				: (object)Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
