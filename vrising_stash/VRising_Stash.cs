using HarmonyLib;
using System;
using System.Collections.Generic;
using ProjectM;
using UnityEngine;
using ProjectM.Scripting;
using Unity.Entities;
using System.Threading;
using Il2CppSystem.Threading.Tasks;
using System.Linq;

namespace vrising_stash
{
    [HarmonyPatch]
    public class GameplayInputSystem_Patch
    {
        private static DateTime _lastInventoryTransfer = DateTime.Now;
        private static DateTime _lastInventoryUpdate = DateTime.Now;
        private static List<Entity> _inventoryEntities = new List<Entity>();
        private static bool _isTransfering = false;
        private static bool _isUpdatingInventory = false;
        private static Task _updateListTask = null;
        private static Task _transferTask = null;

        [HarmonyPatch(typeof(GameplayInputSystem), nameof(GameplayInputSystem.HandleInput))]
        [HarmonyPostfix]
        static void HandleInput(GameplayInputSystem __instance, InputState inputState)
        {
            if (!__instance.World.IsClientWorld())
            {
                return;
            }

            if ((_updateListTask == null || _updateListTask.IsCompleted) && DateTime.Now - _lastInventoryUpdate > TimeSpan.FromSeconds(10))
            {
                var gameManager = __instance.World.GetExistingSystem<ClientScriptMapper>()?._ClientGameManager;
                var character = EntitiesHelper.GetLocalCharacterEntity(__instance.World.EntityManager);

                if (gameManager != null && character != null && character.Index > -1)
                {
                    UpdateInventoryList(__instance.World.EntityManager, gameManager, character);
                }
            }

            if ((_transferTask == null || _transferTask.IsCompleted) && Input.GetKeyInt(KeyCode.G) && DateTime.Now - _lastInventoryTransfer > TimeSpan.FromSeconds(2))
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
                while (_isUpdatingInventory)
                {
                    Thread.Sleep(50);
                }
                _isTransfering = true;

                Entity playerInventory = new Entity();
                InventoryUtilities.TryGetInventoryEntity(entityManager, character, out playerInventory);

                if (playerInventory != null)
                {
                    foreach (var invEntity in _inventoryEntities)
                    {
                        EventHelper.TrySmartMergeItems(entityManager, playerInventory, invEntity);
                    }
                }

                _lastInventoryTransfer = DateTime.Now;
                _isTransfering = false;
            }));
        }

        public static List<int> _containerComponents = new List<int>() {
            941, // InventoryOwner
            1041, // Team
            1405, // CastleHeartConnection
            67109800, // InventoryBuffer
            16779905, // NameableInteractable
            16778151, // Immortal
            16778341, // Immaterial
            16778344, // Invulnerable
        };

        public static bool IsEntityStash(EntityManager entityManager, Entity entity)
        {
            var archType = UnsafeEntityManagerUtility.GetEntityArchetype(entityManager, entity);
            var componentTypes = archType.GetComponentTypes(Unity.Collections.Allocator.Persistent);

            // Make sure all comoponents in _containerComponents is in the entity
            if (_containerComponents.Except(componentTypes.ToArray().ToList().Select(x => x.TypeIndex)).Any())
            {
                return false;
            }

            return true;
        }

        private static void UpdateInventoryList(EntityManager entityManager, ClientGameManager gameManager, Entity character)
        {
            var entities = entityManager.GetAllEntities(Unity.Collections.Allocator.Persistent);
            if (entities == null || gameManager == null || character == null || character == Entity.Null || gameManager._TeamChecker == null)
            {
                return;
            }

            // Run as task to avoid client stutter
            _updateListTask = Task.Run(new Action(() =>
            {
                while (_isTransfering)
                {
                    Thread.Sleep(50);
                }

                _isUpdatingInventory = true;
                _inventoryEntities = new List<Entity>();

                foreach (var entity in entities)
                {
                    Entity inventoryEntity = new Entity();
                    InventoryUtilities.TryGetInventoryEntity(entityManager, entity, out inventoryEntity);

                    if (inventoryEntity == null || inventoryEntity == Entity.Null)
                    {
                        continue;
                    }

                    // Make sure all comoponents in _containerComponents is in the entity
                    if (!IsEntityStash(entityManager, entity))
                    {
                        continue;
                    }

                    if (!gameManager._TeamChecker.IsAllies(character, inventoryEntity))
                    {
                        continue;
                    }


                    _inventoryEntities.Add(inventoryEntity);
                }

                _lastInventoryUpdate = DateTime.Now;
                _isUpdatingInventory = false;
            }));
        }
    }

    [HarmonyPatch]
    public class ClientEventValidation_Patch
    {
        [HarmonyPatch(typeof(ClientEventValidation), nameof(ClientEventValidation.IsInteractingWithInventory))]
        [HarmonyPostfix]
        static void IsInteractingWithInventory(ref bool __result, Entity interactor, Entity inventory, EntityManager entityManager)
        {
            if (!GameplayInputSystem_Patch.IsEntityStash(entityManager, inventory))
            {
                return;
            }

            var gameManager = entityManager.World.GetExistingSystem<ServerScriptMapper>()?._ServerGameManager;
            if (!gameManager._TeamChecker.IsAllies(interactor, inventory))
            {
                return;
            }

            __result = true;
        }
    }
}
