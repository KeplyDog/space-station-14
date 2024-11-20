using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared.VotingNew;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.VotingNew.UI;

public sealed class VoteCallNewEui : BaseEui
{
    private readonly VoteCallNewMenu _menu;
    private readonly PresetControl _presetButtons = new();
    private Dictionary<string, string> _presets = new();
    private Dictionary<Button, List<string>> _gameRulesPresets = new();

    private Dictionary<int, CreateGameRulesPreset> AvailableGameRulesPresets = new()
    {
    };

    public VoteCallNewEui()
    {
        _menu = new VoteCallNewMenu();
        _menu.VoteStartButton.OnPressed += VoteStartPressed;
        SetGameRulesPresets();
        _menu.PresetsButton.OnItemSelected += PickGameRulesPreset;
    }

    private void VoteStartPressed(BaseButton.ButtonEventArgs obj)
    {
        var targetListButton =
            _presetButtons.ButtonsList
                .Where(x => x.Value.Pressed)
                .Select(x => x.Key);

        var targetList =
            _presets
                .Where(x => targetListButton.Contains(x.Value))
                .Select(x => x.Key)
                .ToList();

        SendMessage(new VoteCallNewEuiMsg.DoVote
        {
            TargetPresetList = targetList,
        });
    }

    private void SetPresetsList(List<string> presets)
    {
        _presetButtons.Populate(presets);
        _menu.PresetsContainer.AddChild(_presetButtons);
    }

    private void SetGameRulesPresets()
    {
        List<string> standardPresets = new List<string>
        {
            "Zombie", "Revolutionary", "Nukeops", "Traitor", "Secret", "Extended", "AllAtOnce", "Survival",
        };

        AvailableGameRulesPresets.Add(0, new CreateGameRulesPreset("По умолчанию", standardPresets));
        _menu.PresetsButton.AddItem(AvailableGameRulesPresets[0].Name, 0);

        List<string> rdmPresets = new List<string>
        {
            "Zombie", "Revolutionary", "Nukeops", "AllAtOnce", "Survival",
        };

        AvailableGameRulesPresets.Add(1, new CreateGameRulesPreset("РДМ", rdmPresets));
        _menu.PresetsButton.AddItem(AvailableGameRulesPresets[1].Name, 1);

        List<string> calmPresets = new List<string>
        {
            "Extended", "Greenshift", "Traitor",
        };

        AvailableGameRulesPresets.Add(2, new CreateGameRulesPreset("Спокойный", calmPresets));
        _menu.PresetsButton.AddItem(AvailableGameRulesPresets[2].Name, 2);
    }

    private void PickGameRulesPreset(OptionButton.ItemSelectedEventArgs obj)
    {
        var presets = AvailableGameRulesPresets[obj.Id].GameRulesPresets;
        foreach (var buttonPreset in _presetButtons.ButtonsList.Values)
        {
            buttonPreset.Pressed = presets.Any(x => _presets[x] == buttonPreset.Text);
        }
    }


    public override void HandleState(EuiStateBase state)
    {
        if (state is not VoteCallNewEuiState s)
        {
            return;
        }

        _presets = s.Presets;
        SetPresetsList(s.Presets.Select(x => x.Value).ToList());
        SetGameRulesPresets();
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

public record struct CreateGameRulesPreset
{
    public string Name;
    public List<string> GameRulesPresets;

    public CreateGameRulesPreset(string name, List<string> gameRulesPresets)
    {
        Name = name;
        GameRulesPresets = gameRulesPresets;
    }
}
