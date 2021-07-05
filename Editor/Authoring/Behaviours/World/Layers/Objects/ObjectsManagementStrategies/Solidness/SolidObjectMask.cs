using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using AlephVault.Unity.Support.Utils;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                namespace Layers
                {
                    namespace Objects
                    {
                        namespace ObjectsManagementStrategies
                        {
                            namespace Solidness
                            {
                                /// <summary>
                                ///   The drawer for the mask only involves a button invoking a window for mask edition.
                                /// </summary>
                                [CustomPropertyDrawer(typeof(SolidObjectMask))]
                                public class SolidObjectMaskDrawer : PropertyDrawer
                                {
                                    private SolidnessStatus fillWith = SolidnessStatus.Ghost;
                                    private SerializedProperty widthProperty;
                                    private SerializedProperty heightProperty;
                                    private SerializedProperty cellsProperty;
                                    private Texture2D invalidSquare;
                                    private Texture2D ghostSquare;
                                    private Texture2D holeSquare;
                                    private Texture2D solidSquare;
                                    private uint scrollX = 0;
                                    private uint scrollY = 0;
                                    private Entities.Objects.MapObject clampAgainst = null;
                                    private bool initialized = false;

                                    private Texture2D MakeSolidIcon(Color color, int height = 0)
                                    {
                                        if (height <= 0) height = (int)EditorGUIUtility.singleLineHeight - 2;
                                        int size = height * height;
                                        Color[] content = new Color[size];
                                        for (int index = 0; index < size; index++) content[index] = color;
                                        Texture2D texture = new Texture2D(height, height, TextureFormat.ARGB32, false);
                                        texture.SetPixels(content);
                                        texture.Apply();
                                        return texture;
                                    }

                                    private void Initialize(SerializedProperty property)
                                    {
                                        if (!initialized)
                                        {
                                            widthProperty = property.FindPropertyRelative("width");
                                            heightProperty = property.FindPropertyRelative("height");
                                            cellsProperty = property.FindPropertyRelative("cells");
                                            invalidSquare = MakeSolidIcon(Color.black);
                                            ghostSquare = MakeSolidIcon(new Color(0, 0.5f, 0, 1));
                                            holeSquare = MakeSolidIcon(new Color(0.5f, 0, 0, 1));
                                            solidSquare = MakeSolidIcon(Color.grey);
                                            bool withClampingAttribute = Attribute.IsDefined(fieldInfo, typeof(SolidObjectMask.AutoClampedAttribute));
                                            bool ownerIsBehaviour = property.serializedObject.targetObject is MonoBehaviour;
                                            if (withClampingAttribute && ownerIsBehaviour)
                                            {
                                                clampAgainst = (property.serializedObject.targetObject as MonoBehaviour).GetComponent<Entities.Objects.MapObject>();
                                            }
                                            int width = widthProperty.intValue;
                                            int height = heightProperty.intValue;
                                            if (cellsProperty.arraySize != width * height)
                                            {
                                                cellsProperty.arraySize = width * height;
                                            }
                                            initialized = true;
                                        }
                                    }

                                    private void Resize(uint oldWidth, uint oldHeight, uint newWidth, uint newHeight)
                                    {
                                        SolidnessStatus[] statuses = (SolidnessStatus[])Enum.GetValues(typeof(SolidnessStatus));
                                        SolidnessStatus[] oldStatuses = new SolidnessStatus[oldWidth * oldHeight];
                                        uint index = 0;
                                        for (uint y = 0; y < oldHeight; y++)
                                        {
                                            for (uint x = 0; x < oldWidth; x++)
                                            {
                                                oldStatuses[index] = statuses[cellsProperty.GetArrayElementAtIndex((int)index).enumValueIndex];
                                                index++;
                                            }
                                        }
                                        SolidnessStatus[] newStatuses = SolidObjectMask.Resized(oldStatuses, oldWidth, oldHeight, newWidth, newHeight, fillWith);
                                        if (newStatuses == null)
                                        {
                                            cellsProperty.arraySize = 0;
                                        }
                                        else
                                        {
                                            cellsProperty.arraySize = (int)(newWidth * newHeight);
                                            index = 0;
                                            for (uint y = 0; y < newHeight; y++)
                                            {
                                                for (uint x = 0; x < newWidth; x++)
                                                {
                                                    cellsProperty.GetArrayElementAtIndex((int)index).enumValueIndex = Array.IndexOf(statuses, newStatuses[index++]);
                                                }
                                            }
                                        }
                                    }

                                    // Gets the appropriate image according to the state.
                                    private Texture GetStatusImage(SolidnessStatus status)
                                    {
                                        switch (status)
                                        {
                                            case SolidnessStatus.Solid:
                                                return solidSquare;
                                            case SolidnessStatus.Ghost:
                                                return ghostSquare;
                                            case SolidnessStatus.Hole:
                                                return holeSquare;
                                            default:
                                                return null;
                                        }
                                    }

                                    private void RenderGrid(Vector2 basePosition, uint width, uint height, float squareSize)
                                    {
                                        // Names are overriden here to use appropriately sized squares.
                                        Texture2D invalidSquare = MakeSolidIcon(Color.black, (int)squareSize);
                                        Texture2D ghostSquare = MakeSolidIcon(new Color(0, 0.5f, 0, 1), (int)squareSize);
                                        Texture2D holeSquare = MakeSolidIcon(new Color(0.5f, 0, 0, 1), (int)squareSize);
                                        Texture2D solidSquare = MakeSolidIcon(Color.grey, (int)squareSize);
                                        Vector2 size = Vector2.one * (squareSize - 1);
                                        GUIStyle label = new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0) };
                                        SolidnessStatus[] statuses = (SolidnessStatus[])Enum.GetValues(typeof(SolidnessStatus));

                                        for (uint y = 0; y < 8; y++)
                                        {
                                            uint mappedY = scrollY + 7 - y;
                                            if (mappedY >= height)
                                            {
                                                for (uint x = 0; x < 8; x++)
                                                {
                                                    GUI.Label(new Rect(basePosition + new Vector2(x, y) * squareSize, size), invalidSquare, label);
                                                }
                                            }
                                            else
                                            {
                                                for (uint x = 0; x < 8; x++)
                                                {
                                                    Vector2 offset = new Vector2(x, y) * squareSize;
                                                    offset.x = (int)offset.x;
                                                    offset.y = (int)offset.y;
                                                    uint mappedX = scrollX + x;
                                                    if (mappedX >= width)
                                                    {
                                                        GUI.Label(new Rect(basePosition + offset, size), invalidSquare, label);
                                                    }
                                                    else
                                                    {
                                                        int index = (int)(mappedY * width + mappedX);
                                                        SolidnessStatus status = statuses[cellsProperty.GetArrayElementAtIndex((int)index).enumValueIndex];
                                                        if (status == SolidnessStatus.Mask)
                                                        {
                                                            status = fillWith;
                                                        }
                                                        Texture2D image = null;
                                                        switch (status)
                                                        {
                                                            case SolidnessStatus.Solid:
                                                                image = solidSquare;
                                                                break;
                                                            case SolidnessStatus.Ghost:
                                                                image = ghostSquare;
                                                                break;
                                                            case SolidnessStatus.Hole:
                                                                image = holeSquare;
                                                                break;
                                                        }
                                                        if (GUI.RepeatButton(new Rect(basePosition + offset, size), new GUIContent(image), label))
                                                        {
                                                            cellsProperty.GetArrayElementAtIndex(index).enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(SolidnessStatus)), fillWith);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    private float GetCurrentWidth()
                                    {
                                        // The value "15" has to be tested against 2017.3.
                                        return EditorGUIUtility.currentViewWidth - 15 * EditorGUI.indentLevel;
                                    }

                                    private Vector2 Height2Vector(float height)
                                    {
                                        return new Vector2(0, height);
                                    }

                                    private void MakeFillButton(Rect position, Texture2D image, string text, SolidnessStatus status, GUIStyle baseStyle)
                                    {
                                        GUIStyle style = baseStyle;
                                        if (fillWith == status)
                                        {
                                            style = new GUIStyle(style);
                                            style.normal.background = style.active.background;
                                        }
                                        GUIContent content = new GUIContent(text, image);
                                        if (GUI.Button(position, content, style))
                                        {
                                            fillWith = status;
                                        }
                                    }

                                    private void FillWholeMask()
                                    {
                                        SolidnessStatus[] statuses = (SolidnessStatus[])Enum.GetValues(typeof(SolidnessStatus));
                                        uint width = (uint)widthProperty.intValue;
                                        uint height = (uint)heightProperty.intValue;
                                        uint index = 0;
                                        for (uint x = 0; x < width; x++)
                                        {
                                            for (uint y = 0; y < height; y++)
                                            {
                                                cellsProperty.GetArrayElementAtIndex((int)index).enumValueIndex = Array.IndexOf(statuses, fillWith);
                                                index++;
                                            }
                                        }
                                    }

                                    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                                    {
                                        Initialize(property);
                                        float availableWidth = position.width;
                                        float slHeight = EditorGUIUtility.singleLineHeight;
                                        float propHeight = 0;
                                        Vector2 xyPos = position.position;
                                        Vector2 xySpacing = Height2Vector(EditorGUIUtility.standardVerticalSpacing);
                                        Vector2 xySLHeight = Height2Vector(slHeight);
                                        EditorGUI.BeginProperty(position, label, property);
                                        EditorGUI.BeginDisabledGroup(clampAgainst != null);
                                        // Keep current dimensions
                                        uint oldWidth = (uint)widthProperty.intValue;
                                        uint oldHeight = (uint)heightProperty.intValue;
                                        // Width
                                        propHeight = EditorGUI.GetPropertyHeight(widthProperty);
                                        EditorGUI.PropertyField(new Rect(xyPos, new Vector2(position.width, propHeight)), widthProperty, true);
                                        xyPos += xySpacing + Height2Vector(propHeight);
                                        // Height
                                        propHeight = EditorGUI.GetPropertyHeight(heightProperty);
                                        EditorGUI.PropertyField(new Rect(xyPos, new Vector2(position.width, propHeight)), heightProperty, true);
                                        xyPos += xySpacing + Height2Vector(propHeight);
                                        EditorGUI.EndDisabledGroup();
                                        // Clamp current dimensions
                                        widthProperty.intValue = Values.Clamp(1, widthProperty.intValue, 32767);
                                        heightProperty.intValue = Values.Clamp(1, heightProperty.intValue, 32767);
                                        if (clampAgainst)
                                        {
                                            widthProperty.intValue = (int)clampAgainst.Width;
                                            heightProperty.intValue = (int)clampAgainst.Height;
                                        }
                                        // Compare dimensions and perhaps resize
                                        uint newWidth = (uint)widthProperty.intValue;
                                        uint newHeight = (uint)heightProperty.intValue;
                                        if (oldWidth != newWidth || oldHeight != newHeight)
                                        {
                                            Resize(oldWidth, oldHeight, newWidth, newHeight);
                                        }
                                        else if (cellsProperty.arraySize != (oldWidth * oldHeight))
                                        {
                                            // This will occur typically on first GUI iteration only.
                                            cellsProperty.arraySize = (int)(newWidth * newHeight);
                                        }
                                        // Clamp scrolling coordinates to {1, .., new width - 8}
                                        //                            and {1, .., new height - 8}
                                        uint maxX = newWidth > 8 ? newWidth - 8 : 0;
                                        uint maxY = newHeight > 8 ? newHeight - 8 : 0;
                                        scrollX = Values.Clamp(0, scrollX, maxX);
                                        scrollY = Values.Clamp(0, scrollY, maxY);
                                        // Grid (and scrollbars)
                                        float squareSize = (int)((availableWidth - slHeight) / 8);
                                        float normalizedSize = squareSize * 8;
                                        // This value is at least the default value and at least in Unity 2017.3.
                                        // It should be revised: perhaps may be changed by styling (then the code
                                        // must include a change for that to force it to 1).
                                        const uint STEP_SIZE = 10;
                                        EditorGUI.BeginDisabledGroup(maxX == 0);
                                        scrollX = (uint)GUI.HorizontalScrollbar(new Rect(xyPos + new Vector2(slHeight, normalizedSize), new Vector2(normalizedSize, slHeight)), scrollX * STEP_SIZE, STEP_SIZE, 0, maxX * STEP_SIZE + STEP_SIZE, GUI.skin.horizontalScrollbar) / STEP_SIZE;
                                        EditorGUI.EndDisabledGroup();
                                        EditorGUI.BeginDisabledGroup(maxY == 0);
                                        scrollY = (uint)GUI.VerticalScrollbar(new Rect(xyPos, new Vector2(slHeight, normalizedSize)), scrollY * STEP_SIZE, STEP_SIZE, maxY * STEP_SIZE + STEP_SIZE, 0, GUI.skin.verticalScrollbar) / STEP_SIZE;
                                        EditorGUI.EndDisabledGroup();
                                        RenderGrid(xyPos + new Vector2(slHeight, 0), newWidth, newHeight, squareSize);
                                        xyPos += xySpacing + Height2Vector(position.width);
                                        // Position (x, y) -> (xf, yf)
                                        EditorGUI.LabelField(new Rect(xyPos, new Vector2(position.width, slHeight)), string.Format(
                                            "Left-Down: ({0}, {1}) - Right-Up: ({2}, {3})", scrollX, scrollY, Mathf.Min(scrollX + 7, maxX), Mathf.Min(scrollY + 7, maxY)
                                        ));
                                        xyPos += xySpacing + xySLHeight;
                                        // Buttons
                                        float width3 = position.width / 3;
                                        MakeFillButton(new Rect(xyPos.x, xyPos.y, width3, slHeight), solidSquare, "Solid", SolidnessStatus.Solid, EditorStyles.miniButtonLeft);
                                        MakeFillButton(new Rect(xyPos.x + width3, xyPos.y, width3, slHeight), ghostSquare, "Ghost", SolidnessStatus.Ghost, EditorStyles.miniButtonMid);
                                        MakeFillButton(new Rect(xyPos.x + 2 * width3, xyPos.y, width3, slHeight), holeSquare, "Hole", SolidnessStatus.Hole, EditorStyles.miniButtonRight);
                                        xyPos += xySpacing + xySLHeight;
                                        if (GUI.Button(new Rect(xyPos, new Vector2(position.width, slHeight)), "Fill mask with selected type"))
                                        {
                                            FillWholeMask();
                                        }
                                        EditorGUI.EndProperty();
                                    }

                                    /// <summary>
                                    ///   Allows caching the same drawer for the same mask property instance.
                                    /// </summary>
                                    /// <param name="property">The property to cache for</param>
                                    /// <returns>true</returns>
                                    public override bool CanCacheInspectorGUI(SerializedProperty property)
                                    {
                                        return true;
                                    }

                                    /// <summary>
                                    ///   Property height for 5 fields: 4 having standard size, and 1 having the height
                                    ///   being the same as the GUI width.
                                    /// </summary>
                                    /// <param name="property">The property being calculated for</param>
                                    /// <param name="label">The property label</param>
                                    /// <returns>The height involving all the 5 fields</returns>
                                    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                                    {
                                        Initialize(property);
                                        return 3 * EditorGUIUtility.standardVerticalSpacing + GetCurrentWidth() +
                                               EditorGUI.GetPropertyHeight(widthProperty) + EditorGUI.GetPropertyHeight(heightProperty) +
                                               EditorGUIUtility.singleLineHeight;
                                        // Possible bug: Why I don't need to add the two instances of standard single-line size
                                        //               and their corresponding standard vertical spacing?
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
