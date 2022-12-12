using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using Newtonsoft.Json.Linq;
using UI;
using UI.Battle;
using UnityEngine;
using Action = Model.Action;

namespace BattleSystem
{
    public class BattleHandler : MonoBehaviour
    {

        private readonly Dictionary<string, string> _ailments = new()
        {
            {"par", "Paralysis"},
            {"brn", "Burn"},
            {"psn", "Poison"}
        };

        private readonly Dictionary<string, string> _stats = new()
        {
            {"spe", "Speed"},
            {"atk", "Attack"},
            {"def", "Defense"},
            {"spa", "Special Attack"},
            {"spd", "Special Defense"}
        };
        public GameStateStorage GameState { get; set; }
        public List<Unit> SceneCharacters { get; set; }
        public List<Unit> Enemies { get; set; }
        public NPCHealthStatus CharactersStatus { get; set; }
        public BattleLog BattleLog { get; set; }

        public IEnumerator HandleAction(Action action)
        {
            if (action.SourceUnit.UnitName.Equals("Player"))
            {
                GameState.LevelLog.PlayerMovesUsed.Add(action.Move.Name);
            }

            var target = GetTarget(action);

            if (!action.SourceUnit || !target) yield break;
            yield return HandleAction(action, target);
        }
        
        private IEnumerator HandleAction(Action action, Unit target)
        {
            switch (action.Move.Category)
            {
                case "Status" when action.Move.Target.Equals("self") && action.Move.Secondary.ContainsKey("boosts"):
                {
                    yield return WaitForDelay(1f, 
                        action.SourceUnit.name + " used " + action.Move.Name + " on themselves");
                    yield return HandleStatus(action.Move, action.SourceUnit);
                    break;
                }
                case "Status" when !action.Move.Target.Equals("self") && action.Move.Secondary.ContainsKey("boosts"):
                {
                    yield return WaitForDelay(1f,
                        action.SourceUnit.name + " used " + action.Move.Name + " on " + target.name);
                    yield return HandleStatus(action.Move, target);
                    break;
                }
                default:
                {
                    var damageTaken = 0;
                    yield return HandleDamageTaken(action, target, x => damageTaken = x);
                    if (!target) yield break;
                    yield return HandleSelfBuff(action);
                    yield return HandleDebuff(action, target);
                    yield return HandleDrain(action, damageTaken);
                    yield return HandleStatusAilment(action, target);
                    break;
                }
            }
        }

        private void SetNewStat(int value, Unit character, string stat)
        {
            var statValue = (int) character.GetType()
                .GetProperties().ToList().Find(p => p.Name.ToLower().Equals(stat))
                .GetValue(character);
            print(stat);
            var originalStat = (int) character.OriginalStats.GetType().GetProperties().ToList()
                .Find(p => p.Name.ToLower().Equals(stat.ToLower())).GetValue(character.OriginalStats);
            var newStatValue = value > 0 ? statValue * (value + 2) / 2 : statValue * 2 / (-1 * value + 2);
            if (value < 0 && newStatValue >= originalStat / 4 || value > 0 && newStatValue <= 4 * originalStat) 
                character.GetType().GetProperties().ToList().Find(p => p.Name.ToLower().Equals(stat))
                    .SetValue(character, newStatValue);
        }

        private IEnumerator HandleStatus(Move move, Unit targetUnit)
        {
            if (!targetUnit) yield break;
            var boosts = ((JObject)move.Secondary["boosts"]).ToObject<Dictionary<string, int>>();
            foreach (var key in boosts.Keys)
            {
                if (key.Equals("accuracy") || key.Equals("evasion")) continue;
                var value = boosts[key];
                SetNewStat(value: value, character: targetUnit, stat: key);
                yield return WaitForDelay(1f, 
                    targetUnit.name + "'s " + _stats[key] + (value > 0 ? " was increased!" : " was decreased!"));
            }
        }
        
        
        
        private IEnumerator HandleDamageTaken(Action action, Unit target, Action<int> damageTaken)
        {
            var domainEffect = HandleDomain(action, target);
            var burnEffect = 
                action.SourceUnit.Ailments.Contains("brn") && action.Move.Category.Equals("Physical")
                    ? 0.5
                    : 1.0;
            var damageDealt = 0;
            if (!target.Affixes.Contains("HealthLink"))
            {
                damageDealt = target.TakeDamage(action.Move, action.SourceUnit, multiplier: domainEffect * burnEffect);
                damageTaken(damageDealt);
            }
            yield return WaitForDelay(1f,
                action.SourceUnit.name + " used " + action.Move.Name + " on " + target.name);
            CharactersStatus.UpdateHealthBar(target);
            
            var characters = SceneCharacters.
                Where(sc => sc.Ailments.Contains("HealthLink")).ToList();
            foreach (var character in characters)
            {
                damageDealt += character.TakeDamage(action.Move, action.SourceUnit, 
                    multiplier: Math.Round(1.0 / characters.Count, 2) * domainEffect * burnEffect);
                CharactersStatus.UpdateHealthBar(character);
            }
            damageTaken(damageDealt);
        }
        
        private double HandleDomain(Action action, Unit target)
        {
            var domainSource = Enemies.Find(e => e.Affixes.Contains("Domain"));
            if (!target.UnitName.Equals("Player")) return 1.0;
            if (domainSource is null) return 1.0;
            if (action.SourceUnit.UnitName.Equals("Player") &&
                target.DetermineMoveEffectiveness(action.Move.MoveType) > 1)
            {
                return 1.0 / target.DetermineMoveEffectiveness(action.Move.MoveType);
            }
            return domainSource.Types.Contains(action.Move.MoveType) ? 1.5 : 1.0;
        }
        
