using System.Collections.Generic;
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

        private void Start()
        {
            moves = new();
            currentHealth = maxHealth;
            GenerateIVs();
        }

        private void GenerateIVs()
        {
            _iv = new Dictionary<string, int>();
            for (var i = 0; i < 6; i++)
            {
                var randomNumber = new System.Random().Next(0, 32);
                switch (i)
                {
                    case 0:
                        _iv["hp"] = randomNumber;
                        break;
                    case 1:
                        _iv["atk"] = randomNumber;
                        break;
                    case 2:
                        _iv["def"] = randomNumber;
                        break;
                    case 3:
                        _iv["spa"] = randomNumber;
                        break;
                    case 4:
                        _iv["spd"] = randomNumber;
                        break;
                    case 5:
                        _iv["spe"] = randomNumber;
                        break;
                }
            }
        }

        private double DetermineMoveEffectiveness(string moveType)
        {
            double effectiveness = 1;
            _gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            var typeChart = _gameState.TypeChart;
            foreach(var type in types)
            {
                var damageTaken = typeChart.Find(x => x.Name.Equals(type)).DamageTaken;
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

            var damageTaken = (int) ((((2 * enemySource.level / 5 + 2) * (sourceAttackUsed / defenseUsed) * move.BasePower) / 50 + 2) * randomValue * effectiveness);

            currentHealth -= damageTaken;
            if (IsDead())
            {
                Destroy(this.gameObject);
            }
        }

        private int CalculateHp(int baseHp)
        {
            var hp = (int) System.Math.Floor((2 * baseHp + _iv["hp"]) * level / 100f) + level + 10;
            return hp;
        }

        private int CalculateStats(int baseStat, string stat)
        {
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
            defense = stats.Def;
            magicAttack = stats.Spa;
            magicDefense = stats.Spd;
            speed = stats.Spe;
        }

        private bool IsDead()
        {
            return currentHealth <= 0;
        }

    
    }
}
