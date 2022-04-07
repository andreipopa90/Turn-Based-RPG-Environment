using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private const double UpperBound = 1.00;
    private const double LowerBound = 0.85;
    public string unitName;
    public int maxHealth;
    public int currentHealth;
    public int attack;
    public int defense;
    public int magicAttack;
    public int magicResist;
    public int speed;
    public int level;
    public List<Move> Moves;

    private void Start()
	{
        Moves = new();
        this.currentHealth = this.maxHealth;
	}

	public void TakeDamage(int levelEnemy, int attackEnemy, int powerEnemy, bool magic)
    {
        int defenseUsed = magic ? magicResist : defense;

        double randomValue = new System.Random().NextDouble();

        this.currentHealth -= (int) (((2 * levelEnemy / 5 + 2) * powerEnemy * attackEnemy /
            defenseUsed / 50 + 2) * (randomValue *
            (UpperBound - LowerBound) + LowerBound));
        if (IsDead())
		{
			Destroy(GameObject.Find(unitName));
		}
    }

    public void TakeDamage(Move Move, Unit EnemySource)
    {
        int DefenseUsed = Move.category.Equals("Sepcial") ? magicResist : defense;
        int SourceAttackUsed = Move.category.Equals("Special") ? EnemySource.magicAttack : EnemySource.attack;
        double RandomValue = new System.Random().NextDouble() * (UpperBound - LowerBound) + LowerBound;

        int DamageTaken = (int) ((((2 * EnemySource.level / 5 + 2) * (SourceAttackUsed / DefenseUsed) * Move.basePower) / 50 + 2) * RandomValue);

        currentHealth -= DamageTaken;
    }

    bool IsDead()
    {
        return currentHealth <= 0;
    }

    
}
