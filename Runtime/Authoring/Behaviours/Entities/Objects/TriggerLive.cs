using System;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                /// <summary>
                ///   <para>
                ///     This components sets its underlying collider (which will be a
                ///       <see cref="BoxCollider"/>) to the dimensions of the object
                ///       (considering underlying cell's width/height).
                ///   </para>
                ///   <para>
                ///     Live triggers are intended to be installed into moving objects,
                ///       and not in objects that intend to be like platforms. They 
                ///       ensure the collider is only active while the object is added
                ///       to a map.
                ///   </para>
                /// </summary>
                [RequireComponent(typeof(MapObject))]
                [RequireComponent(typeof(BoxCollider))]
                public class TriggerLive : TriggerHolder
                {
                    private Rigidbody rigidbody;
                    protected override void Awake()
                    {
                        base.Awake();
                        collider.enabled = false;
                        MapObject mapObject = GetComponent<MapObject>();
                        mapObject.onAttached.AddListener(delegate (World.Map map)
                        {
                            collider.enabled = true;
                            RefreshDimensions();
                        });
                        mapObject.onDetached.AddListener(delegate ()
                        {
                            collider.enabled = false;
                        });
                        rigidbody = GetComponent<Rigidbody>();
                    }

                    /// <summary>
                    ///   Gets its underlying <see cref="BoxCollider"/> as the involved collider.
                    /// </summary>
                    /// <returns>The collider</returns>
                    protected override Collider GetCollider()
                    {
                        return GetComponent<BoxCollider>();
                    }

                    /// <summary>
                    ///   Sets up the collider to the dimensions of this object, with respect
                    ///     to the dimensions and cell width/height in the objects layer.
                    /// </summary>
                    /// <param name="collider"></param>
                    protected override void SetupCollider(Collider collider)
                    {
                        BoxCollider boxCollider = (BoxCollider)collider;
                        MapObject mapObject = GetComponent<MapObject>();
                        if (mapObject.ParentMap == null) return;
                        // collision mask will have certain width and height
                        boxCollider.size = new Vector3(mapObject.Width * mapObject.GetCellWidth(), mapObject.Height * mapObject.GetCellHeight(), 1f);
                        // and starting with those dimensions, we compute the offset as >>> and vvv
                        boxCollider.center = new Vector3(boxCollider.size.x / 2, boxCollider.size.y / 2, 0);
                    }

                    protected override void Start()
                    {
                        base.Start();
                        if (rigidbody) rigidbody.isKinematic = true;
                    }
                }
            }
        }
    }
}