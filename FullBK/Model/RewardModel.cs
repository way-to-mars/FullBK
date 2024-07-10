using System.Collections.ObjectModel;

namespace FullBK.Model;

public class RewardModel
{
    public int Id { get; set; }

    public RewardStates RewardState { get; set; } = RewardStates.UNKNOWN;

    public string tempString { get; set; } = string.Empty;

    public enum RewardStates {
        NEW = 1,
        INPROGRESS = 2,
        FAILED = 3,
        FINISHED = 4,
        UNKNOWN = -1,
    }

    public static RewardModel GetRandom()
    {
        Array values = Enum.GetValues(typeof(RewardStates));
        Random random = new Random();
        RewardStates randomState = (RewardStates?)values.GetValue(random.Next(values.Length)) ?? RewardStates.UNKNOWN;
        int randomId = random.Next(1000);

        return new RewardModel() { Id = randomId, RewardState = randomState };
    }
}

public class RewardGroup : ObservableCollection<RewardModel>
{
    public string GroupName { get; }
    public RewardModel.RewardStates State { get; private set; }

    public RewardGroup(RewardModel.RewardStates state, IEnumerable<RewardModel> rewards) : base(rewards)
    {
        State = state;
        GroupName = state.ToString();
    }
}

