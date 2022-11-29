using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Model
{
	[Serializable]
	public class BaseStat : ICloneable
	{
		public string Name { get; set; }
		public string KeyName { get; set; }
		public int Hp { get; set; }
		public int Atk { get; set; }
		public int Def { get; set; }
		public int Spa { get; set; }
		public int Spd { get; set; }
		public int Spe { get; set; }
		public List<string> Types { get; set; }
		public object Clone()
		{
			var clone = new BaseStat();
			clone.Hp = Hp;
			clone.Atk = Atk;
			clone.Spa = Spa;
			clone.Def = Def;
			clone.Spd = Spd;
			clone.Spe = Spe;
			clone.Types = Types;
			clone.Name = Name;
			clone.KeyName = KeyName;
			return clone;
		}
	}
}
