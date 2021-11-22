using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEditor;

namespace GameMeanMachine.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Maps
        {
            using AlephVault.Unity.Support.Utils;
            using AlephVault.Unity.MenuActions.Utils;

            /// <summary>
            ///   Menu actions to create a map in the scene.
            /// </summary>
            public static class MapUtils
            {
                /// <summary>
                ///   Utility window used to create a Map. It fills its properties, adds optional layers,
                ///     and allows setting the names of the floors.
                /// </summary>
                public class CreateMapWindow : EditorWindow
                {
                    public Transform selectedTransform;
                    private Vector2Int mapSize = new Vector2Int(8, 6);
                    private Vector3 cellSize = Vector3.one;
                    private string mapObjectName = "New Map";
                    private string[] floors = new string[] { "New Floor" };
                    private bool addCeilingsLayer = false;
                    private int strategy = 0;
                    private Vector2 scrollPosition = Vector2.zero;

                    private string[] UpdateFloors(string[] floors)
                    {
                        int newFloorsLength = EditorGUILayout.IntField("Length", floors.Length);
                        if (newFloorsLength < 0) newFloorsLength = 0;
                        string[] newFloors = new string[newFloorsLength];
                        int copyLength = Values.Min(newFloorsLength, floors.Length);
                        for (int i = 0; i < copyLength; i++) newFloors[i] = floors[i];
                        for (int i = copyLength; i < newFloorsLength; i++) newFloors[i] = "New Floor";
                        // Now the update cycles must run
                        for (int i = 0; i < newFloorsLength; i++)
                        {
                            newFloors[i] = MenuActionUtils.EnsureNonEmpty(EditorGUILayout.TextField(string.Format("Floor {0} name", i), newFloors[i]), "Floor " + i);
                        }
                        return newFloors;
                    }

                    private void OnGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle captionLabelStyle = MenuActionUtils.GetCaptionLabelStyle();
                        GUIStyle indentedStyle = MenuActionUtils.GetIndentedStyle();

                        minSize = new Vector2(643, 250);

                        // General settings start here.

                        titleContent = new GUIContent("Wind Rose - Creating a new map");
                        EditorGUILayout.LabelField("This wizard will create a map in the hierarchy of the current scene, under the selected object in the hierarchy.", longLabelStyle);

                        // Map properties.

                        EditorGUILayout.BeginHorizontal();

                        // -> (Direct) map properties start here.

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Map properties", captionLabelStyle);

                        EditorGUILayout.LabelField("This is the name the game object will have when added to the hierarchy.", longLabelStyle);
                        mapObjectName = MenuActionUtils.EnsureNonEmpty(EditorGUILayout.TextField("Object name", mapObjectName), "New Map");

                        EditorGUILayout.LabelField("These are the map properties in the editor. Can be changed later.", longLabelStyle);
                        mapSize = EditorGUILayout.Vector2IntField("Map width (X) and height (Y) [1 to 32767]", mapSize);
                        mapSize = new Vector2Int(Values.Clamp(1, mapSize.x, 32767), Values.Clamp(1, mapSize.y, 32767)); 
                        cellSize = EditorGUILayout.Vector3Field("Cell Size", cellSize);
                        EditorGUILayout.EndVertical();

                        // -> Map layers properties.

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Map layers properties", captionLabelStyle);

                        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);
                        EditorGUILayout.LabelField("Floors, Objects and Visuals layer will be added. There are more layers that can be added as well:", longLabelStyle);
                        addCeilingsLayer = EditorGUILayout.ToggleLeft("Ceilings Layer", addCeilingsLayer);
                        EditorGUILayout.LabelField("These are the names for each of the floors layers to be added in the hierarchy.", longLabelStyle);
                        floors = UpdateFloors(floors);
                        EditorGUILayout.EndScrollView();
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();

                        // End Map properties.
                        // Map strategies.

                        EditorGUILayout.LabelField("Map strategies", captionLabelStyle);
                        strategy = EditorGUILayout.IntPopup(strategy, new string[] { "Simple (includes Solidness and Layout)", "Layout", "Nothing (will be added manually later)" }, new int[] { 0, 1, 2 });

                        // End Map strategies.
                        // Call to action.

                        if (GUILayout.Button("Create Map")) Execute();
                    }

                    private void Execute()
                    {
                        GameObject mapObject = new GameObject(mapObjectName);
                        mapObject.transform.parent = selectedTransform;
                        mapObject.SetActive(false);
                        // Creating the map component & sorting group.
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<SortingGroup>(mapObject);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Map>(mapObject, new Dictionary<string, object>() {
                            { "width", (ushort)mapSize.x },
                            { "height", (ushort)mapSize.y },
                            { "cellSize", cellSize},
                        });
                        // Now, creating the layers as children AND the floors.
                        // 1. Floors layer.
                        GameObject floorLayer = new GameObject("FloorLayer");
                        floorLayer.transform.parent = mapObject.transform;
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Grid>(floorLayer).cellSize = cellSize;
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<SortingGroup>(floorLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<AlephVault.Unity.Support.Authoring.Behaviours.Normalized>(floorLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Layers.Floor.FloorLayer>(floorLayer);
                        foreach (string floorName in floors)
                        {
                            GameObject floor = new GameObject(floorName);
                            floor.transform.parent = floorLayer.transform;
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Tilemap>(floor);
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<TilemapRenderer>(floor);
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<AlephVault.Unity.Support.Authoring.Behaviours.Normalized>(floor);
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.Floors.Floor>(floor);
                        }
                        // 2. Objects layer.
                        GameObject objectsLayer = new GameObject("ObjectsLayer");
                        objectsLayer.transform.parent = mapObject.transform;
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Grid>(objectsLayer).cellSize = cellSize;
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<SortingGroup>(objectsLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<AlephVault.Unity.Support.Authoring.Behaviours.Normalized>(objectsLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Layers.Objects.ObjectsLayer>(objectsLayer);
                        // Creating the strategy holder & strategies.
                        Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategies.ObjectsManagementStrategy mainStrategy = null;
                        switch (strategy)
                        {
                            case 0:
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategies.Base.LayoutObjectsManagementStrategy>(objectsLayer);
                                AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategies.Solidness.SolidnessObjectsManagementStrategy>(objectsLayer);
                                mainStrategy = AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategies.Simple.SimpleObjectsManagementStrategy>(objectsLayer);
                                break;
                            case 1:
                                mainStrategy = AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategies.Base.LayoutObjectsManagementStrategy>(objectsLayer);
                                break;
                            default:
                                break;
                        }
                        Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategyHolder currentHolder = objectsLayer.AddComponent<Authoring.Behaviours.World.Layers.Objects.ObjectsManagementStrategyHolder>();
                        AlephVault.Unity.Layout.Utils.Behaviours.SetObjectFieldValues(currentHolder, new Dictionary<string, object>()
                        {
                            { "strategy", mainStrategy }
                        });
                        // 3. Visuals layer.
                        GameObject visualsLayer = new GameObject("VisualsLayer");
                        visualsLayer.transform.parent = mapObject.transform;
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<SortingGroup>(visualsLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<AlephVault.Unity.Support.Authoring.Behaviours.Normalized>(visualsLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Layers.Visuals.VisualsLayer>(visualsLayer);
                        // 4. Ceilings layer.
                        if (addCeilingsLayer)
                        {
                            GameObject ceilings = new GameObject("CeilingLayer");
                            ceilings.transform.parent = mapObject.transform;
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Grid>(ceilings).cellSize = cellSize;
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<SortingGroup>(ceilings);
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<AlephVault.Unity.Support.Authoring.Behaviours.Normalized>(ceilings);
                            AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<Authoring.Behaviours.World.Layers.Ceiling.CeilingLayer>(ceilings);
                        }
                        // Ok. Now activate the object.
                        mapObject.SetActive(true);
                        Undo.RegisterCreatedObjectUndo(mapObject, "Create Map");
                        Close();
                    }
                }

                /// <summary>
                ///   This method is used in a menu action: GameObject > Wind Rose > Maps > Create Map.
                ///   It creates a map inside the selected object in the hierarchy.
                /// </summary>
                [MenuItem("GameObject/Wind Rose/Maps/Create Map", false, 11)]
                public static void CreateMap()
                {
                    CreateMapWindow window = ScriptableObject.CreateInstance<CreateMapWindow>();
                    window.position = new Rect(new Vector2(110, 250), new Vector2(643, 250));
                    window.selectedTransform = Selection.activeTransform;
                    window.ShowUtility();
                }

                /// <summary>
                ///   Validates the menu item: GameObject > Wind Rose > Maps > Create Map.
                ///   It enables such menu option when no transform is selected in the scene
                ///     hierarchy.
                /// </summary>
                [MenuItem("GameObject/Wind Rose/Maps/Create Map", true)]
                public static bool CanCreateHUD()
                {
                    return Selection.activeTransform == null;
                }
            }
        }
    }
}
