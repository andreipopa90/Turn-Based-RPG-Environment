using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class JSONReader: MonoBehaviour
{
	public void ReadAbilityJSON()
	{
		List<Move> source;
		StreamReader reader = new StreamReader(
			Path.Combine(@"", "Assets", "Scripts", "Combat", "moves.json"));
		string json = reader.ReadToEnd();
		source = JsonConvert.DeserializeObject<List<Move>>(json);
	}
}
