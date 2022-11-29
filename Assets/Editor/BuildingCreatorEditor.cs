using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This custom editor manages input and displays settings

[ExecuteInEditMode]
[CustomEditor(typeof(BuildingCreator))]
public class BuildingCreatorEditor : Editor {

    // Used to repaint views / remake meshes
    bool buildingHasChanged = false;

    // Custom inspector
    public override void OnInspectorGUI() {
        // base.OnInspectorGUI();

        // Header
        Rect rect = GUILayoutUtility.GetRect(18, 18, 30f, 30f);
        EditorGUI.ProgressBar(rect, 1, "Building Creator");

        // Settings
        BC.handleRadius = EditorGUILayout.Slider("Handle Size", BC.handleRadius, 0f, 5f);
        BC.showOutlines = EditorGUILayout.Toggle("Show Outlines", BC.showOutlines);
        BC.showMesh = EditorGUILayout.Toggle("Show Mesh", BC.showMesh);

        // Selection info foldout
        EditorGUILayout.Space(15f);
        BC.showSelectionInfo = EditorGUILayout.Foldout(BC.showSelectionInfo, "Selection Info");
        if (BC.showSelectionInfo) {
            EditorGUILayout.Toggle("Mouse over line?", SelectionInfo.mouseIsOverLine);
            EditorGUILayout.Toggle("Mouse over point?", SelectionInfo.mouseIsOverPoint);
            EditorGUILayout.Toggle("Point being dragged?", SelectionInfo.pointIsBeingDragged);
            EditorGUILayout.Space(5f);
            EditorGUILayout.IntField("Mouse Over Building Index", SelectionInfo.mouseOverBuildingIndex);
            EditorGUILayout.IntField("Mouse Over Line Index", SelectionInfo.mouseOverLineIndex);
            EditorGUILayout.IntField("Mouse Over Point Index", SelectionInfo.mouseOverPointIndex);
            EditorGUILayout.Space(5f);
            EditorGUILayout.IntField("Selected Building Index", SelectionInfo.buildingIndex);
        }

        // Buildings list foldout
        EditorGUILayout.Space(15f);
        BC.showBuildingsList = EditorGUILayout.Foldout(BC.showBuildingsList, "Buildings List");
        int buildingDeleteIndex = -1;
        if (BC.showBuildingsList) {
            for (int i = 0; i < BC.buildings.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Building " + (i + 1));
                GUI.enabled = (i != SelectionInfo.buildingIndex);
                if (GUILayout.Button("Select")) {
                    Undo.RecordObject(BC, "Select Building");
                    SelectionInfo.buildingIndex = i;
                }
                GUI.enabled = true;
                if (GUILayout.Button("Delete")) {
                    buildingDeleteIndex = i;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(" ");
            if (GUILayout.Button("Deselect")) {
                SelectionInfo.buildingIndex = -1;
            }
            GUILayout.EndHorizontal();
        }

        // Selected building info foldout
        EditorGUILayout.Space(15f);
        BC.showSelectedBuildingInfo = EditorGUILayout.Foldout(BC.showSelectedBuildingInfo, "Selected Building Info");
        if (BC.showSelectedBuildingInfo && SelectionInfo.buildingIndex != -1) {
            // Material
            EditorGUILayout.Space();
            SelectedBuilding.buildingMaterial = (Material)EditorGUILayout.ObjectField("Material", SelectedBuilding.buildingMaterial, typeof(Material), true);

            // Assets
            EditorGUILayout.Space();
            SelectedBuilding.door = (GameObject)EditorGUILayout.ObjectField("Door", SelectedBuilding.door, typeof(GameObject), true);
            SelectedBuilding.window = (GameObject)EditorGUILayout.ObjectField("Window", SelectedBuilding.window, typeof(GameObject), true);

            EditorGUILayout.Space();
            SelectedBuilding.inverted = EditorGUILayout.Toggle("Inverted", SelectedBuilding.inverted);

            EditorGUILayout.Space();
            SelectedBuilding.edgeOffset = EditorGUILayout.FloatField("Edge Offset", SelectedBuilding.edgeOffset);
            SelectedBuilding.gap = EditorGUILayout.FloatField("Gap", SelectedBuilding.gap);
            SelectedBuilding.height = EditorGUILayout.FloatField("Height", SelectedBuilding.height);
        }

        // Delete buildings if needed (While maintaining proper selection)
        if (buildingDeleteIndex != -1) {
            Undo.RecordObject(BC, "Delete Building");
            BC.buildings.RemoveAt(buildingDeleteIndex);
            if (SelectionInfo.buildingIndex == buildingDeleteIndex) SelectionInfo.buildingIndex = -1;
            else if (SelectionInfo.buildingIndex > buildingDeleteIndex) SelectionInfo.buildingIndex--;
        }

        // If settings were changed repaint Scene view
        if (GUI.changed) {
            buildingHasChanged = true;
            SceneView.RepaintAll();
        }
    
    }

    // Scene view event handler
    void OnSceneGUI() {
        Event guiEvent = Event.current;
        if (guiEvent.type == EventType.Repaint)
            Draw();
        else if (guiEvent.type == EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        else {
            HandleInput(guiEvent);
            if (buildingHasChanged) {
                HandleUtility.Repaint();
                Repaint();
            }
        }
    }

    // Handles input
    void HandleInput(Event guiEvent) {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneHeight = 0;
        float distanceToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(distanceToDrawPlane);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift) 
            HandleShiftLeftMouseDown();
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None) 
            HandleLeftMouseDown();
        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0) 
            HandleLeftMouseUp();
        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None) 
            HandleLeftMouseDrag();
        if (!SelectionInfo.pointIsBeingDragged) 
            UpdateMouseOverInfo();

