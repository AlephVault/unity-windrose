using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                /// <summary>
                ///   A platform notifies the zone events (see <see cref="TriggerZone"/> for more details)
                ///     and calculates the zone bounds based on its underlying map object's dimensions,
                ///     but considering an "inner margin" factor to avoid collisions when objects are
                ///     not there but immediately adjacent in any axis.
                /// </summary>
                [RequireComponent(typeof(MapObject))]
                [RequireComponent(typeof(BoxCollider))]
                public class TriggerPlatform : TriggerZone
                {
                    /// <summary>
                    ///   The inner margin to set. It must be strictly positive to avoid "bleeding"
                    ///     (collisions with adjacent <see cref="TriggerLive"/> objects).
                    /// </summary>
                    [SerializeField]
                    private float innerMarginFactor = 0.25f;

                    /// <summary>
                    ///   The delta X is the object's X position.
                    /// </summary>
                    /// <returns>The delta X</returns>
                    protected override int GetDeltaX()
                    {
                        return (int)mapObject.X;
                    }

                    /// <summary>
                    ///   The delta Y is the object's Y position.
                    /// </summary>
                    /// <returns>The delta Y</returns>
                    protected override int GetDeltaY()
                    {
                        return (int)mapObject.Y;
                    }

                    /// <summary>
                    ///   The related map object is itself.
                    /// </summary>
                    /// <returns></returns>
                    protected override MapObject GetRelatedObject()
                    {
                        return GetComponent<MapObject>();
                    }

                    /// <summary>
                    ///   The related collider is its <see cref="BoxCollider"/>
                    ///     component, required right here.
                    /// </summary>
                    /// <returns></returns>
                    protected override Collider GetCollider()
                    {
                        return GetComponent<BoxCollider>();
                    }

                    /// <summary>
                    ///   Sets up the collider considering not just its dimensions
                    ///     (like <see cref="TriggerLive"/> does) but also the inner
                    ///     margin to avoid bleeding.
                    /// </summary>
                    /// <param name="collider">The collider to set up</param>
                    protected override void SetupCollider(Collider collider)
                    {
                        BoxCollider boxCollider = (BoxCollider)collider;
                        float cellWidth = mapObject.GetCellWidth();
                        float cellHeight = mapObject.GetCellHeight();
                        // collision mask will have certain width and height
                        boxCollider.size = new Vector3(mapObject.Width * cellWidth, mapObject.Height * cellHeight, 1f);
                        // and starting with those dimensions, we compute the offset as >>> and vvv
                        boxCollider.center = new Vector3(boxCollider.size.x / 2, boxCollider.size.y / 2, 0);
                        // adjust to tolerate inner delta and avoid bleeding
                        boxCollider.size = boxCollider.size - 2 * (new Vector3(innerMarginFactor * cellWidth, innerMarginFactor * cellHeight, 0f));
                    }
                }
            }
        }
    }
}