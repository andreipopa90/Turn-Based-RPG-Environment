using System;
using System.Collections.Generic;

namespace Model
{
	[Serializable]
	public class Move
	{
		public string KeyName { get; set; }
		public string Name { get; set; }
		public int Accuracy { get; set; }
		public int BasePower { get; set; }
		public string Category { get; set; }
		public string Target { get; set; }
		public string MoveType { get; set; }
		public Dictionary<string, int> Boosts { get; set; }
		public List<int> Drain { get; set; }
	
	}
}
