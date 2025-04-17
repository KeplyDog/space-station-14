using Robast.Shared.Input.Binding;

namespace Content.Server.TransferItem
{
    public sealed class TransferItemSystem : EntitySystem
    {


        public override void Initialize()
        {


            CommandBinds.Builder
                .Bind(ContentKeyFunction.TransferItem, new PointerInputCmdHandler(HandleTransferItem))
                .Register<TransferItemSystem>();
        }



        public bool HandleTransferItem(ICommonSession? playerSession, EntityCoordinates coordinates, EntityUid entity)
        {
            if (playerSession?.AttachedEntity is not { Valid: true } player || !Exists(player))
                return false;

            if (!TryComp<>)

            return true;
        }
    }
}
