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
			var clone = new BaseStat
			{
				Hp = Hp,
				Atk = Atk,
				Spa = Spa,
				Def = Def,
				Spd = Spd,
				Spe = Spe,
				Types = Types,
				Name = Name,
				KeyName = KeyName
			};
			return clone;
		}

		public override string ToString()
		{
			return "hp: " + Hp + "\n" +
			       "atk: " + Atk + "\n" +
			       "def: " + Def + "\n" +
			       "spa: " + Spa + "\n" +
			       "spd: " + Spd + "\n" +
			       "spe: " + Spe + "\n";
		}
	}
}
