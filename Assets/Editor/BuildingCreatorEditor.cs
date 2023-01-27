using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This custom editor manages input and displays settings

[ExecuteInEditMode]
[CustomEditor(typeof(BuildingCreator))]
public class BuildingCreatorEditor : Editor {

    // Used to repaint views / remake meshes
    bool buildingHasChanged = true;
    bool meshHasChanged = true;

    // Custom inspector
    public override void OnInspectorGUI() {
        // base.OnInspectorGUI();

        // Header
        Rect rect = GUILayoutUtility.GetRect(18, 18, 30f, 30f);
        EditorGUI.ProgressBar(rect, 1, "Building Creator");

        // Debug info foldout
        BC.showSelectionInfo = EditorGUILayout.Foldout(BC.showSelectionInfo, "Debug Info");
        if (BC.showSelectionInfo) {
            EditorGUILayout.Space(5f);
            EditorGUILayout.ToggleLeft("Mouse over line?", SelectionInfo.mouseIsOverLine);
            EditorGUILayout.ToggleLeft("Mouse over point?", SelectionInfo.mouseIsOverPoint);
            EditorGUILayout.ToggleLeft("Point being dragged?", SelectionInfo.pointIsBeingDragged);
            EditorGUILayout.Space(5f);
            EditorGUILayout.IntField("Mouse Over Building Index", SelectionInfo.mouseOverBuildingIndex);
            EditorGUILayout.IntField("Mouse Over Line Index", SelectionInfo.mouseOverLineIndex);
            EditorGUILayout.IntField("Mouse Over Point Index", SelectionInfo.mouseOverPointIndex);
            EditorGUILayout.Space(5f);
            EditorGUILayout.IntField("Selected Building Index", SelectionInfo.buildingIndex);
            if (SelectionInfo.buildingIndex > -1) {
                EditorGUILayout.FloatField("Selected Building Area", SelectedBuilding.points.ToXZ().FindArea2D());
            }
            
        }

        // Settings
        EditorGUILayout.Space(15f);
        BC.handleRadius = EditorGUILayout.Slider("Handle Size", BC.handleRadius, 0f, 10f);
        BC.showOutlines = EditorGUILayout.ToggleLeft(TT("Show Outlines", "Hides/Displays building edges"), BC.showOutlines);
        BC.showGuides = EditorGUILayout.ToggleLeft(TT("Show Guides", "Hides/Displays window guides"), BC.showGuides);
        BC.showMesh = EditorGUILayout.ToggleLeft(TT("Show Mesh", "Hides/Displays building meshes"), BC.showMesh);
        BC.showWindows = EditorGUILayout.ToggleLeft(TT("Show Windows", "Hides/Displays window outlines"), BC.showWindows);

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
            // EditorGUILayout.Space();
            // SelectedBuilding.door = (GameObject)EditorGUILayout.ObjectField("Door", SelectedBuilding.door, typeof(GameObject), true);
            // SelectedBuilding.window = (GameObject)EditorGUILayout.ObjectField("Window", SelectedBuilding.window, typeof(GameObject), true);

            // Options 
            EditorGUILayout.Space();
            SelectedBuilding.inverted = EditorGUILayout.ToggleLeft(TT("Inverted", "Inverts building faces"), SelectedBuilding.inverted);
            SelectedBuilding.height = EditorGUILayout.FloatField("Building Height", SelectedBuilding.height);
            EditorGUILayout.Space();
            SelectedBuilding.topOffset = EditorGUILayout.Slider("Top Offset", SelectedBuilding.topOffset, 0f, 1 - SelectedBuilding.bottomOffset);
            SelectedBuilding.bottomOffset = EditorGUILayout.Slider("Bottom Offset", SelectedBuilding.bottomOffset, 0f, 1 - SelectedBuilding.topOffset);
            SelectedBuilding.edgeOffset = EditorGUILayout.Slider("Edge Offset", SelectedBuilding.edgeOffset, 0f, 1f);
            EditorGUILayout.Space();
            SelectedBuilding.windowHeight = EditorGUILayout.FloatField("Window Height", SelectedBuilding.windowHeight);
            SelectedBuilding.windowWidth = EditorGUILayout.FloatField("Window Width", SelectedBuilding.windowWidth);
            EditorGUILayout.Space();
            SelectedBuilding.verticalGap = EditorGUILayout.FloatField("Vertical Gap", SelectedBuilding.verticalGap);
            SelectedBuilding.horizontalGap = EditorGUILayout.FloatField("Horizontal Gap", SelectedBuilding.horizontalGap);
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
            meshHasChanged = true;
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
                meshHasChanged = true;
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
                meshHasChanged = true;
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
            Undo.RecordObject(BC, "Create Building");
            BC.buildings.Add(new Building());
            SelectionInfo.buildingIndex = BC.buildings.Count - 1;
            meshHasChanged = true;
        }
        
