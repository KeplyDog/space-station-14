using Content.Server.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Rotatable;
using Content.Shared.Verbs;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Utility;
using Content.Shared.Hands.Components;
using System.Numerics;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Server.TransferItem
{
    public sealed class TransferItemSystem : EntitySystem
    {


        public override void Initialize()
        {
            SubscribeLocalEvent<TransferItemComponent, GetVerbsEvent<Verb>>(AddTransferItemVerb);

            CommandBinds.Builder
                .Bind(ContentKeyFunctions.TransferItem, new PointerInputCmdHandler(HandleTransferItem))
                .Register<TransferItemSystem>();
        }

        private void AddTransferItemVerb(EntityUid uid, TransferItemComponent component, GetVerbsEvent<Verb> args)
        {
            if (!args.CanAccess || !args.CanInteract)
                return;

            Verb verb = new()
            {
                Act = () => TransferItemVerb(uid, component),
                Text = Loc.GetString("flippable-verb-get-data-text"),
                Category = VerbCategory.Rotate,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/flip.svg.192dpi.png")),
                Priority = -3,
                DoContactInteraction = true
            };
            args.Verbs.Add(verb);
        }

        public void TransferItemVerb(EntityUid uid, TransferItemComponent component)
        {
        }

        public bool HandleTransferItem(ICommonSession? playerSession, EntityCoordinates coordinates, EntityUid entity)
        {
            if (playerSession?.AttachedEntity is not { Valid: true } player || !Exists(player))
                return false;

            if (TryComp(playerSession?.AttachedEntity, out HandsComponent? hands) && hands.ActiveHand != null)
                TryTransferItem(playerSession.AttachedEntity.Value, hands.ActiveHand, coordinates, hands);

            return false;
        }

        public bool TryTransferItem(EntityUid uid, Hand hand, EntityCoordinates coordinates, HandsComponent? handsComp = null)
        {
            if (!Resolve(uid, ref handsComp))
                return false;

            if (!CanDropHeld(uid, hand, checkActionBlocker))
                return false;

            var entity = hand.HeldEntity!.Value;
        }

        public bool CanTransferHeld(EntityUid uid, Hand hand, bool checkActionBlocker = true)
        {
            if (hand.Container?.ContainedEntity is not {} held)
                return false;

            if (!ContainerSystem.CanRemove(held, hand.Container))
                return false;

            if (checkActionBlocker && !_actionBlocker.CanDrop(uid))
                return false;

            return true;
        }
    }
}
