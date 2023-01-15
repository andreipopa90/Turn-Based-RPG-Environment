using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using UnityEngine.Device;

namespace LogFiles
{
    public class Statistics
    {
        private Dictionary<int, List<int>> Difficulty { get; }
        private Dictionary<int, HashSet<string>> MovesUsed { get; }

        private static Statistics Instance { get; set; }

        public static Statistics GetInstance()
        {
            return Instance ??= new Statistics();
        }

        private Statistics()
        {
            Difficulty = new Dictionary<int, List<int>>();
            MovesUsed = new Dictionary<int, HashSet<string>>();
        }
        
        public void AddDifficulty(int currentLevel, Unit unit)
        {
            if (!Difficulty.ContainsKey(currentLevel)) Difficulty[currentLevel] = new List<int>();
            Difficulty[currentLevel].Add(unit.OriginalStats.Hp + unit.OriginalStats.Atk + unit.OriginalStats.Def +
                                       unit.OriginalStats.Spa + unit.OriginalStats.Spd + unit.OriginalStats.Spe +
                                       unit.Ev["hp"] + unit.Ev["atk"] + unit.Ev["def"] + unit.Ev["spa"] + 
                                       unit.Ev["spd"] + unit.Ev["spe"]);
        }

        public void AddMoveUsed(int currentLevel, string move)
        {
            if (!MovesUsed.ContainsKey(currentLevel)) MovesUsed[currentLevel] = new HashSet<string>();
            MovesUsed[currentLevel].Add(move);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append("Difficulty Over Time").AppendLine();
            foreach (var key in Difficulty.Keys)
            {
                result.Append("Level ").Append(key).Append(" ").Append(Difficulty[key].Average()).AppendLine();
            }
            result.Append("Amount of Moves Used Over Time").AppendLine();
            foreach (var key in MovesUsed.Keys)
            {
                result.Append("Level ").Append(key).Append(" ").Append(MovesUsed[key].Count).AppendLine();
            }
            return result.ToString();
        }

        public void PrintStatistics()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Output.txt");
            var file = new StreamWriter(path, append: true);
            file.WriteLine(ToString());
            file.Close();
        }
    }
}