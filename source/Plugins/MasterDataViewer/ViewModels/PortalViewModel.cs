using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;

namespace Grabacr07.KanColleViewer.Plugins.ViewModels
{
	public class MasterData : NotificationObject
	{
		private string _text;

		public string Text
		{
			get { return _text; }
			set
			{
				if (_text == value) return;
				_text = value;
				RaisePropertyChanged();
			}
		}
	}


	public class PortalViewModel : ViewModel
	{
		private MasterData _Master;

		public event EventHandler<NotifyEventArgs> NotifyRequested;

		public MasterData Master { get { return _Master; } }

		public PortalViewModel(KanColleProxy proxy)
		{
			_Master = new MasterData();

			proxy.api_req_sortie_battleresult
				.TryParse()
				.Where(x => x.IsSuccess)
				.Subscribe(data => BattleResult(data));
		}

		private void BattleResult(SvData data)
		{
			string result = "";

			if (KanColleClient.Current.Homeport.Organization.Combined) return;

			// 出撃艦隊を取り出す
			var fleet = KanColleClient.Current.Homeport.Organization.Fleets.First(x => x.Value.IsInSortie).Value;

			// 戦闘後HPを表示
			for (int i = 0; i < 10; i++)
			{
				string hp = data.Request[$"api_l_value[{i}]"];
				if (hp == null)
				{
					continue;
				}
				result += $"{fleet.Ships[i].Info.Name} = {hp}\n";
			}
			result += "\n";

			for (int i = 0; i < 10; i++)
			{
				string hp = data.Request[$"api_l_value3[{i}]"];
				if (hp == null)
				{
					continue;
				}

				result += $"enemy{i + 1} = {hp}\n";
			}
			Master.Text = result;
		}


	}
}
