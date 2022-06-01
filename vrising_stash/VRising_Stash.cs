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
using ProjectM.CastleBuilding;
using ProjectM.Tiles;
using Unity.Transforms;

namespace vrising_stash
{
    [HarmonyPatch]
    public class GameplayInputSystem_Patch
    {
        private static DateTime _lastInventoryTransfer = DateTime.Now;
        private static DateTime _lastInventoryUpdate = DateTime.Now.AddSeconds(-8);
        private static List<Entity> _inventoryEntities = new List<Entity>();
        private static Task _updateListTask = null;
        private static Task _transferTask = null;
        private static object _lock = new object();

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

            if ((_transferTask == null || _transferTask.IsCompleted) && Input.GetKeyInt(Plugin.configKeybinding.Value) && DateTime.Now - _lastInventoryTransfer > TimeSpan.FromSeconds(2))
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
                        if (invEntity == null || invEntity == Entity.Null)
                        {
                            continue;
                        }
                        EventHelper.TrySmartMergeItems(entityManager, playerInventory, invEntity);
                    }
                }
            }));
        }

        private static ComponentType[] _containerComponents = null;
        private static ComponentType[] ContainerComponents
        {
            get
            {
                if (_containerComponents == null)
                {
                    _containerComponents = new[] {
                        ComponentType.ReadOnly(InventoryOwner.Il2CppType),
                        ComponentType.ReadOnly(Team.Il2CppType),
                        ComponentType.ReadOnly(CastleHeartConnection.Il2CppType),
                        ComponentType.ReadOnly(InventoryBuffer.Il2CppType),
                        ComponentType.ReadOnly(NameableInteractable.Il2CppType),
                        ComponentType.ReadOnly(Immortal.Il2CppType),
                        ComponentType.ReadOnly(Immaterial.Il2CppType),
                        ComponentType.ReadOnly(Invulnerable.Il2CppType),
                    };
                }
                return _containerComponents;
            }
        }

        public static bool IsEntityStash(EntityManager entityManager, Entity entity)
        {
            var archType = UnsafeEntityManagerUtility.GetEntityArchetype(entityManager, entity);
            var componentTypes = archType.GetComponentTypes(Unity.Collections.Allocator.Persistent).ToArray().ToList();

            // Make sure all comoponents in _containerComponents is in the entity
            if (ContainerComponents.Select(x => x.TypeIndex).Except(componentTypes.Select(x => x.TypeIndex)).Any())
            {
                return false;
            }

            return true;
        }

        private static void UpdateInventoryList(EntityManager entityManager, ClientGameManager gameManager, Entity character)
        {
            var entities = entityManager.GetAllEntities(Unity.Collections.Allocator.Persistent);
            if (gameManager == null || character == null || character == Entity.Null || gameManager._TeamChecker == null)
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

                        if (!IsEntityStash(entityManager, inventoryEntity))
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
                }
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

            if (!IsWithinDistance(interactor, inventory, entityManager))
            {
                return;
            }

            __result = true;
        }

        private static bool IsWithinDistance(Entity interactor, Entity inventory, EntityManager entityManager)
        {
            var interactorLocation = entityManager.GetComponentData<LocalToWorld>(interactor);
            var inventoryLocation = entityManager.GetComponentData<LocalToWorld>(inventory);

            Vector3 difference = new Vector3(
                interactorLocation.Position.x - inventoryLocation.Position.x,
                interactorLocation.Position.y - inventoryLocation.Position.y,
                interactorLocation.Position.z - inventoryLocation.Position.z);

            double distance = Math.Sqrt(
                  Math.Pow(difference.x, 2f) +
                  Math.Pow(difference.y, 2f) +
                  Math.Pow(difference.z, 2f));

            if (distance > Plugin.configMaxDistance.Value)
            {
                return false;
            }

            return true;
        }
    }
}
