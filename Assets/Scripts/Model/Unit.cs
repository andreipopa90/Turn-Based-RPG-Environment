using System;
using System.Collections.Generic;
using System.Linq;
using Model.Observer;
using UnityEngine;

namespace Model
{
    public class Unit : MonoBehaviour
    {
        private readonly Dictionary<string, string> _typeColor = new()
        {
            {"Normal", "#A8A77AFF"},
            {"Fire", "#EE8130FF"},
            {"Water", "#6390F0FF"},
            {"Electric", "#F7D02CFF"},
            {"Grass", "#7AC74CFF"},
            {"Ice", "#96D9D6FF"},
            {"Fighting", "#C22E28FF"},
            {"Poison", "#A33EA1FF"},
            {"Ground", "#E2BF65FF"},
            {"Flying", "#A98FF3FF"},
            {"Psychic", "#F95587FF"},
            {"Bug", "#A6B91AFF"},
            {"Rock", "#B6A136FF"},
            {"Ghost", "#735797FF"},
            {"Dragon", "#6F35FCFF"},
            {"Dark", "#705746FF"},
            {"Steel", "#B7B7CEFF"},
            {"Fairy", "#D685ADFF"}
        };

        public GameStateStorage _gameState;
        private const double UpperBound = 1.00;
        private const double LowerBound = 0.85;
        public List<string> Types { get; set; }
        public string UnitName {get;set;}
        public int MaxHealth {get;set;}
        public int CurrentHealth {get;set;}
        public int Atk {get;set;}
        public int Def {get;set;}
        public int Spa {get;set;}
        public int Spd {get;set;}
        public int Spe {get;set;}
        public int Level {get;set;}
        
        public GameObject typeIndicatorOne;
        public GameObject typeIndicatorTwo;
        public List<Move> Moves { get; set; }
        
        public Dictionary<string, int> _iv;
        public Dictionary<string, int> Ev { get; set; }
        
        public Nature unitNature;
        public List<string> Affixes { get; set; }
        public List<string> Ailments { get; set; }
        public EventManager Manager { get; set; }
        public BaseStat OriginalStats { get; set; }

        private void Awake()
        {
            Moves = new List<Move>();
            CurrentHealth = MaxHealth;
            Affixes = new List<string>();
            Ailments = new List<string>();
            //_gameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
            GenerateIVs();
        }

        public void GenerateIVs()
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

        public double DetermineMoveEffectiveness(string moveType)
        {
            var effectiveness = 1.0;
            var typeChart = _gameState.TypeChart;
            foreach (var damageTaken in Types.Select(type => 
                         typeChart.Find(x => x.Name.Equals(type)).DamageTaken))
            {
                switch(damageTaken[moveType])
                {
                    case 0:
                        effectiveness *= 1;
                        break;
                    case 1:
                        effectiveness = !Ailments.Contains("ReverseWeakness") ? 
                            effectiveness * 2 : effectiveness / 2;
                        break;
                    case 2:
                        effectiveness = !Ailments.Contains("ReverseWeakness") ? 
                            effectiveness / 2 : effectiveness * 2;
                        break;
                    default:
                        effectiveness = !Ailments.Contains("ReverseWeakness") ? 
                            effectiveness * 0 : effectiveness * 2;
                        break;
                }
            }
            return effectiveness;
        }

        public int TakeDamage(Move move, Unit enemySource, double multiplier = 1.0)
        {
            var effectiveness = DetermineMoveEffectiveness(move.MoveType);

            if (effectiveness > 1 && Affixes.Contains("Domain")) effectiveness = 1;
            
            var defenseUsed = (double) (move.Category.Equals("Special") ? Spd : Def);
            var sourceAttackUsed = (double) (move.Category.Equals("Special") ? enemySource.Spa : enemySource.Atk);
            var randomValue = new System.Random().NextDouble() * (UpperBound - LowerBound) + LowerBound;
            var stab = enemySource.Types.Contains(move.MoveType) ? 1.5 : 1.0;

            var damageTaken = (int) (((Math.Round(2.0 * enemySource.Level / 5.0, 2) + 2) * 
                                         Math.Round(sourceAttackUsed / defenseUsed * move.BasePower / 50.0, 2) + 2) * 
                                     randomValue * effectiveness * stab * multiplier);

            var successChance = new System.Random().Next(0, 100);
            if (successChance <= move.Accuracy)
            {
                CurrentHealth -= damageTaken;
            } else if (Affixes.Contains("CounterAttack")) enemySource.TakeDamage(damageTaken / 4);

            if (Affixes.Contains("Sturdy") && CurrentHealth <= 0)
            {
                CurrentHealth = 1;
                Affixes.Remove("Sturdy");
            }

            if (!IsDead())
            {
                Evolve();
                return damageTaken;
            }

            // Manager.RemoveListener(this);
            // Manager.NotifyOnDeath();
            // Destroy(gameObject);

            return damageTaken;
        }

