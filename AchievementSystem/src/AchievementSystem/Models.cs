namespace AchievementSystem;

public enum AchievementType
{
    Daily,
    Weekly,
    Permanent,
}

public enum AchievementStateType
{
    NotStarted,
    InProgress,
    Completable,
    Claimed,
}

public enum AchievementConditionType
{
    MonsterKill,
    StageClear,
    ItemAcquire,
}

public enum AchievementProgressMode
{
    Add,
    Max,
}

public sealed class AchievementCondition
{
    public AchievementConditionType Type { get; init; }
    public int Target { get; init; }
    public float Goal { get; init; }
}

public sealed class AchievementSpec
{
    public int Id { get; init; }
    public AchievementType Type { get; init; }
    public bool IsActive { get; init; } = true;
    public List<AchievementCondition> Conditions { get; init; } = [];
}

public sealed class AchievementSignal
{
    public AchievementConditionType Type { get; init; }
    public int Target { get; init; }
    public float Value { get; init; } = 1;
    public AchievementProgressMode ProgressMode { get; init; } = AchievementProgressMode.Add;
}

public sealed class AchievementState
{
    public int AchievementId { get; init; }
    public AchievementStateType State { get; set; }
    public float[] ProgressValues { get; set; } = [];

    public static AchievementState New(int achievementId, int conditionCount)
    {
        return new AchievementState
        {
            AchievementId = achievementId,
            State = AchievementStateType.NotStarted,
            ProgressValues = new float[conditionCount],
        };
    }
}

public sealed class UserAchievementData
{
    public Dictionary<int, AchievementState> States { get; } = [];
    public DateOnly? LastDailyResetDate { get; set; }
    public DateOnly? LastWeeklyResetDate { get; set; }
}
