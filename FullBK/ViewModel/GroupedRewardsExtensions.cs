
using FullBK.Model;
using System.Collections.ObjectModel;

namespace FullBK.ViewModel;

public static class GroupedRewardsExtensions
{
    public static void AddReward(this ObservableCollection<RewardGroup> grouped, RewardModel reward)
    {
        foreach (var item in grouped)
            if (item.State == reward.RewardState)
            {
                item.Add(reward);
                return;
            }

        grouped.Add(new RewardGroup(reward.RewardState, [reward]));
    }
}
