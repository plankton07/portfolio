namespace AchievementSystem;

/// <summary>
/// 업적 진행도와 상태 전이를 처리하는 도메인 로직.
/// 저장소 및 보상 지급은 예제 범위에서 제외한다.
/// </summary>
public sealed class AchievementService
{
    private readonly IReadOnlyList<AchievementSpec> _specs;

    public AchievementService(IReadOnlyList<AchievementSpec> specs)
    {
        _specs = specs;
    }

    /// <summary>
    /// 행동 기록을 현재 업적 스펙에 반영한다.
    /// </summary>
    public void ApplySignal(UserAchievementData userData, AchievementSignal signal)
    {
        foreach (var spec in _specs)
        {
            if (!spec.IsActive || spec.Conditions.Count == 0)
            {
                continue;
            }

            // 완료된 업적은 진행도를 계속 누적하지만, 보상 수령 후에는 누적하지 않는다.
            if (!CanAccumulateProgress(userData, spec))
            {
                continue;
            }

            AchievementState? state = null;
            var isProgressChanged = false;

            for (var conditionIndex = 0; conditionIndex < spec.Conditions.Count; conditionIndex++)
            {
                var condition = spec.Conditions[conditionIndex];

                // 행동 기록이 해당 조건과 일치하는지 확인한다.
                if (!IsConditionMatched(condition, signal))
                {
                    continue;
                }

                // 조건에 해당하는 signal이 있을 때만 상태를 생성한다.
                state ??= GetOrCreateState(userData, spec);

                var progressChanged = TryAccumulateProgress(
                    state,
                    conditionIndex,
                    signal.Value,
                    signal.ProgressMode);

                if (!progressChanged)
                {
                    continue;
                }

                if (state.State == AchievementStateType.NotStarted)
                {
                    state.State = AchievementStateType.InProgress;
                }

                isProgressChanged = true;
            }

            // 진행도가 변경되지 않았다면 완료 여부를 다시 검사하지 않는다.
            if (!isProgressChanged || state == null)
            {
                continue;
            }

            if (IsCompletable(state, spec))
            {
                state.State = AchievementStateType.Completable;
            }
        }
    }

    /// <summary>
    /// 일일 / 주간 업적의 진행 상태를 주기 경계에서 초기화한다.
    /// 영구 업적은 초기화하지 않는다.
    /// </summary>
    public bool TryResetCycles(
        UserAchievementData userData,
        DateOnly today,
        DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        var isChanged = false;
        var weekStart = GetWeekStart(today, weekStartsOn);

        if (!userData.LastDailyResetDate.HasValue || userData.LastDailyResetDate.Value < today)
        {
            ResetType(userData, AchievementType.Daily);
            userData.LastDailyResetDate = today;
            isChanged = true;
        }

        if (!userData.LastWeeklyResetDate.HasValue || userData.LastWeeklyResetDate.Value < weekStart)
        {
            ResetType(userData, AchievementType.Weekly);
            userData.LastWeeklyResetDate = weekStart;
            isChanged = true;
        }

        return isChanged;
    }

    /// <summary>
    /// 지정한 요일을 기준으로 해당 주의 시작 날짜를 반환한다.
    /// </summary>
    private static DateOnly GetWeekStart(DateOnly date, DayOfWeek weekStartsOn)
    {
        var daysSinceWeekStart =
            ((int)date.DayOfWeek - (int)weekStartsOn + 7) % 7;

        return date.AddDays(-daysSinceWeekStart);
    }

    private bool ResetType(UserAchievementData userData, AchievementType type)
    {
        var isChanged = false;

        foreach (var spec in _specs)
        {
            if (spec.Type != type)
            {
                continue;
            }

            isChanged |= userData.States.Remove(spec.Id);
        }

        return isChanged;
    }

    /// <summary>
    /// 현재 업적 상태가 진행도를 누적할 수 있는 상태인지 확인한다.
    /// </summary>
    private static bool CanAccumulateProgress(UserAchievementData userData, AchievementSpec spec)
    {
        if (!userData.States.TryGetValue(spec.Id, out var state))
        {
            return true;
        }

        return state.State is AchievementStateType.NotStarted or AchievementStateType.InProgress or AchievementStateType.Completable;
    }

    /// <summary>
    /// 업적 상태를 가져오거나 새로 생성한다.
    /// </summary>
    private static AchievementState GetOrCreateState(UserAchievementData userData, AchievementSpec spec)
    {
        if (!userData.States.TryGetValue(spec.Id, out var state))
        {
            state = AchievementState.New(spec.Id, spec.Conditions.Count);
            userData.States[spec.Id] = state;
        }

        return state;
    }

    /// <summary>
    /// 조건 index의 진행도를 누적하거나 최댓값으로 갱신한다.
    /// </summary>
    private static bool TryAccumulateProgress(
        AchievementState state,
        int conditionIndex,
        float value,
        AchievementProgressMode progressMode)
    {
        if (conditionIndex < 0 || conditionIndex >= state.ProgressValues.Length)
        {
            return false;
        }

        var before = state.ProgressValues[conditionIndex];
        float after;

        switch (progressMode)
        {
            case AchievementProgressMode.Add:
                after = before + value;
                break;

            case AchievementProgressMode.Max:
                after = Math.Max(before, value);
                break;

            default:
                return false;
        }

        if (Math.Abs(after - before) <= float.Epsilon)
        {
            return false;
        }

        state.ProgressValues[conditionIndex] = after;
        return true;
    }

    /// <summary>
    /// 모든 완료 조건을 만족했는지 확인한다.
    /// </summary>
    private static bool IsCompletable(AchievementState state, AchievementSpec spec)
    {
        for (var i = 0; i < spec.Conditions.Count; i++)
        {
            var currentValue = i < state.ProgressValues.Length
                ? state.ProgressValues[i]
                : 0;

            if (currentValue < spec.Conditions[i].Goal)
            {
                return false;
            }
        }

        return spec.Conditions.Count > 0;
    }

    /// <summary>
    /// 행동 기록과 업적 조건이 일치하는지 확인한다.
    /// Target 0은 전체 대상을 의미한다.
    /// </summary>
    private static bool IsConditionMatched(AchievementCondition condition, AchievementSignal signal)
    {
        if (condition.Type != signal.Type)
        {
            return false;
        }

        if (condition.Target == 0)
        {
            return true;
        }

        return condition.Target > 0
            && condition.Target == signal.Target;
    }
}