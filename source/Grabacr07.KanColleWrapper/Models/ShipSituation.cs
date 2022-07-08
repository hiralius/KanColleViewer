using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models
{
	[Flags]
	public enum ShipSituation
	{
		None = 0,
		//Sortie = 1,
		/// <summary>
		/// 入渠
		/// </summary>
		Repair = 1 << 1,
		/// <summary>
		/// 退避
		/// </summary>
		Evacuation = 1 << 2,
		/// <summary>
		/// 退避護衛
		/// </summary>
		Tow = 1 << 3,
		//ModerateDamaged = 1 << 4,
		/// <summary>
		/// 大破
		/// </summary>
		HeavilyDamaged = 1 << 5,
		/// <summary>
		/// ダメコン搭載
		/// </summary>
		DamageControlled = 1 << 6,
	}
}
