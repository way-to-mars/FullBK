using System;
using System.Collections.Generic;
using System.Linq;

namespace FullBK.Model;

public class Reward
{
    public int Id { get; set; }

    public RewardStates RewardState { get; set; } = RewardStates.UNKNOWN;

    public enum RewardStates {
        NEW = 1,
        INPROGRESS = 2,
        FAILED = 3,
        FINISHED = 4,
        UNKNOWN = -1,
    }
}
