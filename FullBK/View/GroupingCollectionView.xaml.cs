using FullBK.Model;
using FullBK.ViewModel;

namespace FullBK.View;

public partial class GroupingCollectionView : ContentPage
{
    // ObservableCollection should be used
    public List<RewardGroup> Rewards { get; set; } = new List<RewardGroup>();

    public GroupingCollectionView()
    {
        InitializeComponent();
        PopulateList(Rewards);
        BindingContext = this;
    }

    private void PopulateList(List<RewardGroup> list)
    {
        list.Add(new RewardGroup(
            "New",
            Enumerable.Range(1, 3).Select(i => new Reward
            {
                Id = i,
                RewardState = Reward.RewardStates.NEW
            }).ToList()
        ));
        list.Add(new RewardGroup(
            "Done",
            Enumerable.Range(11, 5).Select(i => new Reward
            {
                Id = i,
                RewardState = Reward.RewardStates.FINISHED
            }).ToList()
        ));
    }
}