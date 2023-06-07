using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demon
{
    public DemonBase Base { get; set; }
    public int Level { get; set; }

    public int HP { get; private set; }

    public List<Move> Moves {  get; private set; } 

    public Demon(DemonBase demonBase, int demonLevel)
    {
        Base = demonBase;
        Level = demonLevel;
        HP = MaxHp;

        Moves = new List<Move>();

        foreach (var learnableMove in Base.LearnableMoves)
        {
            if (learnableMove.Level <= Level)
            {
                Moves.Add(new Move(learnableMove.Base));
            }

            if (Moves.Count > 4)
            {
                break;
            }
        }
    }

    public int Attack
    {
        get { return formulaLevelStat(Base.Attack, Level); }
    }

    public int Defense
    {
        get { return formulaLevelStat(Base.Defense, Level); }
    }

    public int SpAttack
    {
        get { return formulaLevelStat(Base.SpAttack, Level); }
    }

    public int SpDefense
    {
        get { return formulaLevelStat(Base.SpDefense, Level); }
    }

    public int Speed
    {
        get { return formulaLevelStat(Base.Speed, Level); }
    }

    public int MaxHp
    {
        get { return formulaLevelStat(Base.MaxHP, Level) + 5; }
    }

    private int formulaLevelStat(int stat, int level)
    {
        return Mathf.FloorToInt((stat * level) / 100f) + 5;
    }

    public DamageDetails TakeDamage(Move move, Demon attacker)
    {
        float criticalHit = 1.0f;
        if (Random.value * 100f <= 6.25)
            criticalHit = 2.0f;

        float typeEffectiveness = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = typeEffectiveness,
            Critical = criticalHit,
            Fainted = false,
        };

        float modifiers = Random.Range(0.85f, 1f) * typeEffectiveness * criticalHit;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}

public class DamageDetails
{
    public bool Fainted { get; set;}
    public float Critical { get; set;}
    public float TypeEffectiveness { get; set;}
}