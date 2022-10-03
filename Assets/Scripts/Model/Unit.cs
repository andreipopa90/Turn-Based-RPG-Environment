using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Model
{
    public class Unit : MonoBehaviour
    {
        private GameStateStorage _gameState;
        private const double UpperBound = 1.00;
        private const double LowerBound = 0.85;
        public List<string> types;
        public string unitName;
        public int maxHealth;
        public int currentHealth;
        public int atk;
        public int def;
        public int spa;
        public int spd;
        public int spe;
        public int level;
        public List<Move> moves;
        private Dictionary<string, int> _iv;
        private Dictionary<string, int> _ev;
        public Nature unitNature;
        public List<string> Affixes { get; set; }
        public List<string> Ailments { get; set; }

        private void Start()
        {
            moves = new List<Move>();
            currentHealth = maxHealth;
            Affixes = new List<string>();
            Ailments = new List<string>();
            _gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            GenerateIVs();
        }

        private void GenerateIVs()
        {
            _iv = new Dictionary<string, int>();
            var randomNumber = new System.Random().Next(0, 32);
            _iv["hp"] = randomNumber;
            
            randomNumber = new System.Random().Next(0, 32);
            _iv["atk"] = randomNumber;
            
            randomNumber = new System.Random().Next(0, 32);
            _iv["def"] = randomNumber;
            
            randomNumber = new System.Random().Next(0, 32);
            _iv["spa"] = randomNumber;
            
            randomNumber = new System.Random().Next(0, 32);
            _iv["spd"] = randomNumber;
            
            randomNumber = new System.Random().Next(0, 32);
            _iv["spe"] = randomNumber;
        }

        private double DetermineMoveEffectiveness(string moveType)
        {
            var effectiveness = 1.0;
            var typeChart = _gameState.TypeChart;
            foreach (var damageTaken in types.Select(type => 
                         typeChart.Find(x => x.Name.Equals(type)).DamageTaken))
            {
                switch(damageTaken[moveType])
                {
                    case 0:
                        effectiveness *= 1;
                        break;
                    case 1:
                        effectiveness *= 2;
                        break;
                    case 2:
                        effectiveness /= 2;
                        break;
                    default:
                        effectiveness *= 0;
                        break;
                }
            }
            return effectiveness;
        }

        public int TakeDamage(Move move, Unit enemySource)
        {
            var effectiveness = DetermineMoveEffectiveness(move.MoveType);

            var defenseUsed = (double) (move.Category.Equals("Special") ? spd : def);
            var sourceAttackUsed = (double) (move.Category.Equals("Special") ? enemySource.spa : enemySource.atk);
            var randomValue = new System.Random().NextDouble() * (UpperBound - LowerBound) + LowerBound;
            var stab = enemySource.types.Contains(move.MoveType) ? 1.5 : 1.0;

            var damageTaken = (int) (((Math.Round(2.0 * enemySource.level / 5.0, 2) + 2) * 
                                         Math.Round(sourceAttackUsed / defenseUsed * move.BasePower / 50.0, 2) + 2) * 
                                     randomValue * effectiveness * stab);

            currentHealth -= damageTaken;
            if (Affixes.Contains("Sturdy") && currentHealth <= 0)
            {
                currentHealth = 1;
                Affixes.Remove("Sturdy");
            }
            if (IsDead())
            {
                Destroy(gameObject);
            }

            return damageTaken;
        }

        public void Heal(int healAmount)
        {
            currentHealth = Math.Min(currentHealth + healAmount, maxHealth);
        }

        private int CalculateHp(int baseHp)
        {
            var hp = (int) Math.Floor((2 * baseHp + _iv["hp"] + Math.Floor(_ev["hp"] / 4.0)) * level / 100f) + level + 10;
            return hp;
        }

        private int CalculateStats(int baseStat, string stat)
        {
            if (level == 1) return baseStat;
            double natureModifier = 1;
            if (stat.Equals(unitNature.Plus)) natureModifier = 1.1;
            else if (stat.Equals(unitNature.Minus)) natureModifier = 0.9;
            var statValue = (int) Math.Floor((Math.Floor((2 * baseStat + _iv[stat] + Math.Floor(_ev[stat] / 4.0)) * level / 100d) + 5) * natureModifier);
            return statValue;
        }

        public void SetStats(BaseStat stats)
        {
            GenerateIVs();
            GenerateEVs();

            currentHealth = CalculateHp(stats.Hp);
            maxHealth = currentHealth;

            atk = CalculateStats(stats.Atk, "atk");
            def = CalculateStats(stats.Def, "def");
            spa = CalculateStats(stats.Spa, "spa");
            spd = CalculateStats(stats.Spd, "spd");
            spe = CalculateStats(stats.Spe, "spe");
        }

        private void GenerateEVs()
        {
            _ev = new Dictionary<string, int>
            {
                {"hp", 0},
                {"atk", 0},
                {"def", 0},
                {"spa", 0},
                {"spd", 0},
                {"spe", 0}
            };
        }

        public void AddAffixes(List<string> newAffixes)
        {
            Affixes = newAffixes;
        }
        

        private bool IsDead()
        {
            return currentHealth <= 0;
        }

    
    }
}
