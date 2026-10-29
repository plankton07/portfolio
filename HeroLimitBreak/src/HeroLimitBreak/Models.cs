namespace HeroLimitBreak;

public enum Rarity
{
    Normal = 0,
    Rare = 1,
    Sr = 2,
    Ssr = 3,
}

public enum LimitBreakResult
{
    Success,
    AlreadyMaximum,
    NotEnoughItems,
    InvalidSpec,
}

public readonly record struct ItemRequirement(int ItemId, int Amount);

public sealed class LimitBreakSpec
{
    public Rarity NextRarity { get; init; }
    public int AttackBonus { get; init; }
    public int DefenseBonus { get; init; }
    public List<ItemRequirement> RequiredItems { get; init; } = [];
}

public sealed class Hero
{
    public int Id { get; init; }
    public Rarity Rarity { get; set; }
    public int Level { get; set; } = 1;
    public int LimitBreakLevel { get; set; }
    public int BaseAttack { get; set; }
    public int BaseDefense { get; set; }
    public int LimitBreakAttackBonus { get; set; }
    public int LimitBreakDefenseBonus { get; set; }
    public int CombatPower { get; set; }
    public List<Equipment> Equipments { get; init; } = [];
    public List<HeroSkill> Skills { get; init; } = [];
    public AttributeBonus Attributes { get; init; } = new();
    public int PetLevel { get; set; }
}

public sealed class Equipment
{
    public int Attack { get; init; }
    public int Defense { get; init; }
}

public sealed class HeroSkill
{
    public int Level { get; init; }
    public int CombatPowerPerLevel { get; init; }
}

public sealed class AttributeBonus
{
    public int Attack { get; set; }
    public int Defense { get; set; }
}
