using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class JSONReader
{

	private string PathRoot = Path.Combine(@"", "Assets", "Scripts", "JsonParser");

	public List<Move> ReadMovesJSON()
	{
		List<Move> moves;
		StreamReader reader = new(Path.Combine(PathRoot, "moves.json"));
		string json = reader.ReadToEnd();
		moves = JsonConvert.DeserializeObject<List<Move>>(json);
		reader.Close();

		return moves;
	}

	public List<BaseStat> ReadBaseStatsJSON()
	{
		List<BaseStat> BaseStats;
		StreamReader reader = new(Path.Combine(PathRoot, "pokedex.json"));
		string json = reader.ReadToEnd();
		BaseStats = JsonConvert.DeserializeObject<List<BaseStat>>(json);
		reader.Close();

		return BaseStats;
	}

	public List<Type> ReadTypeChart()
	{
		List<Type> TypeChart;
		StreamReader reader = new(Path.Combine(PathRoot, "typechart.json"));
		string json = reader.ReadToEnd();
		TypeChart = JsonConvert.DeserializeObject<List<Type>>(json);
		reader.Close();

		return TypeChart;
	}
}
