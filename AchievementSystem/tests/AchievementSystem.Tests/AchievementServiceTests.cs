using AchievementSystem;
using Xunit;

namespace AchievementSystem.Tests;

public sealed class AchievementServiceTests
{
    [Fact]
    public void ApplySignal_AccumulatesProgress_AndCompletesAchievement()
    {
        var spec = new AchievementSpec
        {
            Id = 100,
            Type = AchievementType.Permanent,
            Conditions =
            [
                new AchievementCondition
                {
                    Type = AchievementConditionType.MonsterKill,
                    Target = 10,
                    Goal = 3,
                },
            ],
        };

        var service = new AchievementService([spec]);
        var userData = new UserAchievementData();

        service.ApplySignal(userData, new AchievementSignal
        {
            Type = AchievementConditionType.MonsterKill,
            Target = 10,
            Value = 2,
        });

        Assert.Equal(AchievementStateType.InProgress, userData.States[100].State);
        Assert.Equal(2f, userData.States[100].ProgressValues[0]);

        service.ApplySignal(userData, new AchievementSignal
        {
            Type = AchievementConditionType.MonsterKill,
            Target = 10,
            Value = 1,
        });

        Assert.Equal(AchievementStateType.Completable, userData.States[100].State);
        Assert.Equal(3f, userData.States[100].ProgressValues[0]);
    }

    [Fact]
    public void ApplySignal_DoesNotCreateState_WhenSignalDoesNotMatch()
    {
        var spec = new AchievementSpec
        {
            Id = 101,
            Type = AchievementType.Permanent,
            Conditions =
            [
                new AchievementCondition
                {
                    Type = AchievementConditionType.StageClear,
                    Target = 5,
                    Goal = 1,
                },
            ],
        };

        var service = new AchievementService([spec]);
        var userData = new UserAchievementData();

        service.ApplySignal(userData, new AchievementSignal
        {
            Type = AchievementConditionType.StageClear,
            Target = 8,
        });

        Assert.Empty(userData.States);
    }

    [Fact]
    public void ApplySignal_TargetZero_MatchesAnyTarget()
    {
        var spec = new AchievementSpec
        {
            Id = 102,
            Type = AchievementType.Permanent,
            Conditions =
            [
                new AchievementCondition
                {
                    Type = AchievementConditionType.MonsterKill,
                    Target = 0,
                    Goal = 2,
                },
            ],
        };

        var service = new AchievementService([spec]);
        var userData = new UserAchievementData();

        service.ApplySignal(userData, new AchievementSignal
        {
            Type = AchievementConditionType.MonsterKill,
            Target = 999,
            Value = 2,
        });

        Assert.Equal(AchievementStateType.Completable, userData.States[102].State);
    }

    [Fact]
    public void ApplySignal_MaxMode_DoesNotDecreaseProgress()
    {
        var spec = new AchievementSpec
        {
            Id = 103,
            Type = AchievementType.Permanent,
            Conditions =
            [
                new AchievementCondition
                {
                    Type = AchievementConditionType.StageClear,
                    Target = 1,
                    Goal = 10,
                },
            ],
        };

        var service = new AchievementService([spec]);
        var userData = new UserAchievementData();

        service.ApplySignal(userData, new AchievementSignal
        {
            Type = AchievementConditionType.StageClear,
            Target = 1,
            Value = 7,
            ProgressMode = AchievementProgressMode.Max,
        });

        service.ApplySignal(userData, new AchievementSignal
        {
            Type = AchievementConditionType.StageClear,
            Target = 1,
            Value = 4,
            ProgressMode = AchievementProgressMode.Max,
        });

        Assert.Equal(7f, userData.States[103].ProgressValues[0]);
    }

    [Fact]
    public void ApplySignal_DoesNotAccumulateClaimedAchievement()
    {
        var spec = new AchievementSpec
        {
            Id = 104,
            Type = AchievementType.Permanent,
            Conditions =
            [
                new AchievementCondition
                {
                    Type = AchievementConditionType.MonsterKill,
                    Target = 1,
                    Goal = 10,
                },
            ],
        };

        var service = new AchievementService([spec]);
        var userData = new UserAchievementData();

        userData.States[104] = new AchievementState
        {
            AchievementId = 104,
            State = AchievementStateType.Claimed,
            ProgressValues = [5],
        };

        service.ApplySignal(userData, new AchievementSignal
        {
            Type = AchievementConditionType.MonsterKill,
            Target = 1,
            Value = 1,
        });

        Assert.Equal(5f, userData.States[104].ProgressValues[0]);
        Assert.Equal(AchievementStateType.Claimed, userData.States[104].State);
    }

    [Fact]
    public void TryResetCycles_ResetsDailyAndWeekly_ButKeepsPermanent()
    {
        var specs = new List<AchievementSpec>
        {
            new()
            {
                Id = 1,
                Type = AchievementType.Daily,
                Conditions =
                [
                    new AchievementCondition
                    {
                        Type = AchievementConditionType.MonsterKill,
                        Goal = 1,
                    },
                ],
            },
            new()
            {
                Id = 2,
                Type = AchievementType.Weekly,
                Conditions =
                [
                    new AchievementCondition
                    {
                        Type = AchievementConditionType.StageClear,
                        Goal = 1,
                    },
                ],
            },
            new()
            {
                Id = 3,
                Type = AchievementType.Permanent,
                Conditions =
                [
                    new AchievementCondition
                    {
                        Type = AchievementConditionType.ItemAcquire,
                        Goal = 1,
                    },
                ],
            },
        };

        var service = new AchievementService(specs);
        var userData = new UserAchievementData();

        userData.States[1] = AchievementState.New(1, 1);
        userData.States[2] = AchievementState.New(2, 1);
        userData.States[3] = AchievementState.New(3, 1);

        var changed = service.TryResetCycles(
            userData,
            new DateOnly(2026, 10, 29));

        Assert.True(changed);
        Assert.DoesNotContain(1, userData.States.Keys);
        Assert.DoesNotContain(2, userData.States.Keys);
        Assert.Contains(3, userData.States.Keys);
    }

    [Fact]
    public void TryResetCycles_ReturnsFalse_WhenAlreadyResetForCurrentCycle()
    {
        var specs = new List<AchievementSpec>
        {
            new()
            {
                Id = 1,
                Type = AchievementType.Daily,
                Conditions =
                [
                    new AchievementCondition
                    {
                        Type = AchievementConditionType.MonsterKill,
                        Goal = 1,
                    },
                ],
            },
        };

        var service = new AchievementService(specs);
        var userData = new UserAchievementData();

        userData.LastDailyResetDate = new DateOnly(2026, 10, 29);
        userData.LastWeeklyResetDate = new DateOnly(2026, 10, 26);

        var changed = service.TryResetCycles(
            userData,
            new DateOnly(2026, 10, 29));

        Assert.False(changed);
    }
}