        // Create new point
        void CreateNewPoint() {
            bool mouseIsOverSelectedBuilding = (SelectionInfo.mouseOverBuildingIndex == SelectionInfo.buildingIndex);
            int newPointIndex = (SelectionInfo.mouseIsOverLine && mouseIsOverSelectedBuilding) ? SelectionInfo.mouseOverLineIndex + 1 : SelectedBuilding.points.Count;
            Undo.RecordObject(BC, "Add point");
            SelectedBuilding.points.Insert(newPointIndex, mousePosition);
            SelectionInfo.mouseOverPointIndex = newPointIndex;
            SelectionInfo.mouseOverBuildingIndex = SelectionInfo.buildingIndex;
            SelectPointUnderMouse();
        }

        // Select the building under the mouse
        void SelectBuildingUnderMouse() {
            if (SelectionInfo.mouseOverBuildingIndex != -1) {
                SelectionInfo.buildingIndex = SelectionInfo.mouseOverBuildingIndex;
                buildingHasChanged = true;
                meshHasChanged = true;
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
            meshHasChanged = true;
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
            buildingHasChanged = true;
            meshHasChanged = true;
        }

    }

    // Draws handles
    void Draw() {
        // For each building
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
                    if (thisPointIsBeingDragged) {
                        Handles.color = dragged;
                    } else if (mouseIsOverThisPoint) {
                        Handles.color = hover;
                    } else if (currentBuildingIsSelected) {
                        Handles.color = activeLine;
                    } else {
                        Handles.color = deselected;
                    }
                    Handles.DrawDottedLine(thisPoint, aboveThisPoint, 4f);
                }
                
                // Guides
                if (BC.showGuides && currentBuildingIsSelected) {
                    // Top and Bottom offset guides
                    Vector3 topHeight = Vector3.up * currentBuilding.height * (1 - currentBuilding.topOffset);
                    Vector3 bottomHeight = Vector3.up * currentBuilding.height * (currentBuilding.bottomOffset);
                    Handles.color = Color.green;
                    Handles.DrawDottedLine(thisPoint + topHeight, nextPoint + topHeight, 8);
                    Handles.DrawDottedLine(thisPoint + bottomHeight, nextPoint + bottomHeight, 8);
                    
                    // Edge offset guides
                    Vector3 thisToNext = nextPoint - thisPoint;
                    Vector3 p1 = thisPoint +  thisToNext * currentBuilding.edgeOffset * 0.5f;
                    Vector3 p2 = nextPoint + -thisToNext * currentBuilding.edgeOffset * 0.5f;
                    Handles.color = Color.green;
                    Handles.DrawDottedLine(p1, p1 + Vector3.up * currentBuilding.height, 8);
                    Handles.DrawDottedLine(p2, p2 + Vector3.up * currentBuilding.height, 8);
                }

                // Windows
                if (BC.showWindows 
                    && currentBuildingIsSelected 
                    && currentBuilding.edgeOffset != 1 
                    && currentBuilding.topOffset + currentBuilding.bottomOffset < 0.99) {

                    // Perpendicular lines
                    Vector3 thisToNext = nextPoint - thisPoint;
                    Vector3 direction = thisToNext.normalized;
                    Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x);
                    Vector3 middle = thisPoint + thisToNext * 0.5f;
                    Handles.color = Color.blue;
                    Handles.DrawLine(middle, middle + perpendicular * 10);

