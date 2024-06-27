using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FullBK.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FullBK.ViewModel;

public partial class GroupingCollectionViewModel : ObservableObject
{
    private List<RewardModel> _rewardsRaw = new List<RewardModel>();

    public ObservableCollection<RewardGroup> GroupedRewards { get; private set; } = new();

    [ObservableProperty]
    int columnsCount = 1;

    public GroupingCollectionViewModel()
    {
        PopulateRawList(_rewardsRaw);
        PopulateGroupedList(GroupedRewards, _rewardsRaw);
    }

    private static void PopulateGroupedList(ObservableCollection<RewardGroup> list, List<RewardModel> raw)
    {
        var groups = raw
            .GroupBy(rew => rew.RewardState)
            .Select(group => new RewardGroup(state: group.Key, rewards: group));
        list.Clear();
        foreach(var group in groups) list.Add(group);  
        Debug.WriteLine(list.Count);
    }

    private static void PopulateRawList(List<RewardModel> list)
    {
        list.Clear();
        foreach (var it in Enumerable.Range(1, 4))
        {
            list.Add(new RewardModel
            { 
                Id = it,
                RewardState = RewardModel.RewardStates.NEW,
            });
        }
        foreach (var it in Enumerable.Range(11, 5))
        {
            list.Add(new RewardModel
            {
                Id = it,
                RewardState = RewardModel.RewardStates.FINISHED,
            });
        }
    }

    [RelayCommand]
    void ItemTapped(RewardModel reward)
    {
        Debug.WriteLine($"{reward.RewardState}, id={reward.Id}");



        GroupedRewards.AddReward(RewardModel.GetRandom());
    }
}

