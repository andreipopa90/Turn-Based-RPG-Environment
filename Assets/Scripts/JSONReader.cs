using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class JSONReader
{
	public List<Move> ReadMovesJSON()
	{
		List<Move> moves;
		StreamReader reader = new StreamReader(
			Path.Combine(@"", "Assets", "Scripts", "Combat", "moves.json"));
		string json = reader.ReadToEnd();
		moves = JsonConvert.DeserializeObject<List<Move>>(json);

		return moves;
	}
}
