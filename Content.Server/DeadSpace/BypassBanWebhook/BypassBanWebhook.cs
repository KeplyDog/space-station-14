using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using Content.Server.Discord;
using Robust.Shared;
using Robust.Shared.Network;

namespace Content.Server.DeadSpace.BypassBanWebhook;

public sealed class BypassBanWebhook : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly DiscordWebhook _discord = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IServerNetManager _netMgr = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        // _playerManager.PlayerStatusChanged += PlayerStatusChanged;
        _netMgr.Connecting += NetMgrOnConnecting;
    }

    private async Task NetMgrOnConnecting(NetConnectingArgs e)
    {
        _sawmill = Logger.GetSawmill("discord");

        var banList = await _db.GetServerBansAsync(null, e.UserId, null, null);

        if (banList.Count > 0)
        {
            return;
        }

        banList = await _db.GetServerBansAsync(e.IP.Address, null, null, null);
        var infoIp = banList.Count > 0 ? e.IP.Address.ToString() : null;

        banList = await _db.GetServerBansAsync(null, null, e.UserData.HWId, e.UserData.ModernHWIds);

        var infoHwid = banList.Count > 0
            ? e.UserData.HWId == null
                ? $"V2-{Convert.ToBase64String(e.UserData.ModernHWIds.First().AsSpan())}"
                : Convert.ToBase64String(e.UserData.HWId.AsSpan())
            : null;

        CreateMessage(infoIp, infoHwid);
    }

    private async void CreateMessage(string? infoIp, string? infoHwid)
    {
        var url = _cfg.GetCVar(CCVars.DiscordNewPlayerWebhook);

        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        var fields = new List<WebhookEmbedField>();
        var infoType = "";

        if (infoIp != null)
        {
            fields.Add(new() { Name = "Попытка обхода бана!", Value = ProfileUrl("Ip", infoIp)});
            infoType = "Ip";
        }

        if (infoHwid != null)
        {
            fields.Add(new() { Name = "Попытка обхода бана!", Value = ProfileUrl("Hwid", infoHwid)});
            infoType = string.IsNullOrEmpty(infoType) ? "HWid" : $"{infoType} и HWid";
        }

        var serverName = _cfg.GetCVar(CVars.GameHostName);

        serverName = serverName[..Math.Min(serverName.Length, 1500)];

        var payload = new WebhookPayload()
        {
            Username = serverName,
            Embeds = new List<WebhookEmbed>
            {
                new()
                {
                    Title = "Попытка обхода бана",
                    Color = 9442302, // #9013FE
                    Description = $"Совпадение по {infoType}",
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
            _sawmill.Error($"Error while sending bypassBan webhook to Discord: {e}");
        }
    }

    private sealed class WebhookState
    {
        public required string WebhookUrl;
        public required WebhookPayload Payload;
        public WebhookIdentifier Identifier;
    }

    private string ProfileUrl(string? value, string? info)
    {
        return $"[{value}]({_cfg.GetCVar(CCVars.AdminWebSite)}" +
               $"/Connections?showSet=true&search={info}" +
               $"&showAccepted=true&showBanned=true&showWhitelist=true&showFull=true&showPanic=true)";
    }
}
