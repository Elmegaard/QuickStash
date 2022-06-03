using HarmonyLib;
using ProjectM;
using Unity.Entities;

namespace vrising_stash
{

    [HarmonyPatch]
    public class ClientEventValidation_Patch
    {

        [HarmonyPatch(typeof(ClientEventValidation), nameof(ClientEventValidation.IsInteractingWithInventory))]
        [HarmonyPostfix]
        static void IsInteractingWithInventory(ref bool __result, Entity interactor, Entity inventory, EntityManager entityManager)
        {
            QuickStashServer.IsInteractingWithInventory(ref __result, interactor, inventory, entityManager);
        }
    }
}
