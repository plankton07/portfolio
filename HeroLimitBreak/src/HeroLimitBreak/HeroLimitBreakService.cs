namespace HeroLimitBreak;

public sealed class HeroLimitBreakService
{
    private readonly IReadOnlyDictionary<Rarity, LimitBreakSpec> _specByRarity;

    public HeroLimitBreakService(IReadOnlyDictionary<Rarity, LimitBreakSpec> specByRarity)
    {
        _specByRarity = specByRarity;
    }

    public LimitBreakResult LimitBreak(Hero hero, ItemInventory inventory)
    {
        // SSR 이상은 한계 돌파할 수 없음
        if (hero.Rarity >= Rarity.Ssr)
        {
            return LimitBreakResult.AlreadyMaximum;
        }

        if (!_specByRarity.TryGetValue(hero.Rarity, out var spec)
            || spec.NextRarity <= hero.Rarity
            || spec.NextRarity > Rarity.Ssr
            || spec.AttackBonus < 0
            || spec.DefenseBonus < 0)
        {
            return LimitBreakResult.InvalidSpec;
        }

        // 동일한 아이템이 여러 번 정의되어 있으면 필요 수량을 합산
        var requiredItemCountById = new Dictionary<int, int>();
        foreach (var requiredItem in spec.RequiredItems)
        {
            if (requiredItem.ItemId <= 0 || requiredItem.Amount <= 0)
            {
                continue;
            }

            if (requiredItemCountById.TryGetValue(requiredItem.ItemId, out var amount))
            {
                requiredItemCountById[requiredItem.ItemId] = amount + requiredItem.Amount;
            }
            else
            {
                requiredItemCountById.Add(requiredItem.ItemId, requiredItem.Amount);
            }
        }

        // 재료가 부족하면 상태를 변경하지 않음
        foreach (var requiredItem in requiredItemCountById)
        {
            if (inventory.GetCount(requiredItem.Key) < requiredItem.Value)
            {
                return LimitBreakResult.NotEnoughItems;
            }
        }

        // 재료 아이템 소모
        inventory.UseItems(requiredItemCountById);

        // 한계 돌파 적용
        hero.LimitBreakLevel++;
        hero.Rarity = spec.NextRarity;
        hero.LimitBreakAttackBonus += spec.AttackBonus;
        hero.LimitBreakDefenseBonus += spec.DefenseBonus;

        // 변경된 영웅 상태를 기준으로 전투력을 재계산
        hero.CombatPower = HeroBattlePowerLogic.Calculate(hero);

        return LimitBreakResult.Success;
    }
}