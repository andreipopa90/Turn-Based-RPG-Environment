using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private GameStateStorage GameState;
    private const double UpperBound = 1.00;
    private const double LowerBound = 0.85;
    public List<string> Types;
    public string UnitName;
    public int MaxHealth;
    public int CurrentHealth;
    public int Attack;
    public int Defense;
    public int MagicAttack;
    public int MagicDefense;
    public int Speed;
    public int Level;
    public List<Move> Moves;
    public Dictionary<string, int> IV;
    public Nature UnitNature;

    void Start()
	{
        Moves = new();
        CurrentHealth = MaxHealth;
        GenerateIVs();
	}

    void GenerateIVs()
    {
        IV = new();
        for (int i = 0; i < 6; i++)
        {
            int RandomNumber = new System.Random().Next(0, 32);
            switch (i)
            {
                case 0:
                    IV["hp"] = RandomNumber;
                    break;
                case 1:
                    IV["atk"] = RandomNumber;
                    break;
                case 2:
                    IV["def"] = RandomNumber;
                    break;
                case 3:
                    IV["spa"] = RandomNumber;
                    break;
                case 4:
                    IV["spd"] = RandomNumber;
                    break;
                case 5:
                    IV["spe"] = RandomNumber;
                    break;
            }
        }
    }

    private int DetermineMoveEffectiveness(string MoveType)
	{
        int effectivness = 1;
        GameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
        List<Type> TypeChart = GameState.TypeChart;
        foreach(string type in Types)
		{
            Dictionary<string, int> damageTaken = TypeChart.Find(x => x.Name.Equals(type.ToLower())).DamageTaken;
            switch(damageTaken[MoveType])
			{
                case 0:
                    effectivness *= 1;
                    break;
                case 1:
                    effectivness *= 2;
                    break;
                case 2:
                    effectivness /= 2;
                    break;
                default:
                    effectivness *= 0;
                    break;
			}
		}
        return effectivness;
	}

    public void TakeDamage(Move Move, Unit EnemySource)
    {
        int Effectiveness = DetermineMoveEffectiveness(Move.Type);

        int DefenseUsed = Move.Category.Equals("Sepcial") ? MagicDefense : Defense;
        int SourceAttackUsed = Move.Category.Equals("Special") ? EnemySource.MagicAttack : EnemySource.Attack;
        double RandomValue = new System.Random().NextDouble() * (UpperBound - LowerBound) + LowerBound;

        int DamageTaken = (int) ((((2 * EnemySource.Level / 5 + 2) * (SourceAttackUsed / DefenseUsed) * Move.BasePower) / 50 + 2) * RandomValue);

        DamageTaken *= Effectiveness;

        CurrentHealth -= DamageTaken;
        if (IsDead())
        {
            Destroy(this.gameObject);
        }
    }

    int CalculateHP(int BaseHP)
    {
        int HP = (int) System.Math.Floor((2 * BaseHP + IV["hp"]) * Level / 100f) + Level + 10;
        return HP;
    }

    int CalculateStats(int BaseStat, string stat)
    {
        print(stat);
        double NatureModifier = 1;
        if (stat.Equals(UnitNature.Plus)) NatureModifier = 1.1;
        else if (stat.Equals(UnitNature.Minus)) NatureModifier = 0.9;
        double  Stat = (System.Math.Floor((2 * BaseStat) * Level / 100d) + 5) * NatureModifier;
        print(Stat);
        return 0;
    }

    public void SetStats(Dictionary<string, int> Stats)
	{
        CurrentHealth = Stats["hp"]; // CalculateHP(Stats["hp"]);
        MaxHealth = CurrentHealth;

        Attack = CalculateStats(Stats["atk"], "atk");
        //Defense = CalculateStats(Stats["def"], "def");
        //MagicAttack = CalculateStats(Stats["spa"], "spa");
        //MagicDefense = CalculateStats(Stats["spd"], "spd");
        //Speed = CalculateStats(Stats["spe"], "spe");

        //Attack = Stats["atk"];
        Defense = Stats["def"];
        MagicAttack = Stats["spa"];
        MagicDefense = Stats["spd"];
        Speed = Stats["spe"];
    }

    bool IsDead()
    {
        return CurrentHealth <= 0;
    }

    
}
