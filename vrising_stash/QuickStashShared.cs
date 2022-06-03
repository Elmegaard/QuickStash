using ProjectM;
using ProjectM.CastleBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerRuntimeLib;
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

                        ComponentType.ReadOnly(Il2CppType.Of<InventoryOwner>()),
                        ComponentType.ReadOnly(Il2CppType.Of<Team>()),
                        ComponentType.ReadOnly(Il2CppType.Of<CastleHeartConnection>()),
                        ComponentType.ReadOnly(Il2CppType.Of<InventoryBuffer>()),
                        ComponentType.ReadOnly(Il2CppType.Of<NameableInteractable>()),
                        ComponentType.ReadOnly(Il2CppType.Of<Immortal>()),
                        ComponentType.ReadOnly(Il2CppType.Of<Immaterial>()),
                        ComponentType.ReadOnly(Il2CppType.Of<Invulnerable>()),
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
    }
}
