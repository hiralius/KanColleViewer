using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;
using utility;

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

		/// <summary>
		/// 地図画像
		/// </summary>
		private string _mapImage;
		public string MapImage
		{
			get { return _mapImage; }
			set
			{
				if (_mapImage == value) return;
				_mapImage = value;
				RaisePropertyChanged();
			}
		}

		public string[] Enemies { get; private set; }

		public Dictionary<int, string> CellInfo { get; private set; }

		public MasterData()
		{
			Enemies = new string[20];
			CellInfo = new Dictionary<int, string>();
		}

	}


	public class PortalViewModel : ViewModel
	{
		//public event EventHandler<NotifyEventArgs> NotifyRequested;

		public MasterData Master { get; }

		private static readonly string[] CellEvents =
		{
			"初期位置",
			"イベントなし",
			"資源獲得",
			"渦潮",
			"通常戦闘",
			"ボス戦闘",
			"気のせいだった",
			"航空戦／航空偵察",
			"船団護衛成功",
			"揚陸地点",
			"泊地"
		};

		private static readonly string[] CellKinds =
		{
			"非戦闘セル",
			"通常戦闘",
			"夜戦",
			"夜昼戦",
			"航空戦",
			"敵連合艦隊戦",
			"長距離空襲戦",
			"夜昼戦(対連合艦隊)",
			"レーダー射撃",
			"",

			// event_id = 1 or 6
			"気のせいだった",
			"敵影を見ず",
			"能動分岐",
			"穏やかな海です",
			"穏やかな海峡です",
			"警戒が必要です",
			"静かな海です",
			"",
			"",
			"",

			// event_id = 7
			"航空偵察",
			"",
			"",
			"",
			"航空戦"
		};

		/// <summary>
		/// このプラグインがあるフォルダに
		/// \アセンブリ名.xaml
		/// を繋げたもの（デフォルトはこれ）
		/// </summary>
		private static string ConfigFileName => "AreaMapInfo.ini";
		private static string ConfigFilePath { get; } = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
			 ConfigFileName);

		private static string MapFolder { get; } = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Maps");

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
					Master.Info = "";
					Master.MapImage = $"{MapFolder}\\{Master.MapID}.jpg";

					// 海域情報を作る
					ReadMapInfo(Master.MapID, result.api_cell_data.Length);

					ChangeCellID(result.api_no, result.api_event_id, result.api_event_kind);
				});
			_ = proxy.api_req_map_next
				.TryParse<kcsapi_map_next>()
				.Where(x => x.IsSuccess)
				.Subscribe(data =>
				{
					ChangeCellID(data.Data.api_no, data.Data.api_event_id, data.Data.api_event_kind);
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

		/// <summary>
		/// 海域情報取得
		/// </summary>
		/// <param name="area"></param>
		private void ReadMapInfo(string area, int cellMax)
		{
			Master.CellInfo.Clear();

			if (!File.Exists(ConfigFilePath)) return;

			var config = new ConfigFile(ConfigFilePath);

			if (!config.IsSectionExist(area)) return;

			for (var i = 0; i < cellMax; i++)
			{
				var cellName = config.GetString(area, $"{i}", null);
				if (cellName == null) continue;

				Master.CellInfo.Add(i, cellName);
			}
		}

		/// <summary>
		/// 進撃位置更新
		/// </summary>
		/// <param name="id">セルID</param>
		private void ChangeCellID(int id, int eventID, int eventKind)
		{
			var cell = $"id={id}";

			if (Master.CellInfo != null)
			{
				var value = "";
				if(Master.CellInfo.TryGetValue(id, out value))
				{
					cell = value;
				}
			}

			var kindIndex = eventKind;
			if((eventID == 1) || (eventID == 6))
			{
				kindIndex += 10;
			}
			else if(eventID == 7)
			{
				kindIndex += 20;
			}

			Master.CellID = $"{cell} [\'{CellEvents[eventID]}\'／\'{CellKinds[kindIndex]}\']";
		}

		/// <summary>
		/// 戦闘結果更新
		/// </summary>
		/// <param name="request"></param>
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
