using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Ceilings
            {
                using World.Layers.Ceiling;
                using AlephVault.Unity.Support.Utils;
                using AlephVault.Unity.Layout.Utils;

                /// <summary>
                ///    Ceilings are mini-tilemaps that will be painted with custom
                ///      tiles and will provide features to hide/show/make-translucent.
                /// </summary>
                [RequireComponent(typeof(Tilemap))]
                [RequireComponent(typeof(TilemapRenderer))]
                public class Ceiling : MonoBehaviour
                {
                    /// <summary>
                    ///   Tells when the parent object is not a <see cref="CeilingLayer"/>.
                    /// </summary>
                    public class ParentMustBeCeilingLayerException : Types.Exception
                    {
                        public ParentMustBeCeilingLayerException() : base() { }
                        public ParentMustBeCeilingLayerException(string message) : base(message) { }
                    }

                    private TilemapRenderer tilemapRenderer;
                    private Grid parentGrid;

                    /// <summary>
                    ///   Display mode. If translucent, the value of <see cref="opacityInTranslucentMode"/>
                    ///     is chosen as the rendering opacity. If hidden it will be 0, and if visible, it
                    ///     will be 1.
                    /// </summary>
                    public enum DisplayMode { HIDDEN, TRANSLUCENT, VISIBLE }

                    /// <summary>
                    ///   The chosen <see cref="DisplayMode"/>.
                    /// </summary>
                    public DisplayMode displayMode;

                    /// <summary>
                    ///   The display mode opacity for the translucent mode.
                    ///   It will be between 0 (invisible) and 1 (visible).
                    ///     Other values will be clamped.
                    /// </summary>
                    [SerializeField]
                    [Range(0, 1)]
                    private float opacityInTranslucentMode;

                    /// <summary>
                    ///   The variable name for the main color variable in the
                    ///     shader material. By default it will be _Color and should
                    ///     not be changed unless the mini-tilemap uses another shader.
                    /// </summary>
                    [SerializeField]
                    private string materialColorVariable = "_Color";

                    /// <summary>
                    ///   See <see cref="opacityInTranslucentMode"/>.
                    /// </summary>
                    public float DisplayModeOpacity
                    {
                        get { return opacityInTranslucentMode; }
                        set { opacityInTranslucentMode = Values.Clamp(0, value, 1); }
                    }

                    private void Awake()
                    {
                        try
                        {
                            CeilingLayer ceilingLayer = Behaviours.RequireComponentInParent<CeilingLayer>(this);
                            parentGrid = ceilingLayer.GetComponent<Grid>();
                            Tilemap tilemap = GetComponent<Tilemap>();
                            tilemap.orientation = Tilemap.Orientation.XY;
                            tilemapRenderer = GetComponent<TilemapRenderer>();
                            tilemapRenderer.sortOrder = TilemapRenderer.SortOrder.BottomLeft;
                        }
                        catch (Exception)
                        {
                            Destroy(gameObject);
                            throw new ParentMustBeCeilingLayerException();
                        }
                    }

                    private void Start()
                    {
                        // Rounding position and setting relative z to 0.
                        transform.localPosition = parentGrid.CellToLocal(parentGrid.LocalToCell(new Vector3(transform.localPosition.x, transform.localPosition.y, 0)));
                    }

                    private void Update()
                    {
                        try
                        {
                            Color color = tilemapRenderer.material.GetColor(materialColorVariable);
                            switch (displayMode)
                            {
                                case DisplayMode.HIDDEN:
                                    color.a = 0;
                                    break;
                                case DisplayMode.VISIBLE:
                                    color.a = 1;
                                    break;
                                case DisplayMode.TRANSLUCENT:
                                    color.a = opacityInTranslucentMode;
                                    break;
                            }
                            tilemapRenderer.material.SetColor(materialColorVariable, color);
                        }
                        catch (Exception)
                        {
                            // Diaper - nothing will be done here.
                        }
                    }
                }
            }
        }
    }
}