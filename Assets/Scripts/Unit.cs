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

    void TakeDamage(int level, int attack, int power, bool magic)
    {
        int defenseUsed = magic ? magicResist : defense;

        double randomValue = new System.Random().NextDouble();

        currentHealth -= ((2 * level / 5 + 2) * power * attack / defenseUsed / 50 + 2) * (int) (randomValue * (UpperBound - LowerBound) + LowerBound);
    }

    bool IsDead()
    {
        return currentHealth > 0;
    }
}
