using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace ScriptableObjects
        {
            namespace Tiles
            {
                using Layout.Utils;

                /// <summary>
                ///   <para>
                ///     This tile is a bundle of strategies that may add functionality to
                ///       your game.
                ///   </para>
                ///     
                ///   <para>
                ///     To work appropriately, another tile asset must be created (a concrete
                ///       tile of another type), and then the instance of this class must
                ///       reference that tile. The behaviour will be entirely delegated to
                ///       that tile.
                ///   </para>
                ///   
                ///   <para>
                ///     On top of that proxying, this asset also adds a bunch of strategies
                ///       that provide data to the maps (actually: their map strategies)
                ///       referencing them.
                ///   </para>
                /// </summary>
                [CreateAssetMenu(fileName = "NewBundledTile", menuName = "Aleph Vault/WindRose/Tiles/Bundled Tile", order = 201)]
                public class BundledTile : TileBase
                {
                    /// <summary>
                    ///   The underlying tile. It may be the tile you want, but it was little
                    ///     to no use if it is a BundledTile as well. Please, choose another
                    ///     tile (concrete) type.
                    /// </summary>
                    [SerializeField]
                    private TileBase sourceTile;

                    [SerializeField]
                    private Strategies.TileStrategy[] strategies;

                    public class TileStrategyDependencyException : Assets.DependencyException
                    {
                        public TileStrategyDependencyException(string message) : base(message) { }
                    }

                    void OnEnable()
                    {
                        try
                        {
                            // Order / Flatten dependencies
                            if (strategies != null) strategies = Assets.FlattenDependencies<Strategies.TileStrategy, RequireTileStrategy, TileStrategyDependencyException>(strategies);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e, this);
                        }
                    }

                    /// <summary>
                    ///   Returns the source tile.
                    /// </summary>
                    public TileBase SourceTile => sourceTile;

                    /// <summary>
                    ///   This call is delegated into the source's <see cref="TileBase.GetTileAnimationData(Vector3Int, ITilemap, ref TileAnimationData)"/>.
                    /// </summary>
                    /// <param name="position"></param>
                    /// <param name="tilemap"></param>
                    /// <param name="tileAnimationData"></param>
                    /// <returns></returns>
                    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
                    {
                        return sourceTile.GetTileAnimationData(position, tilemap, ref tileAnimationData);
                    }

                    /// <summary>
                    ///   This call is delegated into the source's <see cref="TileBase.GetTileData(Vector3Int, ITilemap, ref TileData)"/>
                    /// </summary>
                    /// <param name="position"></param>
                    /// <param name="tilemap"></param>
                    /// <param name="tileData"></param>
                    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
                    {
                        sourceTile.GetTileData(position, tilemap, ref tileData);
                    }

                    /// <summary>
                    ///   This call is delegated into the source's <see cref="TileBase.RefreshTile(Vector3Int, ITilemap)"/>
                    /// </summary>
                    /// <param name="position"></param>
                    /// <param name="tilemap"></param>
                    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
                    {
                        sourceTile.RefreshTile(position, tilemap);
                    }

                    /// <summary>
                    ///   This call is delegated into the source's <see cref="TileBase.StartUp(Vector3Int, ITilemap, GameObject)"/>
                    /// </summary>
                    /// <param name="position"></param>
                    /// <param name="tilemap"></param>
                    /// <param name="go"></param>
                    /// <returns></returns>
                    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
                    {
                        return sourceTile.StartUp(position, tilemap, go);
                    }

                    /// <summary>
                    ///   Gets a tile strategy instance of given type <typeparamref name="T"/>. If many instances of
                    ///     that type are present, the first one is returned.
                    /// </summary>
                    /// <typeparam name="T">The strategy type to choose</typeparam>
                    /// <returns>An attached tile strategy instance for the given type, or null</returns>
                    public T GetStrategy<T>() where T : Strategies.TileStrategy
                    {
                        return (from strategy in strategies where strategy is T select (T)strategy).FirstOrDefault();
                    }

                    /// <summary>
                    ///   Gets all the tile strategy instances of given type <typeparamref name="T"/>.
                    /// </summary>
                    /// <typeparam name="T">The strategy type to choose</typeparam>
                    /// <returns>An array of attached tile strategy instances for the given type</returns>
                    public T[] GetStrategies<T>() where T : Strategies.TileStrategy
                    {
                        return (from strategy in strategies where strategy is T select (T)strategy).ToArray();
                    }


                    /// <summary>
                    ///   <para>
                    ///     This static helper is mainly used in map strategies, on tiles of any types.
                    ///   </para>
                    ///   
                    ///   <para>
                    ///     Invoked on BundledTiles, this method returns whatever the result of
                    ///       <see cref="BundledTile.GetStrategy{T}"/> is. Invoked on other
                    ///       <see cref="TileBase"/> instances, this method returns null.
                    ///   </para>
                    /// </summary>
                    /// <typeparam name="T">The strategy type to choose</typeparam>
                    /// <param name="tile">The tile the strategies must be taken from</param>
                    /// <returns>An attached tile strategy instance -from the tile- for the given type, or null</returns>
                    public static T GetStrategyFrom<T>(TileBase tile) where T : Strategies.TileStrategy
                    {
                        if (tile is BundledTile)
                        {
                            return ((BundledTile)tile).GetStrategy<T>();
                        }
                        else
                        {
                            return null;
                        }
                    }

                    /// <summary>
                    ///   <para>
                    ///     This static helper is mainly used in map strategies, on tiles of any types.
                    ///   </para>
                    ///   
                    ///   <para>
                    ///     Invoked on BundledTiles, this method returns whatever the result of
                    ///       <see cref="BundledTile.GetStrategies{T}"/> is. Invoked on other
                    ///       <see cref="TileBase"/> instances, this method returns an empty array.
                    ///   </para>
                    /// </summary>
                    /// <typeparam name="T">The strategy type to choose</typeparam>
                    /// <param name="tile">The tile the strategies must be taken from</param>
                    /// <returns>An array of attached tile strategy instances for the given type, or an empty array</returns>
                    public static T[] GetStrategiesFrom<T>(TileBase tile) where T : Strategies.TileStrategy
                    {
                        return tile is BundledTile bundledTile ? bundledTile.GetStrategies<T>() : Array.Empty<T>();
                    }
                }
            }
        }
    }
}
