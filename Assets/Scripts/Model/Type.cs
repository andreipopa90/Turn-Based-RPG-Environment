using System;
using System.Collections.Generic;

namespace Model
{
	[Serializable]
	public class Type
	{
		public string Name { get; set; }
		public Dictionary<string, int> DamageTaken { get; set; }
	}
}
