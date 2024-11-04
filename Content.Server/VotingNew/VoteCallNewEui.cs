using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.GameTicking.Presets;
using Content.Shared.Administration;
using Content.Shared.VotingNew;
using Content.Shared.Eui;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.VotingNew;

public sealed class VoteCallNewEui : BaseEui
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;

    public VoteCallNewEui()
    {
        IoCManager.InjectDependencies(this);
    }

    public override void Opened()
    {
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        Dictionary<string, string> presets = new();
        foreach (var preset in _prototypeManager.EnumeratePrototypes<GamePresetPrototype>())
        {
            if(!preset.ShowInVote)
                continue;

            if(_playerManager.PlayerCount < (preset.MinPlayers ?? int.MinValue))
                continue;

            if(_playerManager.PlayerCount > (preset.MaxPlayers ?? int.MaxValue))
                continue;

            presets[preset.ID] = Loc.GetString(preset.ModeTitle);
        }
        return new VoteCallNewEuiState(presets);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case VoteCallNewEuiMsg.DoVote doVote:
                if (!_adminManager.HasAdminFlag(Player, AdminFlags.Admin))
                {
                    Close();
                    break;
                }

                _consoleHost.RemoteExecuteCommand(Player, $"createvote Preset {doVote.TargetPresetList}");
                break;
        }
    }
}
