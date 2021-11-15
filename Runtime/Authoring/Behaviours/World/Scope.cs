using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                /// <summary>
                ///   <para>
                ///     A scope is useful in a "static" context. It assumes it will
                ///     have children <see cref="Map"/> objects, and will enumerate
                ///     them and be able to access them by index.
                ///   </para>
                ///   <para>
                ///     The initial list of maps will be built on Awake. If this
                ///     game is not static, users can invoke <see cref="RefreshMapArray"/>
                ///     to update the list of maps when a map is added or destroyed.
                ///   </para>
                /// </summary>
                public class Scope : MonoBehaviour, IEnumerable<Map>
                {
                    /// <summary>
                    ///   Tells whether this scope is static. This directly involves
                    ///   that <see cref="RefreshMapArray"/> is not available when
                    ///   the value for this property is true.
                    /// </summary>
                    [SerializeField]
                    private bool isStatic = false;

                    /// <summary>
                    ///   Returns the vlaue of <see cref="isStatic"/>.
                    /// </summary>
                    public bool IsStatic { get { return isStatic; } }

                    /// <summary>
                    ///   Tells whether a first map enumeration was done.
                    /// </summary>
                    public bool Ready { get; private set; }

                    // The internal list of maps.
                    private Map[] maps = null;

                    // A read-only wrapper over that mapping.
                    private IReadOnlyDictionary<Map, int> mapsToIDs = null;

                    void Awake()
                    {
                        DoRefreshMapArray();
                        Ready = true;
                    }

                    // Recalculates the list of identified children maps.
                    // This is done NON-RECURSIVELY.
                    private void DoRefreshMapArray()
                    {
                        List<Map> mapList = new List<Map>();
                        Dictionary<Map, int> mapDict = new Dictionary<Map, int>();
                        int count = transform.childCount;
                        for(int i = 0; i < count; i++)
                        {
                            Map map = transform.GetChild(i).GetComponent<Map>();
                            if (map != null)
                            {
                                mapList.Add(map);
                                mapDict.Add(map, mapDict.Count);
                            }
                        }
                        maps = mapList.ToArray();
                        mapsToIDs = new ReadOnlyDictionary<Map, int>(mapDict);
                    }

                    /// <summary>
                    ///   <para>
                    ///     Initializes every map (actually: their object layers) within.
                    ///     Typically, maps know how to initialize themselves on Start(),
                    ///     but this method is a convenience if the users need them already
                    ///     initialized (before Start(), but after Awake()).
                    ///   </para>
                    /// </summary>
                    public void Initialize()
                    {
                        if (!Ready) throw new InvalidOperationException("The collection of maps is not yet ready");
                        foreach(Map map in maps)
                        {
                            map.ObjectsLayer.Initialize();
                        }
                    }

                    /// <summary>
                    ///   On non-static scopes, forces a refresh of the list of identified
                    ///   children maps.
                    /// </summary>
                    public void RefreshMapArray()
                    {
                        if (isStatic)
                        {
                            throw new InvalidOperationException("This scope is static - map list cannot be refreshed");
                        }
                        DoRefreshMapArray();
                        Ready = true;
                    }

                    /// <summary>
                    ///   Allows enumeration of maps.
                    /// </summary>
                    /// <returns>The enumerator for maps</returns>
                    public IEnumerator<Map> GetEnumerator()
                    {
                        if (!Ready) throw new InvalidOperationException("The collection of maps is not yet ready");
                        return ((IEnumerable<Map>)maps).GetEnumerator();
                    }

                    /// <summary>
                    ///   Allows enumeration of maps.
                    /// </summary>
                    /// <returns>An untyped enumerator for maps</returns>
                    IEnumerator IEnumerable.GetEnumerator()
                    {
                        if (!Ready) throw new InvalidOperationException("The collection of maps is not yet ready");
                        return maps.GetEnumerator();
                    }

                    /// <summary>
                    ///   Allows access to the map => id mapping.
                    /// </summary>
                    public IReadOnlyDictionary<Map, int> MapsToIDs 
                    {
                        get
                        {
                            if (!Ready) throw new InvalidOperationException("The collection of maps is not yet ready");
                            return mapsToIDs;
                        }
                    }

                    /// <summary>
                    ///   Returns the count of identified <see cref="Map"/> elements.
                    /// </summary>
                    public int Count => MapsToIDs.Count;

                    /// <summary>
                    ///   Gets a map by its id (which is essentially an index).
                    /// </summary>
                    /// <param name="id">The id of the map to query</param>
                    /// <returns>The map with that id</returns>
                    public Map this[int id]
                    {
                        get
                        {
                            if (!Ready) throw new InvalidOperationException("The collection of maps is not yet ready");
                            return maps[id];
                        }
                    }
                }
            }
        }
    }
}