        // Handles LMBD input
        void HandleLeftMouseDown() {
            // if (BC.buildings.Count == 0) CreateNewBuilding();
            // SelectBuildingUnderMouse();
            // if (SelectionInfo.mouseIsOverPoint) SelectPointUnderMouse();
            // else CreateNewPoint();

            if (BC.buildings.Count == 0) {
                CreateNewBuilding();
                CreateNewPoint();
            } else if (SelectionInfo.mouseIsOverPoint) {
                SelectBuildingUnderMouse();
                SelectPointUnderMouse();
            } else if (SelectionInfo.buildingIndex == -1 && !SelectionInfo.mouseIsOverLine) {
                CreateNewBuilding();
                CreateNewPoint();
            } else {
                SelectBuildingUnderMouse();
                CreateNewPoint();
            }
        }

        // Handles LMBU input
        void HandleLeftMouseUp() {
            if (SelectionInfo.pointIsBeingDragged) {
                SelectedBuilding.points[SelectionInfo.mouseOverPointIndex] = SelectionInfo.positionAtDragStart;
                Undo.RecordObject(BC, "Move point");
                SelectedBuilding.points[SelectionInfo.mouseOverPointIndex] = mousePosition;
                SelectionInfo.pointIsBeingDragged = false;

                buildingHasChanged = true;
            }
        }

        // Handles Shift + LMBD input
        void HandleShiftLeftMouseDown() {
            if (SelectionInfo.mouseIsOverPoint) {
                SelectBuildingUnderMouse();
                DeletePointUnderMouse();
            }
            else {
                CreateNewBuilding();
                CreateNewPoint();
            }
        }

        // Handles LMB drag input
        void HandleLeftMouseDrag() {
            if (SelectionInfo.mouseOverPointIndex != -1) {
                SelectedBuilding.points[SelectionInfo.mouseOverPointIndex] = mousePosition;
                buildingHasChanged = true;
            }
        }

