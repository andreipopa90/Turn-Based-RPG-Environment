using System.Collections.Generic;

namespace Model
{
    public static class StarterTable
    {
        public static readonly Dictionary<string, List<string>> StartersList = new()
        {
            {"Sceptile", new List<string> {"leafage", "dualchop", "captivate", "swordsdance"}},
            {"Blaziken", new List<string> {"scratch", "ember", "featherdance", "bulkup"}},
            {"Swampert", new List<string> {"watergun", "tackle", "screech", "bulkup"}}
            
        };
    }
}