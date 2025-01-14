using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleViewer.ViewModels.Composition;

namespace Grabacr07.KanColleViewer.ViewModels.Contents
{
	public class ToolsViewModel : TabItemViewModel
	{
		public override string Name
		{
			get { return "ツール"; }
			protected set { throw new NotImplementedException(); }
		}

		#region Items 変更通知プロパティ

		private List<ToolViewModel> _Tools;

		public List<ToolViewModel> Tools
		{
			get { return this._Tools; }
			set
			{
				if (this._Tools != value)
				{
					this._Tools = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region SelectedTool 変更通知プロパティ

		private ToolViewModel _SelectedTool;

		public ToolViewModel SelectedTool
		{
			get { return this._SelectedTool; }
			set
			{
				if (this._SelectedTool != value)
				{
					this._SelectedTool = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region TabVisibility 変更通知プロパティ

		public Visibility _TabVisibility = Visibility.Visible;

		public Visibility TabVisibility
		{
			get { return this._TabVisibility; }
			set
			{
				if (this._TabVisibility != value)
				{
					this._TabVisibility = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public void TabDoubleClick()
		{
			this.TabVisibility = Visibility.Collapsed;
		}

		public ToolsViewModel()
		{
			this.Tools = new List<ToolViewModel>(PluginService.Current.Get<ITool>().Select(x => new ToolViewModel(x)));
			this.SelectedTool = this.Tools.FirstOrDefault();
		}
	}
}
