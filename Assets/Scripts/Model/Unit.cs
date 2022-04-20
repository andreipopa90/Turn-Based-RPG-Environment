using System.Collections;
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

    private void Start()
	{
        Moves = new();
        this.CurrentHealth = this.MaxHealth;
	}

    private int DetermineMoveEffectiveness(string MoveType)
	{
        int effectivness = 1;
        GameState = GameObject.Find("GameState").GetComponent<GameStateStorage>();
        List<Type> TypeChart = GameState.TypeChart;
        print("I have " + Types.Count + " types");
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

    public void SetStats(Dictionary<string, int> Stats, List<string> Types)
	{
        CurrentHealth = Stats["hp"];
        MaxHealth = Stats["hp"];
        Attack = Stats["atk"];
        Defense = Stats["def"];
        MagicAttack = Stats["spa"];
        MagicDefense = Stats["spd"];
        Speed = Stats["spe"];
        this.Types = Types;
	}

    bool IsDead()
    {
        return CurrentHealth <= 0;
    }

    
}
