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
        public string Command => "serverannouncement";
        public string Description => "Send an in-game message announcement as server.";
        public string Help => $"{Command} <sender> <playSound> <message> or {Command} <sender> <message> or {Command} <message> to send announcement as server.";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var chat = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();

            if (args.Length == 0)
            {
                shell.WriteError("Not enough arguments! Need at least 1.");
                return;
            }

            if (args.Length == 1)
            {
                chat.DispatchServerAnnouncement(args[0], colorOverride: Color.Cyan);
            }
            else
            if (args.Length == 2)
            {
                var message = string.Join(' ', new ArraySegment<string>(args, 1, args.Length - 1));
                chat.DispatchServerAnnouncement(message: message, sender: args[0], colorOverride: Color.Cyan);
            }
            else
            {
                var message = string.Join(' ', new ArraySegment<string>(args, 2, args.Length - 2));
                bool isPlaySound = args[1] == "true";
                chat.DispatchServerAnnouncement(message: message, sender: args[0], playSound: isPlaySound, colorOverride: Color.Cyan);
            }
            shell.WriteLine("Sent!");
        }
    }
}
