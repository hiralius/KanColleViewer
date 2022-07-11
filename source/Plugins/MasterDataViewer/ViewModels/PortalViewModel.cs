using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
		private string _info;

		public string Info
		{
			get { return _info; }
			set
			{
				if (_info == value) return;
				_info = value;
				RaisePropertyChanged();
			}
		}

		/// <summary>
		/// 海域名
		/// </summary>
		private string _area;
		public string Area
		{
			get { return _area; }
			set
			{
				if (_area == value) return;
				_area = value;
				RaisePropertyChanged();
			}
		}

		/// <summary>
		/// 海域番号　
		/// </summary>
		private string _mapid;
		public string MapID
		{
			get { return _mapid; }
			set
			{
				if (_mapid == value) return;
				_mapid = value;
				RaisePropertyChanged();
			}
		}

		/// <summary>
		/// 進撃位置
		/// </summary>
		private string _cellid;
		public string CellID
		{
			get { return _cellid; }
			set 
			{
				if (_cellid == value) return;
				_cellid = value;
				RaisePropertyChanged();
			}
		}

		public bool OverAlphabet { get; set; }

		public string[] enemies;

		public MasterData()
		{
			enemies = new string[20];
		}

	}


	public class PortalViewModel : ViewModel
	{
		//public event EventHandler<NotifyEventArgs> NotifyRequested;

		public MasterData Master { get; }

		public PortalViewModel(KanColleProxy proxy)
		{
			Master = new MasterData();

			_ = proxy.api_req_map_start
				.TryParse<kcsapi_map_start>()
				.Where(x => x.IsSuccess)
				.Subscribe(data =>
				{
					var result = data.Data;

					var area = KanColleClient.Current.Master.MapAreas[result.api_maparea_id];
					var map = KanColleClient.Current.Master.MapInfos[(result.api_maparea_id * 10) + result.api_mapinfo_no];
					Master.Area = $"{area.Name} {map.Name}";
					Master.MapID = $"{result.api_maparea_id}-{result.api_mapinfo_no}";
					Master.CellID = $"{result.api_no}";

					Master.OverAlphabet = (result.api_cell_data.Length > 25);
					ChangeCellID(result.api_no);
				});
			_ = proxy.api_req_map_next
				.TryParse<kcsapi_map_next>()
				.Where(x => x.IsSuccess)
				.Subscribe(data =>
				{
					ChangeCellID(data.Data.api_no);
				});


			_ = proxy.api_req_sortie_battleresult
				.TryParse<kcsapi_battleresult>()
				.Where(x => x.IsSuccess)
				.Subscribe(data => BattleResult(data.Request));

			_ = proxy.api_req_combined_battle_battleresult
				.TryParse<kcsapi_combined_battle_battleresult>()
				.Where(x => x.IsSuccess)
				.Subscribe(data =>
				{
					BattleResult(data.Request);
				});
		}

		private void ChangeCellID(int id)
		{
			if (Master.OverAlphabet)
			{
				Master.CellID = $"{id}";
			}
			else
			{
				Master.CellID = $"\'{(char)('@' + id)}\'";
			}
		}
		private void BattleResult(NameValueCollection request)
		{
			string result = "";

			//foreach (var key in request.AllKeys)
			//{
			//	result += $"{key} = {request[key]}\n";
			//}
			//result += "\n";

			// 出撃艦隊を取り出す
			var fleets = KanColleClient.Current.Homeport.Organization.Fleets.Where(x => x.Value.IsInSortie).Select(x => x.Value).ToArray();

			// 戦闘後HPを表示
			if (fleets.Length > 1)
			{
				result = "主力艦隊\n";
			}
			else
			{
				result = $"{fleets[0].Name}\n";
			}

			for (int i = 0; i < 10; i++)
			{
				string hp = request[$"api_l_value[{i}]"];
				if (hp == null)
				{
					continue;
				}
				result += $"{fleets[0].Ships[i].Info.Name} = {hp}\n";
			}
			result += "\n";

			if (fleets.Length > 1)
			{
				result += "随伴艦隊\n";
				for (int i = 0; i < 10; i++)
				{
					string hp = request[$"api_l_value2[{i}]"];
					if (hp == null)
					{
						continue;
					}
					result += $"{fleets[1].Ships[i].Info.Name} = {hp}\n";
				}
				result += "\n";
			}

			result += "敵艦隊\n";
			for (int i = 0; i < 10; i++)
			{
				string hp = request[$"api_l_value3[{i}]"];
				if (hp == null)
				{
					continue;
				}

				result += $"enemy{i + 1} = {hp}\n";
			}
			
			Master.Info = result;
		}


	}
}
