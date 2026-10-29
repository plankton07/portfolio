namespace HeroLimitBreak;

public static class HeroBattlePowerLogic
{
    public static int Calculate(Hero hero)
    {
        var attack = hero.BaseAttack + hero.Level * 2 + hero.LimitBreakAttackBonus + hero.Attributes.Attack;
        var defense = hero.BaseDefense + hero.Level + hero.LimitBreakDefenseBonus + hero.Attributes.Defense;

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