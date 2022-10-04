using System;
using System.Collections.Generic;

namespace Model
{
    [Serializable]
    public class SelfBuff
    {
        public int Chance { get; set; }
        public Dictionary<string, Dictionary<string, int>> Self { get; set; }
    }
}