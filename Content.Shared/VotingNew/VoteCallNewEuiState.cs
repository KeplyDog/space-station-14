using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.VotingNew;

[Serializable, NetSerializable]
public sealed class VoteCallNewEuiState : EuiStateBase
{
    public readonly Dictionary<string, string> Presets;

    public VoteCallNewEuiState(Dictionary<string, string> presets)
    {
        Presets = presets;
    }
}

public static class VoteCallNewEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class DoVote : EuiMessageBase
    {
        public List<string> TargetPresetList = new();
    }
}