                    // Usable Space
                    Vector3 topHeight = Vector3.up * currentBuilding.height * (1 - currentBuilding.topOffset);
                    Vector3 bottomHeight = Vector3.up * currentBuilding.height * (currentBuilding.bottomOffset);
                    float xAvailable = thisToNext.magnitude - thisToNext.magnitude * SelectedBuilding.edgeOffset;
                    Handles.color = new Color(1, 0, 1);
                    Vector3 p1 = bottomHeight + thisPoint +  direction * (thisToNext.magnitude - xAvailable) / 2;
                    Vector3 p2 = bottomHeight + nextPoint + -direction * (thisToNext.magnitude - xAvailable) / 2;
                    Vector3 p3 = topHeight + thisPoint +  direction * (thisToNext.magnitude - xAvailable) / 2;
                    Vector3 p4 = topHeight + nextPoint + -direction * (thisToNext.magnitude - xAvailable) / 2;
                    Handles.DrawLine(p1, p2, 2f);
                    Handles.DrawLine(p3, p4, 2f);
                    Handles.DrawLine(p1, p3, 2f);
                    Handles.DrawLine(p2, p4, 2f);
 
                }

            }
        }
        if (BC.showMesh && meshHasChanged) {
            ClearMesh();
            SetBuildingMeshes();
            UpdateComponents();
        } else if (!BC.showMesh) {
            ClearMesh();
        }
        meshHasChanged = false;
        buildingHasChanged = false;
    }

    // Create mesh of all buildings
    void SetBuildingMeshes() {
        for (int currentBuildingIndex = 0; currentBuildingIndex < BC.buildings.Count; currentBuildingIndex++) {
            // Current building of this iteration of the loop
            Building currentBuilding = BC.buildings[currentBuildingIndex];
            // Bool to signify the winding order of the buildings points
            bool isClockWise = currentBuilding.points.ToXZ().FindArea2D() > 0;
            // List of vertices of the building
            List<Vector3> vertices = new List<Vector3>();
            // List of triangles of the building
            List<int> triangles = new List<int>();
            // Height of the building
            Vector3 h = currentBuilding.height * Vector3.up;
            
            // Clear mesh
            currentBuilding.mesh = new Mesh();
            currentBuilding.mesh.Clear();
            
            // Wall triangles
            for (int currentPointIndex = 0, totalVerts = 0; currentPointIndex < currentBuilding.points.Count; currentPointIndex++) {
                // First point
                Vector3 p0 = currentBuilding.points.GetItem(currentPointIndex);     
                // Second point (above first point)
                Vector3 p1 = p0 + h;
                // Third point
                Vector3 p2 = currentBuilding.points.GetItem(currentPointIndex + 1);
                // Fourth point (above third point)
                Vector3 p3 = p2 + h;
                // Temp array for wall points
                Vector3[] verts = {p0, p1, p2, p3};
                // Temp list for wall triangles
                List<int> tris = new List<int>{totalVerts, totalVerts + 2, totalVerts + 1, totalVerts + 1, totalVerts + 2, totalVerts + 3};
                // Reverse tris list if the building's winding order is counter-clockwise
                if (!isClockWise) {
                    tris.Reverse();
                }
                // Reverse tris list if the building's inverted option is checked
                if (currentBuilding.inverted) {
                    tris.Reverse();
                }
                // Add temp array/list to their respective array/list
                vertices.AddRange(verts);
                triangles.AddRange(tris);
                // Increment total vertex count
                totalVerts += 4;
            }

            // Roof triangles
            if (currentBuilding.points != null && currentBuilding.points.Count > 2) {
                // Temp roof vertices array
                List<Vector3> verts = currentBuilding.points.Map(a => a + h);
                if (verts.ToXZ().FindArea2D() < 0) {
                    verts.Reverse();
                }
                // Temp roof triangles list
                List<int> tris = verts.ToXZ().Triangulate().Map(a => a + vertices.Count);
                // Reverse tris list if the building's inverted option is checked
                if (currentBuilding.inverted) {
                    tris.Reverse();
                }
                // Add temp array/list to their respective array/list
                vertices.AddRange(verts);
                triangles.AddRange(tris);
            }

            // Apply to mesh
            currentBuilding.mesh.vertices = vertices.ToArray();
            currentBuilding.mesh.triangles = triangles.ToArray();
        }
    }

    // Update shared mesh
    void UpdateComponents() {
        BC.meshFilter.sharedMesh.Clear();
        CombineInstance[] meshes = new CombineInstance[BC.buildings.Count];
        for (int i = 0; i < BC.buildings.Count; i++) {
            meshes[i].mesh = BC.buildings[i].mesh;
        }
        BC.meshFilter.sharedMesh.CombineMeshes(meshes, false, false, false);
        BC.meshFilter.sharedMesh.RecalculateNormals();

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

    // When the editor is enabled
    void OnEnable() {
        // Subscribe undo/redo function
        Undo.undoRedoPerformed += OnUndoOrRedo;
        // Hides tools that get in the way during building editing
        Tools.hidden = true;
        // Fixes a bug that causes the mesh to disappear on reload
        meshHasChanged = true;
    }

    // When the editor is disabled
    void OnDisable() {
        // Unsubscribe undo/redo function
        Undo.undoRedoPerformed -= OnUndoOrRedo;
        // Shows tools again when script is disabled
        Tools.hidden = false;
    }

    // Fixes Undo/Redo bugs
    void OnUndoOrRedo() {
        // Fixes selection index out of bounds when undoing building creation
        if (SelectionInfo.buildingIndex >= BC.buildings.Count || SelectionInfo.buildingIndex == -1) {
            SelectionInfo.buildingIndex = BC.buildings.Count - 1;
        }
        // Update building and mesh on undo/redo
        meshHasChanged = true;
        buildingHasChanged = true;
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

    // Shorthand getting GUIContent with a tooltip
    GUIContent TT(string text, string tooltip) {
        return new GUIContent(text, tooltip);
    }

}
