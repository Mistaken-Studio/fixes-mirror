// -----------------------------------------------------------------------
// <copyright file="FixItemsDisappearOnEscapeHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using Mistaken.API.Diagnostics;
using UnityEngine;

namespace Mistaken.Fixes
{
    internal sealed class FixItemsDisappearOnEscapeHandler : Module
    {
        public FixItemsDisappearOnEscapeHandler(IPlugin<IConfig> plugin)
            : base(plugin)
        {
        }

        public override string Name => nameof(FixItemsDisappearOnEscapeHandler);

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            List<ItemType> itemsToDrop = new();

            foreach (var item in ev.Player.Inventory.UserInventory.Items.ToArray())
            {
                if (item.Value.ItemTypeId == ItemType.SCP2176 || item.Value.ItemTypeId == ItemType.SCP018)
                {
                    ev.Player.Inventory.UserInventory.Items.Remove(item.Key);
                    itemsToDrop.Add(item.Value.ItemTypeId);
                }
            }

            MEC.Timing.RunCoroutine(this.DropItems(itemsToDrop, ev.Player));
        }

        private IEnumerator<float> DropItems(List<ItemType> itemsToDrop, Player player)
        {
            yield return MEC.Timing.WaitForSeconds(0.5f);
            var position = player.Position - Vector3.up;

            foreach (var item in itemsToDrop)
            {
                Item.Create(item, player).Spawn(position);
                yield return MEC.Timing.WaitForSeconds(0.2f);
            }
        }
    }
}
