using System.Collections;
using System.Collections.Generic;


public class Attack
{
	


	public int Level;
	public int Power;
	public int AttackPower;
	public int Speed;
	public string Target;

	public Attack(int level, int power, int attackPower, int speed)
	{
		Level = level;
		Power = power;
		AttackPower = attackPower;
		Speed = speed;
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
