using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

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

		private Dictionary<string, string> _statusToString = new()
		{
			{ "brn", "burning" },
			{ "par", "paralysis" },
			{ "psn", "poison" }
		};
		
		private string DrainToString()
		{
			var result = new StringBuilder();
			if (Secondary.ContainsKey("drain"))
				result.Append(Secondary["drain"] == 1 ? "Drain" : "High Drain").Append("\n");
			return result.ToString();
		}

		private string StatusAilmentToString()
		{
			var result = new StringBuilder();
			if (!Secondary.ContainsKey("status")) return result.ToString();
			
			var statusEffect = ((JObject)Secondary["status"]).ToObject<Dictionary<string, string>>();
			result.Append(statusEffect["chance"]).Append("% chance of ").Append(_statusToString[statusEffect["status"]]).Append("\n");
			return result.ToString();
		}

		private string SelfBuffToString()
		{
			var result = new StringBuilder();
			if (!Secondary.ContainsKey("self-buff")) return result.ToString();

			var selfBuff = ((JObject)Secondary["self-buff"]).ToObject<SelfBuff>();
			result.Append("Boosts: ");
			foreach (var key in selfBuff.Self["boosts"].Keys)
			{
				result.Append(key).Append(", ");
			}

			result.Remove(result.Length - 2, 2);
			result.Append("\n");
			return result.ToString();
		}

		private string DeBuffToString()
		{
			if (!Secondary.ContainsKey("debuff")) return string.Empty;
			var result = new StringBuilder();
			var debuff = ((JObject)Secondary["debuff"]).ToObject<Dictionary<string, dynamic>>();
			var boosts = ((JObject)debuff["boosts"]).ToObject<Dictionary<string, int>>();
			result.Append(debuff["chance"]).Append("% chance to reduce: ");
			foreach (var key in boosts.Keys)
			{
				result.Append(key).Append(", ");
			}

			result.Remove(result.Length - 2, 2);
			result.Append("\n");
			return result.ToString();
		}

		private string StatusToString()
		{
			if (!Category.Equals("Status")) return string.Empty;
			if (!Secondary.ContainsKey("boosts")) return string.Empty;
			var boosts = ((JObject)Secondary["boosts"]).ToObject<Dictionary<string, int>>();
			var result = new StringBuilder();
			result.Append(Target.Equals("self") ? "Boosts: " : "Reduces: ");
			foreach (var key in boosts.Keys)
			{
				result.Append(key).Append(", ");
			}

			result.Remove(result.Length - 2, 2);
			result.Append("\n");
			return result.ToString();
		}

		public override string ToString()
		{
			var result = new StringBuilder();
			result.Append(Name).Append("\n");
			if (!Category.Equals("Status"))
				result.Append("Power: ").Append(BasePower).Append("\n");
			result.Append("Target: ").Append(Target.Equals("self") ? "Self\n" : "Enemy\n");
			result.Append("Category: ").Append(Category).Append("\n");
			result.Append("Type: ").Append(MoveType).Append("\n");
			result.Append(DrainToString());
			result.Append(StatusAilmentToString());
			result.Append(SelfBuffToString());
			result.Append(StatusToString());
			return result.ToString();
		}
	}
}
