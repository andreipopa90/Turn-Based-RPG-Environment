using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private const double UpperBound = 1.00;
    private const double LowerBound = 0.85;
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

    public void TakeDamage(Move Move, Unit EnemySource)
    {
        int DefenseUsed = Move.Category.Equals("Sepcial") ? MagicDefense : Defense;
        int SourceAttackUsed = Move.Category.Equals("Special") ? EnemySource.MagicAttack : EnemySource.Attack;
        double RandomValue = new System.Random().NextDouble() * (UpperBound - LowerBound) + LowerBound;

        int DamageTaken = (int) ((((2 * EnemySource.Level / 5 + 2) * (SourceAttackUsed / DefenseUsed) * Move.BasePower) / 50 + 2) * RandomValue);

        CurrentHealth -= DamageTaken;
        if (IsDead())
        {
            Destroy(this.gameObject);
        }
    }

    bool IsDead()
    {
        return CurrentHealth <= 0;
    }

    
}
