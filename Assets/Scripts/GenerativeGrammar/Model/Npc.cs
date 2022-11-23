using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;

namespace GenerativeGrammar.Model
{
    public class Npc
    {
        public Dictionary<string, int> Attributes { get; }

        public Dictionary<string, List<dynamic>> ValuesOfNodes { get; set; }

        public Npc()
        {
            Attributes = new Dictionary<string, int>();
            ValuesOfNodes = new Dictionary<string, List<dynamic>>();
        }

        public void GetNodesTerminalValues(string nodeName, ref List<dynamic> result, ref List<string> visited)
        {
            foreach (var nextNode in ValuesOfNodes[nodeName])
            {
                if (!ValuesOfNodes.ContainsKey(nextNode.ToString()))
                {
                    result.Add(nextNode);
                }
                else if (!visited.Contains(nextNode))
                {
                    visited.Add(nextNode);
                    GetNodesTerminalValues(nextNode.ToString(), ref result, ref visited);
                }
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append("Types: ");
            foreach (var type in ValuesOfNodes["TYPE"])
            {
                result.Append(type.Name);
                result.Append(" ");
            }

            result.AppendLine();
            result.Append("Base: ").Append(ValuesOfNodes["BASE"][0].Name).AppendLine();
            result.Append("Nature: ").Append(ValuesOfNodes["NATURE"][0].Name).AppendLine();
            result.Append("Moves: ");
            foreach (var move in ValuesOfNodes["MOVE"])
            {
                result.Append(move);
                result.Append(" ");
            }

            result.AppendLine();
            result.Append("Affixes: ");
            foreach (var affix in ValuesOfNodes["AFFIX"])
            {
                result.Append(affix.ToString());
                result.Append(" ");
            }

            result.AppendLine();
            return result.ToString();
        }
    }
}