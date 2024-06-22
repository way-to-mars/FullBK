using FullBK.Model;

namespace FullBK.ViewModel;

public class RewardGroup : List<Reward>
{
    public string GroupName { get; private set; }

    public RewardGroup(string name, List<Reward> rewards) : base(rewards)
    {
        GroupName = name;
    }
}
