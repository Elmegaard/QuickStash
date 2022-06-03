using ProjectM.Scripting;
using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace vrising_stash
{
    public class QuickStashServer
    {
        public static void IsInteractingWithInventory(ref bool __result, Entity interactor, Entity inventory, EntityManager entityManager)
        {
            //if (!VWorld.IsServer)
            //{
            //    return;
            //}

            if (!QuickStashShared.IsEntityStash(entityManager, inventory))
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
