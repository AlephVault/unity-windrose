using UnityEngine;
using UnityEngine.Events;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Objects
            {
                using AlephVault.Unity.Layout.Utils;

                /// <summary>
                ///   Watchers instantiate their own vision range. Their range can be
                ///     referenced, and event handlers can be tied to it.
                /// </summary>
                [RequireComponent(typeof(MapObject))]
                public class Watcher : MonoBehaviour
                {
                    /// <summary>
                    ///   The value to the <see cref="TriggerVisionRange.visionSize"/> property
                    ///     in their created <see cref="TriggerVisionRange"/>.
                    /// </summary>
                    [SerializeField]
                    private uint visionSize = 0;

                    /// <summary>
                    ///   The value to the <see cref="TriggerVisionRange.visionLength"/> property
                    ///     in their created <see cref="TriggerVisionRange"/>.
                    /// </summary>
                    [SerializeField]
                    private uint visionLength = 0;

                    private TriggerVisionRange relatedVisionRange;

                    /// <summary>
                    ///   Its related <see cref="TriggerVisionRange"/>. The spirit of this property
                    ///     is that it will be the one being retrieved, and events will be tied
                    ///     to them.
                    /// </summary>
                    public TriggerVisionRange RelatedVisionRange { get { return relatedVisionRange; } }

                    /// <summary>
                    ///   This event is triggered when this object is ready (actually: when its related
                    ///     <see cref="TriggerVisionRange"/> is created).
                    /// </summary>
                    public readonly UnityEvent onWatcherReady = new UnityEvent();

                    void Start()
                    {
                        MapObject mapObject = GetComponent<MapObject>();
                        GameObject aNewGameObject = new GameObject("WatcherVisionRange");
                        Behaviours.AddComponent<BoxCollider>(aNewGameObject);
                        relatedVisionRange = Behaviours.AddComponent<TriggerVisionRange>(aNewGameObject, new System.Collections.Generic.Dictionary<string, object>()
                    {
                        { "relatedObject", mapObject },
                        { "visionSize", visionSize },
                        { "visionLength", visionLength }
                    });
                        relatedVisionRange.RefreshDimensions();
                        onWatcherReady.Invoke();
                    }

                    void OnDestroy()
                    {
                        try
                        {
                            if (relatedVisionRange.gameObject != null) Destroy(relatedVisionRange.gameObject);
                        }
                        catch (MissingReferenceException)
                        {
                            // It doesn't matter if this exception is fired when destroying this crap.
                            // This means that somehow the reference failed despite evaluating as not-null.
                            // This means that it does not exist, and was destroyed beforehand.
                        }
                    }
                }
            }
        }
    }
}