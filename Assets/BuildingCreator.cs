using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is used for storing building information and settings

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingCreator : MonoBehaviour {

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    // List of buildings
    public List<Building> buildings = new List<Building>();

    // Singleton functionality
    // https://gamedevbeginner.com/singletons-in-unity-the-right-way/
    public static BuildingCreator main { get; private set; }
    void OnEnable() {
        if (main == null) {
            main = this;
            meshFilter = this.gameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
            this.gameObject.transform.position = Vector3.zero;
        }
    }
}

// Class for individual building
[System.Serializable]
public class Building {
    public string name;
    // List of points for the buildin
    public List<Vector3> points = new List<Vector3>();
    // Mesh of the building
    public Mesh buildingMesh = null;
    public Mesh windowMesh = null;
    // Material of the building
    public Material buildingMaterial = null;
    public Material windowMaterial = null;
    // Windows and doors
    public GameObject window;
    public GameObject door;
    // Building Settings
    public bool inverted = false;
    public float height = 10;
    // Visibility
    public bool showBuildingMesh = false;
    public bool showWindowMesh = false;
    // Window settings
    public float windowHeight = 5;
    public float windowWidth = 5;
    public float windowDepth = 1;
    // Offsets
    public float topOffset = 0.2f;
    public float bottomOffset = 0.2f;
    public float edgeOffset = 0.1f;
    // Gaps
    public float horizontalGap = 1;
    public float verticalGap = 1;
    // Constructor
    public Building(string name = "Untitled") {
        this.name = name;
    }
}

// This class stores selection information
public static class SelectionInfo {
    // Drag
    public static bool pointIsBeingDragged = false;
    public static Vector3 positionAtDragStart;
    // Selection
    public static int buildingIndex;
    // Mouse Over
    public static int mouseOverBuildingIndex = -1;
    public static int mouseOverPointIndex = -1;
    public static int mouseOverLineIndex = -1;
    public static bool mouseIsOverPoint = false;
    public static bool mouseIsOverLine = false;
}

// This class stores info used for the inspector menu
public static class BCMenu {
    public static int mode;
    public static bool showDebugInfo = false; 
    public static bool showViewSettings = false;
    public static bool showOutline = true;
    public static bool showGuides = false;
    public static bool showHandles = true;
    public static float handleRadius = 1f;
    public static bool showBuildingsList = false;
    public static bool showSelectedBuildingInfo = false;
    public static bool showWindowSettings = false;
}