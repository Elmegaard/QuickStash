using ProjectM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using Wetstone.API;

namespace vrising_stash
{
    public class QuickStashClient
    {
        private static DateTime _lastInventoryTransfer = DateTime.Now;
        private static List<Entity> _inventoryEntities = new List<Entity>();

        public static void Reset()
        {
            _lastInventoryTransfer = DateTime.Now;
            _inventoryEntities = new List<Entity>();
        }

        public static void HandleInput(GameplayInputSystem __instance)
        {
            //if (!VWorld.IsClient)
            //{
            //    return;
            //}


            if ((Input.GetKeyInt(Plugin.configKeybinding.Primary) || Input.GetKeyInt(Plugin.configKeybinding.Secondary)) && DateTime.Now - _lastInventoryTransfer > TimeSpan.FromSeconds(0.5))
            {
                _lastInventoryTransfer = DateTime.Now;
                UpdateInventoryList();
                TransferItems();
            }
        }

        private static void TransferItems()
        {
            if (_inventoryEntities.Count == 0)
            {
                return;
            }

            var character = EntitiesHelper.GetLocalCharacterEntity(VWorld.Client.EntityManager);
            if (character == Entity.Null)
            {
                return;
            }

            Entity playerInventory;
            InventoryUtilities.TryGetInventoryEntity(VWorld.Client.EntityManager, character, out playerInventory);

            if (playerInventory == Entity.Null)
            {
                return;
            }

            foreach (var invEntity in _inventoryEntities)
            {
                if (invEntity == Entity.Null || playerInventory == Entity.Null)
                {
                    continue;
                }

                EventHelper.TrySmartMergeItems(VWorld.Client.EntityManager, playerInventory, invEntity);
            }
        }

        public static void UpdateInventoryList()
        {
            var character = EntitiesHelper.GetLocalCharacterEntity(VWorld.Client.EntityManager);
            if (character == Entity.Null)
            {
                return;
            }

            _inventoryEntities = new List<Entity>();
            var entities = QuickStashShared.GetStashEntities(VWorld.Client.EntityManager);

            foreach (var entity in entities)
            {
                Entity inventoryEntity;
                InventoryUtilities.TryGetInventoryEntity(VWorld.Client.EntityManager, entity, out inventoryEntity);

                if (inventoryEntity == Entity.Null)
                {
                    continue;
                }

                _inventoryEntities.Add(inventoryEntity);
            }
        }
    }
}
