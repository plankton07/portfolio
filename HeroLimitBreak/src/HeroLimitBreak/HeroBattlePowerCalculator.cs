namespace HeroLimitBreak;

public sealed class HeroBattlePowerCalculator
{
    public int Calculate(Hero hero, int limitBreakAttackBonus, int limitBreakDefenseBonus)
    {
        var attack = hero.BaseAttack + hero.Level * 2 + limitBreakAttackBonus + hero.Attributes.Attack;
        var defense = hero.BaseDefense + hero.Level + limitBreakDefenseBonus + hero.Attributes.Defense;

        foreach (var equipment in hero.Equipments)
        {
            attack += equipment.Attack;
            defense += equipment.Defense;
        }

        var skillCombatPower = 0;
        foreach (var skill in hero.Skills)
        {
            skillCombatPower += skill.Level * skill.CombatPowerPerLevel;
        }

        var petCombatPower = hero.PetLevel * 3;

        return attack * 2 + defense + skillCombatPower + petCombatPower;
    }
}