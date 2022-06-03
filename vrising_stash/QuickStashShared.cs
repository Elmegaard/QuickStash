using ProjectM;
using ProjectM.CastleBuilding;
using System.Linq;
using UnhollowerRuntimeLib;
using Unity.Collections;
using Unity.Entities;

namespace vrising_stash
{
    public class QuickStashShared
    {
        private static ComponentType[] _containerComponents = null;
        private static ComponentType[] ContainerComponents
        {
            get
            {
                if (_containerComponents == null)
                {
                    _containerComponents = new[] {

                        ComponentType.ReadOnly(Il2CppType.Of<Team>()),
                        ComponentType.ReadOnly(Il2CppType.Of<CastleHeartConnection>()),
                        ComponentType.ReadOnly(Il2CppType.Of<InventoryBuffer>()),
                        ComponentType.ReadOnly(Il2CppType.Of<NameableInteractable>()),
                    };
                }
                return _containerComponents;
            }
        }

        public static NativeArray<Entity> GetStashEntities(EntityManager entityManager)
        {
            var query = entityManager.CreateEntityQuery(ContainerComponents);
            return query.ToEntityArray(Allocator.Temp);
        }

        public static bool IsEntityStash(EntityManager entityManager, Entity entity)
        {
            var archType = UnsafeEntityManagerUtility.GetEntityArchetype(entityManager, entity);
            var componentTypes = archType.GetComponentTypes(Allocator.Temp).ToArray().ToList();

            // Make sure all comoponents in _containerComponents is in the entity
            if (ContainerComponents.Select(x => x.TypeIndex).Except(componentTypes.Select(x => x.TypeIndex)).Any())
            {
                return false;
            }

            return true;
        }
    }
}
