using System.Linq;
using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared.VotingNew;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.VotingNew.UI;

public sealed class VoteCallNewEui : BaseEui
{
    private readonly VoteCallNewMenu _menu;
    private PresetControl PresetButtons = new();
    private Dictionary<string, string> _presets = new();

    public VoteCallNewEui()
    {
        _menu = new VoteCallNewMenu();
        _menu.VoteStartButton.OnPressed += VoteStartPressed;
    }

    private void VoteStartPressed(BaseButton.ButtonEventArgs obj)
    {
        var targetListButton =
            PresetButtons.ButtonsList
                .Where(x => x.Value.Pressed)
                .Select(x => x.Key);
        var targetList = string.Join(" ",
            _presets
                .Where(x => targetListButton.Contains(x.Value))
                .Select(x => x.Key));
        SendMessage(new VoteCallNewEuiMsg.DoVote
        {
            TargetPresetList = targetList,
        });
    }

    private void SetPresetsList(List<string> presets)
    {
        PresetButtons.Populate(presets);
        _menu.PresetsContainer.AddChild(PresetButtons);
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not VoteCallNewEuiState s)
        {
            return;
        }

        _presets = s.Presets;
        SetPresetsList(s.Presets.Select(x => x.Value).ToList());
    }

    public override void Opened()
    {
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        _menu.Close();
    }
}
