using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    // List of points in the building
    public List<Vector3> points = new List<Vector3>();
    // List of doors on the building
    public List<Door> doors = new List<Door>();
    // Meshes of the building
    public Mesh buildingMesh = null;
    public Mesh windowMesh = null;
    // Material of the building
    public Material buildingMaterial = null;
    public Material windowMaterial = null;
    public Material doorMaterial = null;
    // Building Settings
    public bool inverted = false;
    public float height = 10;
    // Visibility
    public bool showBuildingMesh = false;
    public bool showWindowMesh = false;
    public bool showDoorMesh = false;
    // Window settings
    public float windowHeight = 5;
    public float windowWidth = 5;
    public float windowDepth = 0.1f;
    // Offsets
    public float topOffset = 0;
    public float bottomOffset = 0f;
    public float edgeOffset = 0f;
    // Gaps
    public float horizontalGap = 1;
    public float verticalGap = 1;
    // Constructor
    public Building(string name = "Untitled") {
        this.name = name;
    }
    public void MoveBy(Vector3 displacement) {
        this.points = this.points.Map(a => a + displacement);
    }
    public Vector3 CenterPoint { get {
        return new Vector3(
            this.points.Average(v => v.x),
            this.points.Average(v => v.y),
            this.points.Average(v => v.z)
        );
    }}
}

[System.Serializable]
public class Door {

    public int wallIndex = 0;
    
    public float height = 1;
    public float width = 1;
    public float depth = 0.1f;
    public float position = 0;

}

// This class stores selection information
public static class SelectionInfo {
    // Drag
    public static bool mouseIsBeingDragged = false;
    public static Vector3 positionAtDragStart;
    public static List<Vector3> pointsAtDragStart;
    public static Vector3 centerAtDragStart { get {
        if (buildingIndex == -1)
            return Vector3.zero;
        else
            return new Vector3(
                pointsAtDragStart.Average(v => v.x),
                pointsAtDragStart.Average(v => v.y),
                pointsAtDragStart.Average(v => v.z)
            );
    }}
    // Selection
    public static int buildingIndex;
    public static int doorIndex;
    // Mouse Over
    public static int mouseOverBuildingIndex = -1;
    public static int mouseOverPointIndex = -1;
    public static int mouseOverLineIndex = -1;
    public static bool mouseIsOverPoint = false;
    public static bool mouseIsOverLine = false;
}

// This class stores info used for the inspector menu
public static class BCMenu {
    // [0: "Shape Mode", 1: "Move Mode"]
    public static int mode = 0;
    // Is the debug info foldout visible?
    public static bool showDebugInfo;
    // Is the view settings foldout visible?
    public static bool showViewSettings;
    // Is the 3D outline visible?
    public static bool showOutline3D;
    // Is the 2D outline visible?
    public static bool showOutline2D = true;
    // Are guides visible?
    public static bool showGuides = false;
    // Are handles visible?
    public static bool showHandles = true;
    // Radius to use for the handles
    public static float handleRadius = 1f;
    // Is the building list foldout visible?
    public static bool showBuildingsList;
    // Is the selected building foldout visible?
    public static bool showSelectedBuildingInfo;
    // Is the window settings foldout visible?
    public static bool showWindowSettings;
    // Is the door settings foldout visible?
    public static bool showDoorSettings;
}