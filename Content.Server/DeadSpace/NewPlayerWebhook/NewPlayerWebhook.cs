using System.Text.Json;
using Content.Server.Database;
using Content.Server.Discord;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server.DeadSpace.NewPlayerWebhook;

public sealed class NewPlayerWebhook : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly DiscordWebhook _discord = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
    }

    private async void PlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus != SessionStatus.Connected)
        {
            return;
        }

        _sawmill = Logger.GetSawmill("discord");

        var record = await _db.GetPlayerRecordByUserId(args.Session.UserId);
        var firstConnection = record != null &&
                              Math.Abs((record.FirstSeenTime - record.LastSeenTime).TotalMinutes) < 1;

        if (firstConnection && record != null)
        {
            CreateMessage(record);
        }
    }

    private async void CreateMessage(PlayerRecord record)
    {
        var url = _cfg.GetCVar(CCVars.DiscordNewPlayerWebhook);

        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        var hwid = record.HWId != null ? record.HWId.ToString() : "Unknown";

        var fields = new List<WebhookEmbedField>
        {
            new() { Name = "Name", Value = ProfileUrl(record.LastSeenUserName, record.LastSeenUserName), Inline = false },
            new() { Name = "UserId", Value = ProfileUrl(record.UserId.ToString(), record.UserId.ToString()), Inline = false },
            new() { Name = "Address", Value = ProfileUrl(record.LastSeenAddress.ToString(), record.LastSeenAddress.ToString()), Inline = false },
            new() { Name = "HWId", Value = ProfileUrl(hwid, hwid), Inline = false },
        };
        var serverName = _cfg.GetCVar(CVars.GameHostName);

        serverName = serverName[..Math.Min(serverName.Length, 1500)];

        var payload = new WebhookPayload()
        {
            Username = serverName,
            Embeds = new List<WebhookEmbed>
            {
                new()
                {
                    Title = "Arrived new player",
                    Color = 13438992, // #CD1010
                    Description = $"Arrived new player",
                    Fields = fields,
                },
            },
        };

        var state = new WebhookState
        {
            WebhookUrl = url,
            Payload = payload,
        };

        CreateWebhookMessage(state, payload);
    }

    private async void CreateWebhookMessage(WebhookState state, WebhookPayload payload)
    {
        try
        {
            if (await _discord.GetWebhook(state.WebhookUrl) is not { } identifier)
                return;

            state.Identifier = identifier.ToIdentifier();
            _sawmill.Debug(JsonSerializer.Serialize(payload));

            await _discord.CreateMessage(identifier.ToIdentifier(), payload);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Error while sending newPlayer webhook to Discord: {e}");
        }
    }

    private sealed class WebhookState
    {
        public required string WebhookUrl;
        public required WebhookPayload Payload;
        public WebhookIdentifier Identifier;
    }

    private static string ProfileUrl(string? value, string info)
    {
        return $"[{value}](https://admin.deadspace14.net" +
               $"/Connections?showSet=true&search={info}" +
               $"&showAccepted=true&showBanned=true&showWhitelist=true&showFull=true&showPanic=true)";
    }
}
