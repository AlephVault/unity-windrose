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
                ///     This behaviour configures a collision mask (giving it appropriate
                ///       dimensions and and making it a trigger).
                ///   </para>
                ///   <para>
                ///     Although the size and pivot are determined once, and this behaviour
                ///       does not touch the collider or handles the collisions, it will
                ///       provide a method named <see cref="RefreshDimensions"/> so other
                ///       users can make use of it when needed.
                ///   </para>
                /// </summary>
                [RequireComponent(typeof(Collider))]
                public abstract class TriggerHolder : MonoBehaviour
                {
                    /// <summary>
                    ///   The retrieved collider.
                    /// </summary>
                    protected Collider collider;

                    /// <summary>
                    ///   This method must be implemented to retrieve the underlying component.
                    ///   It will actually retrieve a collider component from this component,
                    ///     but will differ on the type.
                    /// </summary>
                    /// <returns>The retrieved component</returns>
                    protected abstract Collider GetCollider();

                    /// <summary>
                    ///   This method must be implemented to setup the (retrieved) component.
                    /// </summary>
                    /// <param name="collider">The component to setup</param>
                    protected abstract void SetupCollider(Collider collider);

                    /// <summary>
                    ///   Refreshes the dimensions (essentially, invokes <see cref="SetupCollider(Collider)"/>
                    ///     again).
                    /// </summary>
                    public void RefreshDimensions()
                    {
                        SetupCollider(collider);
                    }

                    protected virtual void Awake()
                    {
                        collider = GetCollider();
                    }

                    protected virtual void Start()
                    {
                        collider.isTrigger = true;
                    }
                }
            }
        }
    }
}