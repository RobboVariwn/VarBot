using System;
using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace Varwin.ECS.Systems
{
    public sealed class ZoneControlSystem : IExecuteSystem, ICleanupSystem
    {
        private readonly IGroup<GameEntity> _zoneEntities;
        private readonly IGroup<GameEntity> _allEntites;
        private List<Wrapper> _overlappingObjects;

        public ZoneControlSystem(Contexts contexts)
        {
            _zoneEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Zone, GameMatcher.GameObject,
                GameMatcher.ColliderAware));
            _allEntites = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Wrapper, GameMatcher.Collider));
        }

        public void Execute()
        {
            if (ProjectData.GameMode != GameMode.View && ProjectData.GameMode != GameMode.Preview)
            {
                return;
            }

            bool isAdded = false;
            bool isRemoved = false;

            foreach (var zoneEntity in _zoneEntities.GetEntities())
            {
                _overlappingObjects = new List<Wrapper>();

                Collider collider = zoneEntity.gameObject.Value.GetComponent<Collider>();

                if (collider == null)
                {
                    collider = zoneEntity.gameObject.Value.GetComponentInChildren<Collider>();

                    if (collider == null)
                    {
                        continue;
                    }
                }

                foreach (var otherEntity in _allEntites)
                {
                    if (otherEntity.gameObject.Value != zoneEntity.gameObject.Value)
                    {
                        Collider otherCollider = otherEntity.collider.Value;

                        if (otherCollider == null)
                        {
                            continue;
                        }

                        if (!otherEntity.hasWrapper)
                        {
                            continue;
                        }

                        var zoneTransform = collider.transform;
                        var otherTransform = otherCollider.transform;
                        Vector3 dir;
                        float distance;

                        if (collider.bounds.Intersects(otherCollider.bounds) &&
                            Physics.ComputePenetration(collider, zoneTransform.position, zoneTransform.rotation,
                                otherCollider, otherTransform.position, otherTransform.rotation,
                                out dir, out distance))
                        {
                            _overlappingObjects.Add(otherEntity.wrapper.Value);

                            if (!zoneEntity.zone.WrappersList.Contains(otherEntity.wrapper.Value))
                            {
                                isAdded = true;
                            }
                        }
                        else if (zoneEntity.zone.WrappersList.Contains(otherEntity.wrapper.Value))
                        {
                            isRemoved = true;
                        }
                    }
                }

                zoneEntity.ReplaceZone(_overlappingObjects);

                if (isAdded)
                {
                    try
                    {
                        zoneEntity.colliderAware.Value?.OnObjectEnter(_overlappingObjects.ToArray());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"On object enter has error in object {zoneEntity.gameObject.Value}");
                    }
                }

                if (isRemoved)
                {
                    try
                    {
                        zoneEntity.colliderAware.Value?.OnObjectExit(_overlappingObjects.ToArray());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"On object enter has error in object {zoneEntity.gameObject.Value}");
                    }
                }
            }
        }


        public void Cleanup()
        {
        }
    }
}