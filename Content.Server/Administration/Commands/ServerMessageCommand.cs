using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Timing;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Host)]
    public sealed class ServerMessageCommand : IConsoleCommand
    {
        public string Command => "servermessage";
        public string Description => "Send an in-game server message.";
        public string Help => $"{Command} <int times> <message> or {Command} <message> to send message.";
        private IGameTiming _timing = default!;
        private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(5);
        private TimeSpan _repentTime = TimeSpan.Zero;
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var chat = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();
            _timing = IoCManager.Resolve<IGameTiming>();

            if (_timing.CurTime < _repentTime)
            {
                shell.WriteLine($"Cooldown. You can write it after {_repentTime.Seconds - _timing.CurTime.Seconds} seconds.");
                return;
            }

            if (args.Length == 0)
            {
                shell.WriteError("Not enough arguments! Need at least 1.");
                return;
            }

            if (args.Length == 1 || !int.TryParse(args[0], out int times))
            {
                chat.DispatchServerMessage(args[0], colorOverride: Color.Cyan);
                shell.WriteLine("Sent!");
                _repentTime = _cooldown + _timing.CurTime;
            }
            else
            {
                var message = string.Join(' ', new ArraySegment<string>(args, 1, args.Length - 1));
                for (int i = 0; i < times; i++)
                {
                    chat.DispatchServerMessage(message, colorOverride: Color.Cyan);
                }
                shell.WriteLine("Sent!");
                _repentTime = _cooldown + _timing.CurTime;
            }
        }
    }
}
