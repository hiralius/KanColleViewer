using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Livet;

using Grabacr07.KanColleViewer.Properties;

namespace Grabacr07.KanColleViewer.Plugins
{
	public class SettingsBase : NotificationObject
	{
		public String Header;

		#region Text 変更通知プロパティ

		private string _Text;

		public string Text
		{
			get { return this._Text; }
			set
			{
				if (this._Text != value)
				{
					this._Text = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region File 変更通知プロパティ

		private string _File;

		public string File
		{
			get { return this._File ; }
			set
			{
				if (this._File != value)
				{
					this._File = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Enableプロパティ
		private bool _Enable;

		public bool Enable
		{
			get { return this._Enable; }
			set
			{
				if(this._Enable != value)
				{
					this._Enable = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public void TestSound()
		{
			Toast.PlaySound(File);
		}

		public void Reference()
		{
			var diag = new OpenFileDialog();
			if(diag.ShowDialog() == DialogResult.OK)
			{
				File = diag.FileName;
			}
		}

		public SettingsBase()
		{

		}

		public SettingsBase(string header)
		{
			Header = header;
		}
	}

	public class Settings
	{
		public readonly ObservableCollection<SettingsBase> Collections;
		private readonly string filename;
		private readonly XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SettingsBase>));

		public Settings(string settingsFile)
		{
			filename = settingsFile;
			Collections = LoadFile(settingsFile);
		}

		private ObservableCollection<SettingsBase> LoadFile(string file)
		{
			ObservableCollection<SettingsBase> lists = new ObservableCollection<SettingsBase>();
			try
			{
				//読み込むファイルを開く（UTF-8 BOM無し）
				var sr = new StreamReader(file, new UTF8Encoding(false), false);
				//デシリアライズし、メンバにセットする
				lists = serializer.Deserialize(sr) as ObservableCollection<SettingsBase>;
				//ファイルを閉じる
				sr.Close();
			}
			catch (Exception)
			{
				SettingsBase setting = new SettingsBase(Resources.Expedition_NotificationMessage_Title)
				{
					Text = "遠征完了"
				};
				lists.Add(setting);

				setting = new SettingsBase(Resources.Repairyard_NotificationMessage_Title)
				{
					Text = "入渠完了"
				};
				lists.Add(setting);

				setting = new SettingsBase(Resources.Dockyard_NotificationMessage_Title)
				{
					Text = "建造完了"
				};
				lists.Add(setting);

				setting = new SettingsBase("疲労回復完了")
				{
					Text = "疲労回復"
				};
				lists.Add(setting);
			}

			foreach (SettingsBase setting in lists)
			{
				setting.PropertyChanged += Changed;
			}

			return lists;
		}

		private void Changed(object sender, PropertyChangedEventArgs e)
		{
			SaveFile();
		}

		private void SaveFile()
		{
			try
			{
				// 保存先ディレクトリが無ければ作る
				var dir = Path.GetDirectoryName(filename);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}

				//書き込むファイルを開く（UTF-8 BOM無し）
				var sw = new StreamWriter(filename, false, new UTF8Encoding(false));
				//シリアル化し、XMLファイルに保存する
				serializer.Serialize(sw, Collections);
				//ファイルを閉じる
				sw.Close();
			}
			catch (Exception e)
			{
				_ = MessageBox.Show(e.ToString());
			}
		}
	}

}
