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
        private static DateTime _lastInventoryUpdate = DateTime.Now;
        private static List<Entity> _inventoryEntities = new List<Entity>();
        private static Task _updateListTask = null;
        private static Task _transferTask = null;
        private static readonly object _lock = new();

        public static void HandleInput(GameplayInputSystem __instance)
        {
            if (!VWorld.IsClient)
            {
                return;
            }

            if ((_updateListTask == null || _updateListTask.IsCompleted) && DateTime.Now - _lastInventoryUpdate > TimeSpan.FromSeconds(10))
            {
                var character = EntitiesHelper.GetLocalCharacterEntity(__instance.World.EntityManager);

                if (character != null && character != Entity.Null)
                {
                    UpdateInventoryList(__instance.World.EntityManager, character);
                }
            }

            if ((_transferTask == null || _transferTask.IsCompleted) && (Input.GetKeyInt(Plugin.configKeybinding.Primary) || Input.GetKeyInt(Plugin.configKeybinding.Secondary)) && DateTime.Now - _lastInventoryTransfer > TimeSpan.FromSeconds(2))
            {
                TransferItems(__instance);
            }
        }

        private static void TransferItems(GameplayInputSystem instance)
        {
            var character = EntitiesHelper.GetLocalCharacterEntity(instance.World.EntityManager);
            EntityManager entityManager = instance.EntityManager;

            // Run as task to avoid client stutter
            _transferTask = Task.Run(new Action(() =>
            {
                lock (_lock)
                {
                    _lastInventoryTransfer = DateTime.Now;

                    Entity playerInventory = new Entity();
                    InventoryUtilities.TryGetInventoryEntity(entityManager, character, out playerInventory);

                    if (playerInventory == null || playerInventory == Entity.Null)
                    {
                        return;
                    }

                    foreach (var invEntity in _inventoryEntities)
                    {
                        if (invEntity == Entity.Null || playerInventory == Entity.Null)
                        {
                            continue;
                        }

                        EventHelper.TrySmartMergeItems(entityManager, playerInventory, invEntity);
                    }
                }
            }));
        }


        private static void UpdateInventoryList(EntityManager entityManager, Entity character)
        {
            var entities = entityManager.GetAllEntities(Unity.Collections.Allocator.Persistent);
            if (character == null || character == Entity.Null)
            {
                return;
            }

            // Run as task to avoid client stutter
            _updateListTask = Task.Run(new Action(() =>
            {
                lock (_lock)
                {
                    _inventoryEntities = new List<Entity>();

                    foreach (var entity in entities)
                    {
                        Entity inventoryEntity = new Entity();
                        InventoryUtilities.TryGetInventoryEntity(entityManager, entity, out inventoryEntity);

                        if (inventoryEntity == null || inventoryEntity == Entity.Null)
                        {
                            continue;
                        }

                        if (!QuickStashShared.IsEntityStash(entityManager, inventoryEntity))
                        {
                            continue;
                        }

                        _inventoryEntities.Add(inventoryEntity);
                    }

                    _lastInventoryUpdate = DateTime.Now;
                }
            }));
        }
    }
}
