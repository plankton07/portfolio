using HeroLimitBreak;
using Xunit;

namespace HeroLimitBreak.Tests;

public sealed class HeroLimitBreakTests
{
    [Fact]
    public void LimitBreak_ConsumesItemsAndRecalculatesCombatPower()
    {
        var hero = new Hero
        {
            Id = 1,
            Rarity = Rarity.Normal,
            LimitBreakLevel = 0,
            BaseAttack = 40,
            BaseDefense = 10,
            CombatPower = 100,
        };

        var inventory = new ItemInventory();
        inventory.AddItem(1001, 2);
        inventory.AddItem(1002, 1);

        var service = CreateService();

        var result = service.LimitBreak(hero, inventory);

        Assert.Equal(LimitBreakResult.Success, result);
        Assert.Equal(Rarity.Rare, hero.Rarity);
        Assert.Equal(1, hero.LimitBreakLevel);
        Assert.Equal(5, hero.LimitBreakAttackBonus);
        Assert.Equal(105, hero.CombatPower);
        Assert.Equal(0, inventory.GetCount(1001));
        Assert.Equal(0, inventory.GetCount(1002));
    }

    [Fact]
    public void CalculateCombatPower_UsesCurrentHeroState()
    {
        var hero = new Hero
        {
            BaseAttack = 40,
            BaseDefense = 10,
            Level = 1,
            LimitBreakAttackBonus = 5,
            Equipments = [new Equipment { Attack = 3, Defense = 2 }],
            Skills = [new HeroSkill { Level = 2, CombatPowerPerLevel = 4 }],
            Attributes = new AttributeBonus { Attack = 1, Defense = 2 },
            PetLevel = 3,
        };

        var combatPower = HeroBattlePowerLogic.Calculate(hero);

        // Attack: 40 + 2 + 5 + 1 + 3 = 51
        // Defense: 10 + 1 + 2 + 2 = 15
        // Skill: 2 * 4 = 8, Pet: 3 * 3 = 9
        Assert.Equal(51 * 2 + 15 + 8 + 9, combatPower);
    }

    [Fact]
    public void LimitBreak_WhenItemsAreInsufficient_DoesNotChangeState()
    {
        var hero = new Hero
        {
            Id = 1,
            Rarity = Rarity.Normal,
            LimitBreakLevel = 0,
            CombatPower = 100,
        };

        var inventory = new ItemInventory();
        inventory.AddItem(1001, 1);
        inventory.AddItem(1002, 1);

        var service = CreateService();

        var result = service.LimitBreak(hero, inventory);

        Assert.Equal(LimitBreakResult.NotEnoughItems, result);
        Assert.Equal(Rarity.Normal, hero.Rarity);
        Assert.Equal(0, hero.LimitBreakLevel);
        Assert.Equal(100, hero.CombatPower);
        Assert.Equal(1, inventory.GetCount(1001));
        Assert.Equal(1, inventory.GetCount(1002));
    }

    [Fact]
    public void LimitBreak_WhenHeroIsSsr_DoesNotConsumeItems()
    {
        var hero = new Hero
        {
            Id = 1,
            Rarity = Rarity.Ssr,
            LimitBreakLevel = 3,
            CombatPower = 300,
        };

        var inventory = new ItemInventory();
        inventory.AddItem(1001, 10);

        var service = CreateService();

        var result = service.LimitBreak(hero, inventory);

        Assert.Equal(LimitBreakResult.AlreadyMaximum, result);
        Assert.Equal(3, hero.LimitBreakLevel);
        Assert.Equal(300, hero.CombatPower);
        Assert.Equal(10, inventory.GetCount(1001));
    }

    [Fact]
    public void LimitBreak_WhenRequirementsContainDuplicateItemIds_SumsRequiredAmount()
    {
        var hero = new Hero
        {
            Id = 1,
            Rarity = Rarity.Normal,
            LimitBreakLevel = 0,
            CombatPower = 100,
        };

        var inventory = new ItemInventory();
        inventory.AddItem(1001, 4);

        var specByRarity = new Dictionary<Rarity, LimitBreakSpec>
        {
            [Rarity.Normal] = new LimitBreakSpec
            {
                NextRarity = Rarity.Rare,
                AttackBonus = 5,
                DefenseBonus = 0,
                RequiredItems =
                [
                    new ItemRequirement(1001, 2),
                    new ItemRequirement(1001, 3),
                ],
            },
        };

        var service = new HeroLimitBreakService(specByRarity);

        var result = service.LimitBreak(hero, inventory);

        Assert.Equal(LimitBreakResult.NotEnoughItems, result);
        Assert.Equal(Rarity.Normal, hero.Rarity);
        Assert.Equal(4, inventory.GetCount(1001));
    }

    [Fact]
    public void LimitBreak_WhenSpecIsInvalid_DoesNotChangeState()
    {
        var hero = new Hero
        {
            Id = 1,
            Rarity = Rarity.Normal,
            LimitBreakLevel = 0,
            CombatPower = 100,
        };

        var inventory = new ItemInventory();
        inventory.AddItem(1001, 2);

        var specByRarity = new Dictionary<Rarity, LimitBreakSpec>
        {
            [Rarity.Normal] = new LimitBreakSpec
            {
                NextRarity = Rarity.Normal,
                AttackBonus = 5,
                DefenseBonus = 0,
                RequiredItems = [new ItemRequirement(1001, 2)],
            },
        };

        var service = new HeroLimitBreakService(specByRarity);

        var result = service.LimitBreak(hero, inventory);

        Assert.Equal(LimitBreakResult.InvalidSpec, result);
        Assert.Equal(Rarity.Normal, hero.Rarity);
        Assert.Equal(0, hero.LimitBreakLevel);
        Assert.Equal(100, hero.CombatPower);
        Assert.Equal(2, inventory.GetCount(1001));
    }

    private static HeroLimitBreakService CreateService()
    {
        var specByRarity = new Dictionary<Rarity, LimitBreakSpec>
        {
            [Rarity.Normal] = new LimitBreakSpec
            {
                NextRarity = Rarity.Rare,
                AttackBonus = 5,
                DefenseBonus = 0,
                RequiredItems =
                [
                    new ItemRequirement(1001, 2),
                    new ItemRequirement(1002, 1),
                ],
            },
        };

        return new HeroLimitBreakService(specByRarity);
    }
}