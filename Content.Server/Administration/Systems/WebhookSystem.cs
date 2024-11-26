using System.Text.Json;
using System.Text.Json.Nodes;
using Content.Server.Database;
using Content.Server.Discord;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server.Administration.Systems;

public sealed class WebhookSystem : EntitySystem
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

        var record = await _db.GetPlayerRecordByUserId(args.Session.UserId);
        var firstConnection = record != null &&
                              Math.Abs((record.FirstSeenTime - record.LastSeenTime).TotalMinutes) < 1;

        if (firstConnection)
        {
            CreateMessage(args.Session);
        }
    }

    private async void CreateMessage(ICommonSession session)
    {
        var uid = session.UserId;
        var name = session.Name;

        var payload = new WebhookPayload()
        {
            Username = "KeplyBot",
            Embeds = new List<WebhookEmbed>
            {
                new()
                {
                    Title = "Arrived new player",
                    Color = 13438992, // #CD1010
                    Description = $"Arrived new player",
                    Footer = new WebhookEmbedFooter
                    {
                        Text = $"Name: {name} \n" +
                               $"NUID: {uid}",
                    },
                },
            }
        };

        var state = new WebhookState
        {
            WebhookUrl = _cfg.GetCVar(CCVars.DiscordNewPlayerWebhook),
            Payload = payload,
        };

        CreateWebhookMessage(state, payload);
    }

    private async void CreateWebhookMessage(WebhookState state, WebhookPayload payload)
    {
        try
        {
            _sawmill = Logger.GetSawmill("discord");

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

    public sealed class WebhookState
    {
        public required string WebhookUrl;
        public required WebhookPayload Payload;
        public WebhookIdentifier Identifier;
    }
}
