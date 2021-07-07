using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Livet;


namespace Grabacr07.KanColleViewer.Plugins
{
	class SettingsViewModel : ViewModel
	{
		#region Counters 変更通知プロパティ

		private ObservableCollection<SettingsBase> _Settings;

		public ObservableCollection<SettingsBase> Settings
		{
			get { return this._Settings; }
			set
			{
				if (this._Settings != value)
				{
					this._Settings = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

	}

}