        private void Evolve()
        {
            if (CurrentHealth > MaxHealth / 2) return;
            var newUnit = _gameState.BaseStats.Find(bs => bs.Name.Equals("Zygarde-Complete"));
            var currentHealthPercentage = CurrentHealth / (MaxHealth * 1.0);
            SetStats(newUnit, true);
            CurrentHealth = (int) (CurrentHealth * currentHealthPercentage);
        }

        public void TakeDamage(int value)
        {
            CurrentHealth -= value;
            if (Affixes.Contains("Sturdy") && CurrentHealth <= 0)
            {
                CurrentHealth = 1;
                Affixes.Remove("Sturdy");
            }
            if (CurrentHealth <= 0)
            {
                Manager.RemoveListener(this);
                Manager.NotifyOnDeath();
                Destroy(gameObject);
            }
            else
            {
                Evolve();
            }
        }

        public void Heal(int healAmount)
        {
            CurrentHealth = Math.Min(CurrentHealth + healAmount, MaxHealth);
        }

        private int CalculateHp(int baseHp)
        {
            var hp = (int) Math.Floor((2 * baseHp + _iv["hp"] + Math.Floor(Ev["hp"] / 4.0)) * Level / 100f) + Level + 10;
            return hp;
        }

        private int CalculateStats(int baseStat, string stat)
        {
            double natureModifier = 1;
            if (stat.Equals(unitNature.Plus)) natureModifier = 1.1;
            else if (stat.Equals(unitNature.Minus)) natureModifier = 0.9;
            var statValue = (int) Math.Floor((Math.Floor((2 * baseStat + _iv[stat] + Math.Floor(Ev[stat] / 4.0)) * Level / 100d) + 5) * natureModifier);
            return statValue;
        }

        public void SetStats(BaseStat stats, bool evolution = false)
        {
            CurrentHealth = CalculateHp(stats.Hp);
            MaxHealth = CurrentHealth;

            Atk = CalculateStats(stats.Atk, "atk");
            Def = CalculateStats(stats.Def, "def");
            Spa = CalculateStats(stats.Spa, "spa");
            Spd = CalculateStats(stats.Spd, "spd");
            Spe = CalculateStats(stats.Spe, "spe");
        }

        private bool IsDead()
        {
            return CurrentHealth <= 0;
        }

        public void Cure()
        {
            if (Ailments.Contains("par"))
            {
                Spe *= 4 / 3;
            }
            Ailments.Clear();
        }

        public override string ToString()
        {
            var result = string.Empty;
            result += UnitName + "\n";
            result += "Level: " + Level + "\n";
            result += Types[0];
            result = Types.Count == 2 ? result + " " + Types[1] : result;
            result += "\n";
            
            return result;
        }

        public void SetTypes(List<string> characterBaseTypes)
        {
            Types = characterBaseTypes;
            if (ColorUtility.TryParseHtmlString(_typeColor[Types[0]], out var color))
                typeIndicatorOne.GetComponent<MeshRenderer>().material.color = color;
            typeIndicatorTwo.GetComponent<MeshRenderer>().material.color = 
                Types.Count == 2 ? 
                    ColorUtility.TryParseHtmlString(_typeColor[Types[1]], out color) ? color : Color.white : 
                    ColorUtility.TryParseHtmlString(_typeColor[Types[0]], out color) ? color : Color.white;
        }
    }
}
