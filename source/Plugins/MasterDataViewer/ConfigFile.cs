using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace utility

{
	/// <summary>
	/// 設定ファイルクラス
	/// </summary>
	public class ConfigFile
	{
		/// <summary>
		/// 1行あたり最大文字数
		/// </summary>
		protected const int STRING_MAX = 256;
		/// <summary>
		/// セクション最大数
		/// </summary>
		private const int SECTION_MAX = 100;

		// Win32APIの GetPrivateProfileString を使う宣言
		[DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode, SetLastError = true)]
		protected static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName,
			string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

		[DllImport("KERNEL32.DLL",
			EntryPoint = "GetPrivateProfileStringA")]
		private static extern uint GetPrivateProfileStringByByteArray(string lpAppName, string lpKeyName, 
			string lpDefault, byte[] lpReturnedString, uint nSize, string lpFileName);

		/// <summary>
		/// 設定ファイルのパス
		/// </summary>
		private readonly string filePath;
		/// <summary>
		/// セクション名リスト
		/// </summary>
		private List<string> sections;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="path">設定ファイルのパス</param>
		public ConfigFile(string path)
		{
			this.filePath = path;

			// セクション名一覧を取得
			sections = new List<string>();
			var buff = new byte[STRING_MAX * SECTION_MAX];
			var len = GetPrivateProfileStringByByteArray(null, null, null, buff, (uint)buff.Length, path);
			if(len > 0)
			{
				var strs = Encoding.Default.GetString(buff, 0, (int)len - 1);
				sections.AddRange(strs.Split('\0'));
			}
		}

		/// <summary>
		/// IPアドレス文字列→IPEndPoint変換
		/// </summary>
		/// <param name="text">IPアドレス表記文字列</param>
		/// <returns>生成されたIPEndPoint</returns>
		protected static IPEndPoint ParseIPEndPoint(string text)
		{
			if (Uri.TryCreate(text, UriKind.Absolute, out Uri uri))
				return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);
			if (Uri.TryCreate(String.Concat("tcp://", text), UriKind.Absolute, out uri))
				return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);
			if (Uri.TryCreate(String.Concat("tcp://", String.Concat("[", text, "]")), UriKind.Absolute, out uri))
				return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);
			throw new FormatException("Failed to parse text to IPEndPoint");
		}

		/// <summary>
		/// 設定ファイルから定義情報を取得
		/// </summary>
		/// <param name="section">セクション名</param>
		/// <param name="key">定義キー</param>
		/// <param name="defStr">デフォルト定義</param>
		/// <returns>定義情報</returns>
		public string GetString(string section, string key, string defStr)
		{
			StringBuilder sb = new StringBuilder(STRING_MAX);

			uint ret = GetPrivateProfileString(section, key, defStr, sb, Convert.ToUInt32(sb.Capacity), filePath);

			if (ret != 0)
				return sb.ToString();

			return null;
		}

		/// <summary>
		/// 番号リストの取得
		/// </summary>
		/// <param name="section">セクション</param>
		/// <param name="key">キー</param>
		/// <returns>番号リスト</returns>
		/// <remarks>
		/// カンマ区切りの番号リストを取得</br>
		/// "A-B"とすることで、A～Bまでの連番も取得可能
		/// </remarks>
		public IReadOnlyList<int> GetNumberList(string section, string key)
		{

			// 番号リスト(文字列)の取得
			var str = GetString(section, key, null);
			if (str == null)
			{
				// 設定が無ければデフォルト
				return null;
			}

			// カンマ区切りで分割し、配列に格納
			var result = new List<int>();
			var param = str.Split(',');
			// 連番対応
			foreach (var para in param)
			{
				if (para.Contains('-'))
				{
					// "-"があれば連番に変換して格納
					var subParam = para.Split('-');
					if (subParam.Length != 2)
					{
						throw new FormatException($"連番設定が異常です ({para})");
					}
					var start = int.Parse(subParam[0]);
					var count = int.Parse(subParam[1]) - start + 1;
					result.AddRange(Enumerable.Range(start, count));
				}
				else
				{
					// ただの数値なので、そのまま格納
					result.Add(int.Parse(para));
				}
			}

			result.Sort();
			return result;
		}

		/// <summary>
		/// セクション名が設定ファイル上に存在するかチェック
		/// </summary>
		/// <param name="section">セクション名</param>
		/// <returns>true:セクション名あり</returns>
		public bool IsSectionExist(string section)
		{
			return sections.Contains(section);
		}
	}
}
