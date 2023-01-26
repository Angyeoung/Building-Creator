using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is used for storing building information and settings

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingCreator : MonoBehaviour {

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

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

    // List of buildings
    public List<Building> buildings = new List<Building>();

    // Settings
    public float handleRadius = 0.5f;
    public bool showOutlines = true;
    public bool showGuides = false;
    public bool showMesh = false;

    // Dropdowns
    public bool showSelectionInfo = false; 
    public bool showBuildingsList = false;
    public bool showSelectedBuildingInfo = false;

}

// Building information and settings

[System.Serializable]
public class Building {
    // List of points for the building
    public List<Vector3> points = new List<Vector3>();
    
    // Mesh of the building
    public Mesh mesh = null;
    // Material of the building
    public Material buildingMaterial;
    // Windows and doors
    public GameObject window;
    public GameObject door;

    // Building Settings
    public bool inverted = false;
    public float height = 5;

    // Window settings
    public float windowHeight;
    public float windowWidth;
    // Offsets
    public float topOffset = 0;
    public float bottomOffset = 0;
    public float edgeOffset = 0;
    // Gaps
    public float horizontalGap = 1;
    public float verticalGap = 1;

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