        // Updates mouse over info
        void UpdateMouseOverInfo() {
            int mouseOverPointIndex = -1;
            int mouseOverBuildingIndex = -1;
            int mouseOverLineIndex = -1;

            for (int buildingIndex = 0; buildingIndex < BC.buildings.Count; buildingIndex++) {
                Building currentBuilding = BC.buildings[buildingIndex];
                for (int i = 0; i < currentBuilding.points.Count; i++) {
                    if (Vector3.Distance(mousePosition, currentBuilding.points[i]) < BC.handleRadius) {
                        mouseOverPointIndex = i;
                        SelectionInfo.mouseIsOverPoint = true;
                        SelectionInfo.mouseIsOverLine = false;
                        mouseOverBuildingIndex = buildingIndex;
                        break;
                    }
                }
            }

            if (mouseOverPointIndex != SelectionInfo.mouseOverPointIndex || mouseOverBuildingIndex != SelectionInfo.mouseOverBuildingIndex) {
                SelectionInfo.mouseOverBuildingIndex = mouseOverBuildingIndex;
                SelectionInfo.mouseOverPointIndex = mouseOverPointIndex;
                SelectionInfo.mouseIsOverPoint = (mouseOverPointIndex != -1);
                buildingHasChanged = true;
            }

            if (SelectionInfo.mouseIsOverPoint) {
                SelectionInfo.mouseIsOverLine = false;
                SelectionInfo.mouseOverLineIndex = -1;
            } else {
                float closestLineDistance = BC.handleRadius;
                for (int buildingIndex = 0; buildingIndex < BC.buildings.Count; buildingIndex++) {
                    Building currentBuilding = BC.buildings[buildingIndex];
                    for (int i = 0; i < currentBuilding.points.Count; i++) {
                        Vector3 nextPointInBuilding = currentBuilding.points[(i+1) % currentBuilding.points.Count];
                        float distanceFromMouseToLine = HandleUtility.DistancePointToLineSegment(
                            mousePosition.ToXZ(),
                            currentBuilding.points[i].ToXZ(),
                            nextPointInBuilding.ToXZ()
                        );            
                        if (distanceFromMouseToLine < closestLineDistance) {
                            closestLineDistance = distanceFromMouseToLine;
                            mouseOverLineIndex = i; //
                            mouseOverBuildingIndex = buildingIndex;
                        }
                    }
                }

                if (SelectionInfo.mouseOverLineIndex != mouseOverLineIndex || SelectionInfo.mouseOverBuildingIndex != mouseOverBuildingIndex) {
                    SelectionInfo.mouseOverBuildingIndex = mouseOverBuildingIndex;
                    SelectionInfo.mouseOverLineIndex = mouseOverLineIndex;
                    SelectionInfo.mouseIsOverLine = (mouseOverLineIndex != -1);
                }
            }
        }

        // Create new building
        void CreateNewBuilding() {
            Undo.RecordObject(BuildingCreator.main, "Create Building");
            BC.buildings.Add(new Building());
            SelectionInfo.buildingIndex = BC.buildings.Count - 1;
        }
        
        // Create new point
        void CreateNewPoint() {
            bool mouseIsOverSelectedBuilding = (SelectionInfo.mouseOverBuildingIndex == SelectionInfo.buildingIndex);
            int newPointIndex = (SelectionInfo.mouseIsOverLine && mouseIsOverSelectedBuilding) ? SelectionInfo.mouseOverLineIndex + 1 : SelectedBuilding.points.Count;
            Undo.RecordObject(BC, "Add point");
            SelectedBuilding.points.Insert(newPointIndex, mousePosition);
            SelectionInfo.mouseOverPointIndex = newPointIndex;
            SelectionInfo.mouseOverBuildingIndex = SelectionInfo.buildingIndex;
            buildingHasChanged = true;
            SelectPointUnderMouse();
        }

        // Select the building under the mouse
        void SelectBuildingUnderMouse() {
            if (SelectionInfo.mouseOverBuildingIndex != -1) {
                SelectionInfo.buildingIndex = SelectionInfo.mouseOverBuildingIndex;
                buildingHasChanged = true;
            }
        }

