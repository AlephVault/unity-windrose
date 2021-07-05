using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Floors
            {
                using AlephVault.Unity.Layout.Utils;

                /// <summary>
                ///   A floor is a behaviour that normalizes the position of a tilemap inside a map.
                ///   Floors will be identified from object management strategies (and strategy holders)
                ///     to get data from their tiles.
                ///   Many floors may exist (and, often, will) in a single map. They will be stacked
                ///     appropriately and, since they have a <see cref="TilemapRenderer"/>, they may be
                ///     given any sort order of choice.
                /// </summary>
                [RequireComponent(typeof(Tilemap))]
                [RequireComponent(typeof(TilemapRenderer))]
                [RequireComponent(typeof(AlephVault.Unity.Support.Authoring.Behaviours.Normalized))]
                public class Floor : MonoBehaviour
                {
                    /// <summary>
                    ///   Tells when the parent is not a <see cref="World.Layers.Floor.FloorLayer"/>.
                    /// </summary>
                    public class ParentMustBeFloorLayerException : Types.Exception
                    {
                        public ParentMustBeFloorLayerException() : base() { }
                        public ParentMustBeFloorLayerException(string message) : base(message) { }
                    }

                    private void Awake()
                    {
                        try
                        {
                            Behaviours.RequireComponentInParent<World.Layers.Floor.FloorLayer>(this);
                            Tilemap tilemap = GetComponent<Tilemap>();
                            tilemap.orientation = Tilemap.Orientation.XY;
                            TilemapRenderer tilemapRenderer = GetComponent<TilemapRenderer>();
                            tilemapRenderer.sortOrder = TilemapRenderer.SortOrder.BottomLeft;
                        }
                        catch (Types.Exception)
                        {
                            Destroy(gameObject);
                            throw new ParentMustBeFloorLayerException();
                        }
                    }
                }
            }
        }
    }
}
