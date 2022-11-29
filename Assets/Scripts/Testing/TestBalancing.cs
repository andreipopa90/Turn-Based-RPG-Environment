using System;
using System.Collections.Generic;
using System.Linq;
using JsonParser;
using Model;
using UnityEngine;

namespace Testing
{
    public class TestBalancing : MonoBehaviour
    {
        
        [SerializeField] private GameStateStorage gameStateStorage;
        private readonly Unit _player = new Unit();
        private readonly Unit _enemy1 = new Unit();
        private readonly Unit _enemy2 = new Unit();
        private readonly Unit _enemy3 = new Unit();
        private Move _arceusMove;
        private JSONReader _reader;
        private List<BaseStat> _baseStats;

        private void StartState()
        {
            _player._gameState = gameStateStorage;
            _enemy1._gameState = gameStateStorage;
            _enemy2._gameState = gameStateStorage;
            _enemy3._gameState = gameStateStorage;
            _player.Ailments = new List<string>();
            _enemy1.Ailments = new List<string>();
            _enemy2.Ailments = new List<string>();
            _enemy3.Ailments = new List<string>();
            _player.Affixes = new List<string>();
            _enemy1.Affixes = new List<string>();
            _enemy2.Affixes = new List<string>();
            _enemy3.Affixes = new List<string>();
            var nature = new Nature
            {
                Plus = string.Empty,
                Minus = string.Empty
            };
            _player.unitNature = nature;
            _enemy1.unitNature = nature;
            _enemy1.unitNature = nature;
            _enemy2.unitNature = nature;
            _enemy3.unitNature = nature;
            var arceusLearnSet = gameStateStorage.StartMoves
                .Find(sm => sm.Name.Equals("arceus")).LearnSet;
            _arceusMove = gameStateStorage.AllMoves
                .Where(m => arceusLearnSet.Contains(m.KeyName)).ToList()
                .OrderByDescending(m => m.BasePower).First();
            var ev = new Dictionary<string, int>()
            {
                {"hp", 0},
                {"atk", 0},
                {"def", 0},
                {"spa", 0},
                {"spd", 0},
                {"spe", 0}
            };
            _player._iv = ev;
            _enemy1._iv = ev;
            _enemy2._iv = ev;
            _enemy3._iv = ev;
            _player.Ev = ev;
            _enemy1.Ev = ev;
            _enemy2.Ev = ev;
            _enemy3.Ev = ev;
            _player.Level = 11;
            _enemy1.Level = 5;
            _enemy2.Level = 5;
            _enemy3.Level = 5;
            var playerBase = _baseStats.Find(b => b.KeyName.Equals("arceusgrass")).Clone();
            var enemyBase = _baseStats.Find(b => b.KeyName.Equals("arceusfire")).Clone();
            _player.OriginalStats = (BaseStat) playerBase;
            _enemy1.OriginalStats = (BaseStat) enemyBase;
            _enemy2.OriginalStats = (BaseStat) enemyBase;
            _enemy3.OriginalStats = (BaseStat) enemyBase;
            _player.Types = _player.OriginalStats.Types;
            _enemy1.Types = _enemy1.OriginalStats.Types;
            _enemy2.Types = _enemy2.OriginalStats.Types;
            _enemy3.Types = _enemy3.OriginalStats.Types;
            _player.SetStats(_player.OriginalStats);
            _enemy1.SetStats(_enemy1.OriginalStats);
            _enemy2.SetStats(_enemy2.OriginalStats);
            _enemy3.SetStats(_enemy3.OriginalStats);
        }

        private void ReduceEnemyStats(int step)
        {
            // var newStats = _baseStats.Find(b => b.KeyName.Equals("arceuswater")).Clone();
            // _player.OriginalStats = (BaseStat) newStats;
            _player.SetStats(_player.OriginalStats);
            
            _enemy1.OriginalStats.Hp = Math.Max(1, _enemy1.OriginalStats.Hp - step) ;
            _enemy1.OriginalStats.Atk = Math.Max(1, _enemy1.OriginalStats.Atk - step);
            _enemy1.OriginalStats.Def = Math.Max(1, _enemy1.OriginalStats.Def - step);
            _enemy1.OriginalStats.Spa = Math.Max(1, _enemy1.OriginalStats.Spa - step);
            _enemy1.OriginalStats.Spd = Math.Max(1, _enemy1.OriginalStats.Spd - step);
            _enemy1.OriginalStats.Spe = Math.Max(1, _enemy1.OriginalStats.Spe - step);
            _enemy1.SetStats(_enemy1.OriginalStats);
            
            _enemy2.OriginalStats.Hp = Math.Max(1, _enemy2.OriginalStats.Hp - step) ;
            _enemy2.OriginalStats.Atk = Math.Max(1, _enemy2.OriginalStats.Atk - step);
            _enemy2.OriginalStats.Def = Math.Max(1, _enemy2.OriginalStats.Def - step);
            _enemy2.OriginalStats.Spa = Math.Max(1, _enemy2.OriginalStats.Spa - step);
            _enemy2.OriginalStats.Spd = Math.Max(1, _enemy2.OriginalStats.Spd - step);
            _enemy2.OriginalStats.Spe = Math.Max(1, _enemy2.OriginalStats.Spe - step);
            _enemy2.SetStats(_enemy2.OriginalStats);
            
            _enemy3.OriginalStats.Hp = Math.Max(1, _enemy3.OriginalStats.Hp - step) ;
            _enemy3.OriginalStats.Atk = Math.Max(1, _enemy3.OriginalStats.Atk - step);
            _enemy3.OriginalStats.Def = Math.Max(1, _enemy3.OriginalStats.Def - step);
            _enemy3.OriginalStats.Spa = Math.Max(1, _enemy3.OriginalStats.Spa - step);
            _enemy3.OriginalStats.Spd = Math.Max(1, _enemy3.OriginalStats.Spd - step);
            _enemy3.OriginalStats.Spe = Math.Max(1, _enemy3.OriginalStats.Spe - step);
            _enemy3.SetStats(_enemy3.OriginalStats);
        }

        private void Start()
        {
            _reader = new JSONReader();
            _baseStats = _reader.ReadBaseStatsJson();
            var enemies = new List<Unit>
            {
                _enemy1, _enemy2, _enemy3
            };
            var average = 0f;
            for (var index = 1; index <= 100; index++)
            {
                StartState();
                var lost = 0;
                var step = 1;
                var winMessage = "Enemy won!";
                while (!winMessage.Equals("Player won!"))
                {
                    while ((_enemy1.CurrentHealth > 0 
                            || _enemy2.CurrentHealth > 0 
                            || _enemy3.CurrentHealth > 0)
                           && _player.CurrentHealth > 0 )
                    {
                        var target = enemies.Find(e => e.CurrentHealth > 0);
                        if (_enemy1.CurrentHealth > 0)
                            _player.TakeDamage(_arceusMove, _enemy1);
                        if (_enemy2.CurrentHealth > 0)
                            _player.TakeDamage(_arceusMove, _enemy2);
                        if (_enemy3.CurrentHealth > 0)
                            _player.TakeDamage(_arceusMove, _enemy3);
                        target.TakeDamage(_arceusMove, _player);
                    }
            
                    if (_player.CurrentHealth < 0)
                    {
                        print(step);
                        lost += 1;
                        step *= 10;
                        ReduceEnemyStats(step);
                    }
                    else
                    {
                        winMessage = "Player won!";
                    }
                }
                average += lost;
            }
            
            print("Average of lost games is " + average / 100);
        }
    }
}