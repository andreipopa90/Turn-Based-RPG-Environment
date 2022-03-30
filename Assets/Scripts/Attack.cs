using System.Collections;
using System.Collections.Generic;


public class Attack
{
	public Move Move;
	public int Speed;
	public int Level;
	public string Target;
	public int UnitAttack;

	public Attack(Move move, int speed, int level, int attack)
	{
		this.Move = move;
		Speed = speed;
		Level = level;
		UnitAttack = attack;
	}

	public void SetTarget(string target)
	{
		this.Target = target;
	}

	public string GetTarget()
	{
		return Target;
	}
}
