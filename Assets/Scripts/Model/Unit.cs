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
        [FormerlySerializedAs("Types")] public List<string> types;
        [FormerlySerializedAs("UnitName")] public string unitName;
        [FormerlySerializedAs("MaxHealth")] public int maxHealth;
        [FormerlySerializedAs("CurrentHealth")] public int currentHealth;
        [FormerlySerializedAs("Attack")] public int attack;
        [FormerlySerializedAs("Defense")] public int defense;
        [FormerlySerializedAs("MagicAttack")] public int magicAttack;
        [FormerlySerializedAs("MagicDefense")] public int magicDefense;
        [FormerlySerializedAs("Speed")] public int speed;
        [FormerlySerializedAs("Level")] public int level;
        [FormerlySerializedAs("Moves")] public List<Move> moves;
        private Dictionary<string, int> _iv;
        [FormerlySerializedAs("UnitNature")] public Nature unitNature;
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
            double effectiveness = 1;
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

        public void TakeDamage(Move move, Unit enemySource)
        {
            var effectiveness = DetermineMoveEffectiveness(move.MoveType);

            var defenseUsed = move.Category.Equals("Special") ? magicDefense : defense;
            var sourceAttackUsed = move.Category.Equals("Special") ? enemySource.magicAttack : enemySource.attack;
            var randomValue = new System.Random().NextDouble() * (UpperBound - LowerBound) + LowerBound;

            var damageTaken = (int) (((2 * enemySource.level / 5 + 2) * 
                (sourceAttackUsed / defenseUsed) * move.BasePower / 50 + 2) * randomValue * effectiveness);

            currentHealth -= damageTaken;
            if (Affixes.Contains("Sturdy"))
            {
                currentHealth = 1;
                Affixes.Remove("Sturdy");
            }
            if (IsDead())
            {
                Destroy(gameObject);
            }
        }

        public void Heal(int healAmount)
        {
            currentHealth += healAmount;
        }

        private int CalculateHp(int baseHp)
        {
            var hp = (int) System.Math.Floor((2 * baseHp + _iv["hp"]) * level / 100f) + level + 10;
            return hp;
        }

        private int CalculateStats(int baseStat, string stat)
        {
            if (level == 1) return baseStat;
            double natureModifier = 1;
            if (stat.Equals(unitNature.Plus)) natureModifier = 1.1;
            else if (stat.Equals(unitNature.Minus)) natureModifier = 0.9;
            var statValue = (int)((System.Math.Floor((2 * baseStat + _iv[stat]) * level / 100d) + 5) * natureModifier);
            return statValue;
        }

        public void SetStats(BaseStat stats)
        {
            GenerateIVs();

            currentHealth = CalculateHp(stats.Hp);
            maxHealth = currentHealth;

            attack = CalculateStats(stats.Atk, "atk");
            defense = CalculateStats(stats.Def, "def");
            magicAttack = CalculateStats(stats.Spa, "spa");
            magicDefense = CalculateStats(stats.Spd, "spd");
            speed = CalculateStats(stats.Spe, "spe");
        }

        private bool IsDead()
        {
            return currentHealth <= 0;
        }

    
    }
}