        // Select the point under the mouse
        void SelectPointUnderMouse() {
            SelectionInfo.pointIsBeingDragged = true;
            SelectionInfo.mouseIsOverPoint = true;
            SelectionInfo.mouseIsOverLine = false;
            SelectionInfo.mouseOverLineIndex = -1;
            SelectionInfo.positionAtDragStart = SelectedBuilding.points[SelectionInfo.mouseOverPointIndex];
            buildingHasChanged = true;
        }

        // Delete point under mouse
        void DeletePointUnderMouse() {
            Undo.RecordObject(BC, "Delete point");
            SelectedBuilding.points.RemoveAt(SelectionInfo.mouseOverPointIndex);
            if (SelectedBuilding.points.Count == 0) {
                BC.buildings.Remove(SelectedBuilding);
                SelectionInfo.mouseOverBuildingIndex = -1;
                SelectionInfo.buildingIndex = -1;
            }
            SelectionInfo.pointIsBeingDragged = false;
            SelectionInfo.mouseIsOverPoint = false;
        }

    }

    // Draws lines and discs to the scene view
    void Draw() {
        for (int buildingIndex = 0; buildingIndex < BC.buildings.Count; buildingIndex++) {    
            Building currentBuilding = BC.buildings[buildingIndex];
            bool currentBuildingIsSelected = (buildingIndex == SelectionInfo.buildingIndex);
            bool mouseIsOverCurrentBuilding = (buildingIndex == SelectionInfo.mouseOverBuildingIndex);

            // For each point in this building
            for (int i = 0; i < currentBuilding.points.Count; i++) {
                Color deselected = Color.gray, activeLine = Color.red, 
                    hover = Color.blue, dragged = Color.black, idle = Color.white;
                Vector3 thisPoint = currentBuilding.points[i];
                Vector3 nextPoint = currentBuilding.points[(i+1) % currentBuilding.points.Count];
                Vector3 aboveThisPoint = thisPoint + Vector3.up * currentBuilding.height;
                Vector3 aboveNextPoint = nextPoint + Vector3.up * currentBuilding.height;
                bool mouseIsOverThisPoint = (i == SelectionInfo.mouseOverPointIndex && mouseIsOverCurrentBuilding);
                bool mouseIsOverThisLine = (i == SelectionInfo.mouseOverLineIndex && mouseIsOverCurrentBuilding);
                bool thisPointIsBeingDragged = (mouseIsOverThisPoint && SelectionInfo.pointIsBeingDragged);
                
                // Lines
                if (mouseIsOverThisLine) {
                    Handles.color = hover;
                    Handles.DrawLine(thisPoint, nextPoint, 4);
                    if (BC.showOutlines) Handles.DrawLine(aboveThisPoint, aboveNextPoint, 4);
                } else {
                    Handles.color = (currentBuildingIsSelected) ? activeLine : deselected;
                    Handles.DrawDottedLine(thisPoint, nextPoint, 4);
                    if (BC.showOutlines) Handles.DrawDottedLine(aboveThisPoint, aboveNextPoint, 4);
                }

                // Discs
                if (mouseIsOverThisPoint && Event.current.shift) {
                    Handles.color = (SelectionInfo.pointIsBeingDragged) ? dragged : activeLine;
                } else if (mouseIsOverThisPoint) {
                    Handles.color = (SelectionInfo.pointIsBeingDragged) ? dragged : hover;
                } else {
                    Handles.color = (currentBuildingIsSelected) ? idle : deselected;    
                }
                Handles.DrawSolidDisc(thisPoint, Vector3.up, BC.handleRadius);
                
                // Draw Vertical Lines
                if (BC.showOutlines){
                    if (currentBuildingIsSelected) {
                        Handles.color = (thisPointIsBeingDragged) ? dragged : activeLine;
                    } else {
                        Handles.color = deselected;
                    }
                    Handles.DrawDottedLine(thisPoint, aboveThisPoint, 4f);
                }
            }
        }
        if (BC.showMesh) {
            ClearMesh();
            CalculateMesh();
            SetMesh();
        } else {
            ClearMesh();
        }
        buildingHasChanged = false;
    }

    // Create mesh of all buildings
    void CalculateMesh() {
        for (int currentBuildingIndex = 0; currentBuildingIndex < BC.buildings.Count; currentBuildingIndex++) {
            Building currentBuilding = BC.buildings[currentBuildingIndex];
            currentBuilding.mesh = new Mesh();
            currentBuilding.mesh.Clear();

            Vector3[] vertices = new Vector3[currentBuilding.points.Count*2];
            for (int i = 0; i < currentBuilding.points.Count; i++) {
                vertices[i*2] = currentBuilding.points[i];
                vertices[i*2 + 1] = currentBuilding.points[i] + Vector3.up * currentBuilding.height;
            }

            int[] triangles = new int[6 * currentBuilding.points.Count];
            
            for (int i = 0, tris = 0; i < vertices.Length; i+=2) {
                int i1, i2, i3;
                if (currentBuilding.inverted) {
                    i1 = (i+1) % vertices.Length;
                    i2 = (i+2) % vertices.Length;
                    i3 = (i+3) % vertices.Length;
                } else {
                    i1 = (i+2) % vertices.Length;
                    i2 = (i+1) % vertices.Length;
                    i3 = (i+3) % vertices.Length;
                }
                triangles[tris + 0] = i;
                triangles[tris + 1] = i1;
                triangles[tris + 2] = i2;
                triangles[tris + 3] = i2;
                triangles[tris + 4] = i1;
                triangles[tris + 5] = i3;
                tris += 6;
            }

            // Make roof

            currentBuilding.mesh.vertices = vertices;
            currentBuilding.mesh.triangles = triangles;
        }
    }

    // Update shared mesh
    void SetMesh() {
        BC.meshFilter.sharedMesh.Clear();
        CombineInstance[] meshes = new CombineInstance[BC.buildings.Count];
        for (int i = 0; i < BC.buildings.Count; i++) {
            meshes[i].mesh = BC.buildings[i].mesh;
        }
        BC.meshFilter.sharedMesh.CombineMeshes(meshes, false, false, false);
        BC.meshFilter.sharedMesh.RecalculateNormals();
        BC.meshFilter.sharedMesh.RecalculateBounds();  

        // Materials
        List<Material> materials = new List<Material>();
        materials.Clear();
        for (int i = 0; i < BC.buildings.Count; i++) {
            materials.Add(BC.buildings[i].buildingMaterial);
        }
        BC.meshRenderer.sharedMaterials = materials.ToArray();
    }

    // Clear shared mesh
    void ClearMesh() {
        BC.meshFilter.sharedMesh.Clear();
    }

    // Sets mesh filter and stuff
    void FixMesh() {
        BC.meshFilter.mesh = new Mesh();
    }

    // When the editor is enabled
    void OnEnable() {
        Undo.undoRedoPerformed += OnUndoOrRedo;
        Tools.hidden = true;
    }

    // When the editor is disabled
    void OnDisable() {
        Undo.undoRedoPerformed -= OnUndoOrRedo;
        Tools.hidden = false;
    }

    // Fixes Undo/Redo bugs
    void OnUndoOrRedo() {
        if (SelectionInfo.buildingIndex >= BC.buildings.Count || SelectionInfo.buildingIndex == -1) {
            SelectionInfo.buildingIndex = BC.buildings.Count - 1;
        }
    }

    // Shorthand for getting the building creator singleton instance
    BuildingCreator BC {
        get {
            return BuildingCreator.main;
        }
    }

    // Shorthand for getting the selected building
    Building SelectedBuilding {
        get {
            return BuildingCreator.main.buildings[SelectionInfo.buildingIndex];
        }
    }

}