        private IEnumerator HandleDebuff(Action action, Unit target)
        {
            if (!action.Move.Secondary.ContainsKey("debuff")) yield break;
            var debuff = ((JObject) action.Move.Secondary["debuff"]).ToObject<Dictionary<string, dynamic>>();
            var randomNumber = new System.Random().Next(1, 100);
            if (debuff["chance"] < randomNumber) yield break;

            var boosts = ((JObject) debuff["boosts"]).ToObject<Dictionary<string, int>>();

            foreach (var key in boosts.Keys)
            {
                var value = boosts[key];
                if (key.Equals("accuracy") || key.Equals("evasion")) continue;
                SetNewStat(value, target, key);
                yield return WaitForDelay(1f,
                    _stats[key] + " has decreased for " + target.name);
            }
        }
        
        private IEnumerator HandleSelfBuff(Action action)
        {
            if (!action.Move.Secondary.ContainsKey("self-buff")) yield break;
            var selfBuff = ((JObject) action.Move.Secondary["self-buff"]).ToObject<SelfBuff>();
            var randomNumber = new System.Random().Next(1, 100);
            if (selfBuff.Chance < randomNumber) yield break;
        
            foreach (var key in selfBuff.Self["boosts"].Keys)
            {
                var value = selfBuff.Self["boosts"][key];
                if (key.Equals("accuracy") || key.Equals("evasion")) continue;
                SetNewStat(value, action.SourceUnit, key);
                yield return WaitForDelay(1f, 
                    _stats[key] + " has increased for " + action.SourceUnit.name);
            }
        }
        
        private IEnumerator HandleStatusAilment(Action action, Unit target)
        {
            if (!action.Move.Secondary.ContainsKey("status")) yield break;
            
            var random = new System.Random().Next(0, 100);
            var statusEffect = ((JObject) action.Move.Secondary["status"]).ToObject<Dictionary<string, string>>();
            if (random <= int.Parse(statusEffect["chance"]) && !target.Ailments.Contains("ReverseAilment"))
            {
                target.Ailments.Add(statusEffect["status"]);
                yield return WaitForDelay(1f,
                    target.name + " was affected by " + _ailments[statusEffect["status"]]);
                if (statusEffect["status"].Equals("par"))
                    target.Spe /= 2;
                CharactersStatus.GetPanels()[target].
                    GetComponent<AilmentIndicator>().ShowAilment(statusEffect["status"]);
            }
            else if (random <= int.Parse(statusEffect["chance"]))
            {
                action.SourceUnit.Ailments.Add(statusEffect["status"]);
                yield return WaitForDelay(1f,
                    action.SourceUnit.name + " was affected by " + _ailments[statusEffect["status"]]);
                if (statusEffect["status"].Equals("par"))
                    action.SourceUnit.Spe /= 2;
                CharactersStatus.GetPanels()[action.SourceUnit].
                    GetComponent<AilmentIndicator>().ShowAilment(statusEffect["status"]);
            }
            
        }
        
        private IEnumerator HandleDrain(Action action, int damageTaken)
        {
            if (!action.Move.Secondary.ContainsKey("drain")) yield break;
            var healValue = action.Move.Secondary["drain"] == 1 ? damageTaken / 2 : damageTaken * 3 / 4;
            healValue = action.SourceUnit.Affixes.Contains("Lifesteal") ? healValue + damageTaken / 2 : healValue;
            
            yield return WaitForDelay(1f, action.SourceUnit.name + " was healed!");
            action.SourceUnit.Heal(healValue);
            CharactersStatus.UpdateHealthBar(action.SourceUnit);

            if (!action.SourceUnit.Affixes.Contains("Medic")) yield break;
            foreach (var character in 
                     SceneCharacters.Where(character => !character.UnitName.Equals("Player") && 
                                                         !character.UnitName.Equals(action.SourceUnit.UnitName)))
            {
                // battleLog.ShowMessage(character.name + " was healed!");
                yield return WaitForDelay(1f, character.name + " was healed!");
                character.Heal(healValue);
                CharactersStatus.UpdateHealthBar(character);
            }

        }
        
        private static Unit GetTarget(Action action)
        {
            var target = action.TargetUnit;
            if (action.TargetUnit || !action.SourceUnit.UnitName.Equals("Player")) return target;
        
            var enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");
            var randomEnemy = new System.Random().Next(0, enemiesArray.Length);
            if (enemiesArray.Length > 0)
            {
                target = enemiesArray[randomEnemy].GetComponent<Unit>();
            }

            return target;
        }

        public IEnumerator HandleDamageOverTime()
        {
            foreach (var character in SceneCharacters.Where(character => character))
            {
                if (character.Ailments.Contains("brn"))
                {
                    yield return (WaitForDelay(1f, character.name + " was hurt by burn!"));
                    character.TakeDamage(character.CurrentHealth / 16);
                }

                if (character.Ailments.Contains("psn"))
                {
                    yield return (WaitForDelay(1f, character.name + " was hurt by poison!"));
                    character.TakeDamage(character.CurrentHealth / 8);
                }
            }
        }

        private IEnumerator WaitForDelay(float delay, string message)
        {
            BattleLog.ShowMessage(message);
            yield return new WaitForSeconds(delay);
        }
    }
}