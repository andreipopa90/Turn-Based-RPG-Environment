using System;
using System.Collections.Generic;
using System.Text;

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
		public Dictionary<string, dynamic> Secondary { get; set; }

		public override string ToString()
		{
			var result = new StringBuilder();
			result.Append(Name).Append("\n");
			result.Append("Power: ").Append(BasePower).Append("\n");
			result.Append("Category: ").Append(Category).Append("\n");
			result.Append("Type: ").Append(MoveType).Append("\n");
			return result.ToString();
		}
	}
}
