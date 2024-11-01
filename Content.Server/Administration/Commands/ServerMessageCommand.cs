using Content.Server.Administration;
using Content.Server.Chat;
using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Moderator)]
    public sealed class ServerMessageCommand : IConsoleCommand
    {
        public string Command => "servermessage";
        public string Description => "Send an in-game server message.";
        public string Help => $"{Command} <message> to send message.";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var chat = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();

            if (args.Length == 0)
            {
                shell.WriteError("Not enough arguments! Need at least 1.");
                return;
            }

            chat.DispatchServerMessage(args[0], colorOverride: Color.Cyan);
            shell.WriteLine("Sent!");
        }
    }
}
