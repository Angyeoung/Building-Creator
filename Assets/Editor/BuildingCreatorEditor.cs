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
    Vector2 scrollPos;
    Vector2 scrollPos2;

    // Custom inspector
    public override void OnInspectorGUI() {
        // base.OnInspectorGUI();

        // Header
        Rect rect = GUILayoutUtility.GetRect(18, 18, 30f, 30f);
        EditorGUI.ProgressBar(rect, 1, "Building Creator");

        // Edit Modes
        string[] options = {"Shape", "Move", "Rotate"};
        BCMenu.mode = GUILayout.Toolbar(BCMenu.mode, options);

        // Debug info foldout
        BCMenu.showDebugInfo = EditorGUILayout.Foldout(BCMenu.showDebugInfo, "Debug Info");
        if (BCMenu.showDebugInfo) {
            EditorGUILayout.Space(5f);
            EditorGUILayout.ToggleLeft( "M/O line?",       SelectionInfo.mouseIsOverLine     );
            EditorGUILayout.ToggleLeft( "M/O point?",      SelectionInfo.mouseIsOverPoint    );
            EditorGUILayout.ToggleLeft( "Mouse dragging?", SelectionInfo.mouseIsBeingDragged );
            EditorGUILayout.Space(5f);
            EditorGUILayout.IntField( "M/O Building Index", SelectionInfo.mouseOverBuildingIndex );
            EditorGUILayout.IntField( "M/O Line Index",     SelectionInfo.mouseOverLineIndex     );
            EditorGUILayout.IntField( "M/O Point Index",    SelectionInfo.mouseOverPointIndex    );
            EditorGUILayout.Space(5f);
            EditorGUILayout.IntField( "S/B Index", SelectionInfo.buildingIndex );
            if (SelectionInfo.buildingIndex > -1)
                EditorGUILayout.FloatField( "S/B Area", SelectedBuilding.points.ToXZ().FindArea2D() );
            
        }

        // Settings
        EditorGUILayout.Space(15f);
        BCMenu.showViewSettings = EditorGUILayout.Foldout(BCMenu.showViewSettings, "View Settings");
        if (BCMenu.showViewSettings) {
            BCMenu.showOutline2D      = EditorGUILayout.ToggleLeft( "Show 2D Outline",      BCMenu.showOutline2D      );
            BCMenu.showOutline3D      = EditorGUILayout.ToggleLeft( "Show 3D Outline",      BCMenu.showOutline3D      );
            BCMenu.showWindowOutlines = EditorGUILayout.ToggleLeft( "Show Window Outlines", BCMenu.showWindowOutlines );
            BCMenu.showDoorOutlines   = EditorGUILayout.ToggleLeft( "Show Door Outlines",   BCMenu.showDoorOutlines   );
            BCMenu.showGuides         = EditorGUILayout.ToggleLeft( "Show Guides",          BCMenu.showGuides         );
            BCMenu.showHandles        = EditorGUILayout.ToggleLeft( "Show Handles",         BCMenu.showHandles        );
            if (BCMenu.showHandles) BCMenu.handleRadius = EditorGUILayout.Slider("", BCMenu.handleRadius, 0f, 20f);
        }

        // Buildings list foldout
        EditorGUILayout.Space(15f);
        int buildingDeleteIndex = -1;
        BCMenu.showBuildingsList = EditorGUILayout.Foldout(BCMenu.showBuildingsList, "Buildings List");
        if (BCMenu.showBuildingsList) {
            // Scroll view
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(65));  
            for (int i = 0; i < BC.buildings.Count; i++)
            {
                GUILayout.BeginHorizontal();
                BC.buildings[i].name = GUILayout.TextField(BC.buildings[i].name, GUILayout.Width(150));
                // Disable the next button if it represents the selected building
                GUI.enabled = (i != SelectionInfo.buildingIndex);
                if (GUILayout.Button("Select")) {
                    Undo.RecordObject(BC, "Select Building");
                    SelectionInfo.buildingIndex = i;
                }
                // Re-enaable the GUI
                GUI.enabled = true;
                if (GUILayout.Button("Delete")) {
                    buildingDeleteIndex = i;
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            
            // Deselect button
            if (GUILayout.Button("Deselect")) SelectionInfo.buildingIndex = -1;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Disable All Meshes")) {
                for (int i = 0; i < BC.buildings.Count; i++) {
                    BC.buildings[i].showBuildingMesh = false;
                    BC.buildings[i].showWindowMesh = false;
                }
            }
            if (GUILayout.Button("Enable All Meshes")) {
                for (int i = 0; i < BC.buildings.Count; i++) {
                    BC.buildings[i].showBuildingMesh = true;
                    BC.buildings[i].showWindowMesh = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        // Selected building info foldout
        EditorGUILayout.Space(15f);
        int doorDeleteIndex = -1;
        BCMenu.showBuildingInfo = EditorGUILayout.Foldout(BCMenu.showBuildingInfo, "Selected Building Info");
        if (BCMenu.showBuildingInfo && SelectionInfo.buildingIndex != -1) {
            // General Settings
            EditorGUILayout.Space();
            SelectedBuilding.inverted         = EditorGUILayout.ToggleLeft( "Inverted",           SelectedBuilding.inverted         );
            SelectedBuilding.showBuildingMesh = EditorGUILayout.ToggleLeft( "Show Building Mesh", SelectedBuilding.showBuildingMesh );
            SelectedBuilding.showWindowMesh   = EditorGUILayout.ToggleLeft( "Show Window Mesh",   SelectedBuilding.showWindowMesh   );
            SelectedBuilding.showDoorMesh     = EditorGUILayout.ToggleLeft( "Show Door Mesh",     SelectedBuilding.showDoorMesh     );
            EditorGUILayout.Space();
            SelectedBuilding.height           = EditorGUILayout.FloatField( "Building Height",    SelectedBuilding.height );
            EditorGUILayout.Space();
            // Material
            SelectedBuilding.buildingMaterial = (Material)EditorGUILayout.ObjectField( "Building Material", SelectedBuilding.buildingMaterial, typeof(Material), true );
            SelectedBuilding.windowMaterial   = (Material)EditorGUILayout.ObjectField( "Window Material",   SelectedBuilding.windowMaterial,   typeof(Material), true );
            SelectedBuilding.doorMaterial     = (Material)EditorGUILayout.ObjectField( "Door Material",     SelectedBuilding.doorMaterial,     typeof(Material), true );

            // Window Settings Foldout
            EditorGUILayout.Space(15f);
            BCMenu.showWindowSettings = EditorGUILayout.Foldout(BCMenu.showWindowSettings, "Window Settings");
            if (BCMenu.showWindowSettings) {
                SelectedBuilding.topOffset     = EditorGUILayout.Slider( "Top Offset",    SelectedBuilding.topOffset,    0f, 1f - SelectedBuilding.bottomOffset );
                SelectedBuilding.bottomOffset  = EditorGUILayout.Slider( "Bottom Offset", SelectedBuilding.bottomOffset, 0f, 1f - SelectedBuilding.topOffset    );
                SelectedBuilding.edgeOffset    = EditorGUILayout.Slider( "Edge Offset",   SelectedBuilding.edgeOffset,   0f, 1f                                 );
                EditorGUILayout.Space();
                SelectedBuilding.windowHeight  = EditorGUILayout.FloatField( "Window Height", SelectedBuilding.windowHeight );
                SelectedBuilding.windowWidth   = EditorGUILayout.FloatField( "Window Width",  SelectedBuilding.windowWidth  );
                SelectedBuilding.windowDepth   = EditorGUILayout.FloatField( "Window Depth",  SelectedBuilding.windowDepth  );
                EditorGUILayout.Space();
                SelectedBuilding.verticalGap   = EditorGUILayout.FloatField( "Vertical Gap",   SelectedBuilding.verticalGap   );
                SelectedBuilding.horizontalGap = EditorGUILayout.FloatField( "Horizontal Gap", SelectedBuilding.horizontalGap );
            }

            // Door Settings Foldout
            EditorGUILayout.Space(15f);
            BCMenu.showDoorSettings = EditorGUILayout.Foldout(BCMenu.showDoorSettings, "Door Settings");
            if (BCMenu.showDoorSettings) {
                // Scroll view
                if (SelectedBuilding.doors?.Count > 0) {
                    scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUILayout.Height(65));  
                    for (int i = 0; i < SelectedBuilding.doors.Count; i++) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Door " + i, GUILayout.Width(100));
                        // Disable the next button if it represents the selected door
                        GUI.enabled = !(i == SelectionInfo.doorIndex);
                        if (GUILayout.Button("Select")) {
                            Undo.RecordObject(BC, "Select Door");
                            SelectionInfo.doorIndex = i;
                        }
                        // Re-enaable the GUI
                        GUI.enabled = true;
                        if (GUILayout.Button("Delete")) {
                            doorDeleteIndex = i;
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                }
                else {
                    EditorGUILayout.HelpBox("No Doors", MessageType.Warning);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Door")) {
                    SelectedBuilding.doors.Add(new Door());
                }
                if (GUILayout.Button("Deselect")) {
                    SelectionInfo.doorIndex = -1;
                }
                GUILayout.EndHorizontal();
                
                // Selected door settings
                if (SelectionInfo.doorIndex > -1) {
                    EditorGUILayout.Space();
                    Door selectedDoor      = SelectedBuilding.doors[SelectionInfo.doorIndex];
                    selectedDoor.wallIndex = EditorGUILayout.IntSlider(  "Wall Index",    selectedDoor.wallIndex, 0, SelectedBuilding.points.Count - 1 );
                    selectedDoor.height    = EditorGUILayout.FloatField( "Door Height",   selectedDoor.height );
                    selectedDoor.width     = EditorGUILayout.FloatField( "Door Width",    selectedDoor.width  );
                    selectedDoor.depth     = EditorGUILayout.FloatField( "Door Depth",    selectedDoor.depth  );
                    selectedDoor.position  = EditorGUILayout.Slider(     "Door Position", selectedDoor.position, 0f, 1f);
                }
            }

        } 
        else if (BCMenu.showBuildingInfo && SelectionInfo.buildingIndex == -1) {
            EditorGUILayout.HelpBox("No Building Selected", MessageType.Warning);
        }
        EditorGUILayout.Space(15f);

        // Delete buildings / doors if needed (While maintaining proper selection)
        if (buildingDeleteIndex > -1) {
            Undo.RecordObject(BC, "Delete Building");
            BC.buildings.RemoveAt(buildingDeleteIndex);
            if (SelectionInfo.buildingIndex == buildingDeleteIndex) SelectionInfo.buildingIndex = -1;
            else if (SelectionInfo.buildingIndex > buildingDeleteIndex) SelectionInfo.buildingIndex--;
        } 
        else if (doorDeleteIndex > -1) {
            Undo.RecordObject(BC, "Delete Door");
            SelectedBuilding.doors.RemoveAt(doorDeleteIndex);
            if (SelectionInfo.doorIndex == doorDeleteIndex) SelectionInfo.doorIndex = -1;
            else if (SelectionInfo.doorIndex > doorDeleteIndex) SelectionInfo.doorIndex--;
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
            Draw(guiEvent);
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
        Vector3 mousePosition = mouseRay.GetPoint(-mouseRay.origin.y / mouseRay.direction.y);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift) 
            HandleShiftLeftMouseDown();
        else if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None) 
            HandleLeftMouseDown();
        else if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0) 
            HandleLeftMouseUp();
        else if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None) 
            HandleLeftMouseDrag();
        else if (!SelectionInfo.mouseIsBeingDragged) 
            UpdateSelectionInfo();

        // Handles LMBD input
        void HandleLeftMouseDown() {
            if (BCMenu.mode == 0) {
                if (BC.buildings.Count == 0) {
                    CreateNewBuilding();
                    CreateNewPoint();
                } else if (SelectionInfo.mouseIsOverPoint) {
                    SelectBuildingUnderMouse();
                    SelectPointUnderMouse();
                    StartDrag(); 
                } else if (SelectionInfo.buildingIndex == -1 && !SelectionInfo.mouseIsOverLine) {
                    CreateNewBuilding();
                    CreateNewPoint();
                } else {
                    SelectBuildingUnderMouse();
                    CreateNewPoint();
                }
            } else if (BCMenu.mode == 1) {
                SelectBuildingUnderMouse();
                StartDrag();
            } else if (BCMenu.mode == 2) {
                SelectBuildingUnderMouse();
                StartDrag();
            }
        }

        // Handles LMBU input
        void HandleLeftMouseUp() {
            if (BCMenu.mode == 0) {
                if (SelectionInfo.mouseIsBeingDragged) {
                    SelectedBuilding.points[SelectionInfo.mouseOverPointIndex] = SelectionInfo.positionAtDragStart;
                    Undo.RecordObject(BC, "Move point");
                    SelectedBuilding.points[SelectionInfo.mouseOverPointIndex] = mousePosition;
                    SelectionInfo.mouseIsBeingDragged = false;
                    buildingHasChanged = true;
                    meshHasChanged = true;
                }
            } else if (BCMenu.mode == 1) {
                if (SelectionInfo.mouseIsBeingDragged) {
                    List<Vector3> pointsAtDragEnd = new List<Vector3>(SelectedBuilding.points);
                    SelectedBuilding.points = SelectionInfo.pointsAtDragStart;
                    Undo.RecordObject(BC, "Move building");
                    SelectedBuilding.points = pointsAtDragEnd;
                    SelectionInfo.mouseIsBeingDragged = false;
                    buildingHasChanged = true;
                    meshHasChanged = true;
                }
            } else if (BCMenu.mode == 2) {
                List<Vector3> pointsAtDragEnd = new List<Vector3>(SelectedBuilding.points);
                SelectedBuilding.points = SelectionInfo.pointsAtDragStart;
                Undo.RecordObject(BC, "Rotate building");
                SelectedBuilding.points = pointsAtDragEnd;
                SelectionInfo.mouseIsBeingDragged = false;
                buildingHasChanged = true;
                meshHasChanged = true;
            }
        }

        // Handles Shift + LMBD input
        void HandleShiftLeftMouseDown() {
            if (BCMenu.mode == 0) {
                if (SelectionInfo.mouseIsOverPoint) {
                    SelectBuildingUnderMouse();
                    DeletePointUnderMouse();
                } else {
                    CreateNewBuilding();
                    CreateNewPoint();
                }
            }
            // else if (BCMenu.mode == 1) {

            // }
            // else if (BCMenu.mode == 2) {

            // }
        }

        // Handles LMB drag input
        void HandleLeftMouseDrag() { 
            if (BCMenu.mode == 0) {
                if (SelectionInfo.mouseOverPointIndex != -1) {
                    SelectedBuilding.points[SelectionInfo.mouseOverPointIndex] = mousePosition;
                }
            } else if (BCMenu.mode == 1) {
                Vector3 displacement = mousePosition - SelectionInfo.positionAtDragStart;
                SelectedBuilding.points = SelectionInfo.pointsAtDragStart.Map(a => a + displacement);
            } else if (BCMenu.mode == 2) {
                Vector3 initialDirection = (SelectionInfo.positionAtDragStart - SelectionInfo.centerAtDragStart).normalized;
                Vector3 currentDirection = (mousePosition - SelectionInfo.centerAtDragStart).normalized;
                float angle = Vector3.SignedAngle(initialDirection, currentDirection, Vector3.up);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                List<Vector3> relativePositions = SelectionInfo.pointsAtDragStart.Map(p => p - SelectionInfo.centerAtDragStart);
                SelectedBuilding.points = relativePositions.Map(p => (rotation * p) + SelectionInfo.centerAtDragStart);
            }
            buildingHasChanged = true;
            meshHasChanged = true;
 
        }

        // Updates the Selection Info class
        void UpdateSelectionInfo() {
            int mouseOverPointIndex = -1;
            int mouseOverBuildingIndex = -1;
            int mouseOverLineIndex = -1;

            for (int buildingIndex = 0; buildingIndex < BC.buildings.Count; buildingIndex++) {
                Building currentBuilding = BC.buildings[buildingIndex];
                for (int i = 0; i < currentBuilding.points.Count; i++) {
                    if (Vector3.Distance(mousePosition, currentBuilding.points[i]) < BCMenu.handleRadius) {
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
                float closestLineDistance = BCMenu.handleRadius;
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
            BC.buildings.Add(new Building("Building " + BC.buildings.Count));
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
                SelectionInfo.doorIndex = -1;
                buildingHasChanged = true;
            }
        }

        // Select the point under the mouse
        void SelectPointUnderMouse() {
            SelectionInfo.mouseIsOverPoint = true;
            SelectionInfo.mouseIsOverLine = false;
            SelectionInfo.mouseOverLineIndex = -1;
            buildingHasChanged = true;
        }

        // Start dragging the point/building under the mouse
        void StartDrag() {
            SelectionInfo.mouseIsBeingDragged = true;
            SelectionInfo.positionAtDragStart = mousePosition;
            SelectionInfo.pointsAtDragStart = new List<Vector3>(SelectedBuilding.points);
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
            SelectionInfo.mouseIsBeingDragged = false;
            SelectionInfo.mouseIsOverPoint = false;
            buildingHasChanged = true;
            meshHasChanged = true;
        }

    }

    // Draws handles and meshes
    void Draw(Event guiEvent) {
        // Mouse Position 
        Ray     mouseRay      = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        Vector3 mousePosition = mouseRay.GetPoint(-mouseRay.origin.y / mouseRay.direction.y);
        // For each building
        for (int buildingIndex = 0; buildingIndex < BC.buildings.Count; buildingIndex++) {    
            Building currentBuilding = BC.buildings[buildingIndex];
            bool currentBuildingIsSelected  = (buildingIndex == SelectionInfo.buildingIndex);
            bool mouseIsOverCurrentBuilding = (buildingIndex == SelectionInfo.mouseOverBuildingIndex);
            float h = currentBuilding.height;
            
            // For each point in this building
            for (int i = 0; i < currentBuilding.points.Count; i++) {
                Color outline    = new Color(0f, 0.8f, 0f), 
                      deselected = Color.gray,  activeLine = Color.red, 
                      hover      = Color.blue,  dragged    = Color.black, 
                      idle       = Color.white, move       = Color.green;
                      
                Vector3 thisPoint      = currentBuilding.points.GetItem(i);
                Vector3 nextPoint      = currentBuilding.points.GetItem(i + 1);
                Vector3 aboveThisPoint = thisPoint + Vector3.up * h;
                Vector3 aboveNextPoint = nextPoint + Vector3.up * h;
                bool mouseIsOverThisPoint = (i == SelectionInfo.mouseOverPointIndex && mouseIsOverCurrentBuilding);
                bool mouseIsOverThisLine  = (i == SelectionInfo.mouseOverLineIndex && mouseIsOverCurrentBuilding);
                bool thisPointIsDragged = (mouseIsOverThisPoint && SelectionInfo.mouseIsBeingDragged);
                
                // 2D Outline
                if (BCMenu.showOutline2D || BCMenu.showOutline3D) {
                    // On hover
                    if (mouseIsOverThisLine)
                        Handles.color = hover;
                    // Active / Deselected
                    else
                        Handles.color = (currentBuildingIsSelected) ? activeLine : deselected;
                    
                    // Draw
                    // On hover
                    if (mouseIsOverThisLine)
                        Handles.DrawLine(thisPoint, nextPoint, 4);
                    // Active / Deselected
                    else
                        Handles.DrawDottedLine(thisPoint, nextPoint, 4);
                }

                // Discs
                if (BCMenu.showHandles) {
                    // Color
                    Handles.color = thisPointIsDragged ? dragged    :
                                  mouseIsOverThisPoint ? 
                                   Event.current.shift ? activeLine : hover :
                             currentBuildingIsSelected ? 
                                      BCMenu.mode == 1 ? move       : idle  :
                                                         deselected ;
                    // Draw
                    Handles.DrawSolidDisc(thisPoint, Vector3.up, BCMenu.handleRadius);
                }
                
                // 3D Outline
                if (BCMenu.showOutline3D){
                    // Color
                    Handles.color = thisPointIsDragged ? dragged    :
                                  mouseIsOverThisPoint ? hover      :
                             currentBuildingIsSelected ? activeLine :
                                                         deselected ;
                    // Draw
                    Handles.DrawDottedLine(thisPoint, aboveThisPoint, 4f);
                    
                    // Horizontal
                    Handles.color = currentBuildingIsSelected ? activeLine : deselected;
                    Handles.DrawDottedLine(aboveThisPoint, aboveNextPoint, 4f);
                }

                if (BCMenu.showWindowOutlines) {
                    // Show window outlines
                }

                if (BCMenu.showDoorOutlines) {
                    List<Door> doors = currentBuilding.doors;
                    
                    for (int doorIndex = 0; doorIndex < doors.Count; doorIndex++) {
                        Door currentDoor = doors[doorIndex]; 
                        
                        if (currentDoor.wallIndex == i) {
                            bool currentDoorIsSelected = (doorIndex == SelectionInfo.doorIndex);
                            Vector3 thisToNext = nextPoint - thisPoint;
                            Vector3 direction = thisToNext.normalized;
                            Vector3 position = (thisToNext.magnitude - currentDoor.width) * currentDoor.position * direction;
                            Vector3 doorPoint1 = thisPoint + position;
                            Vector3 doorPoint2 = doorPoint1 + currentDoor.height * Vector3.up;
                            Vector3 doorPoint3 = doorPoint2 + currentDoor.width * direction;
                            Vector3 doorPoint4 = doorPoint3 + currentDoor.height * Vector3.down;
                            /*  2 -- 3
                                |    |  1, 0, 1, 1
                                1    4  */
                            Handles.color = currentDoorIsSelected ? new Color(1, 0, 1, 1) : new Color(1, 0, 1, 0.5f);
                            Handles.DrawDottedLine(doorPoint1, doorPoint2, 4);
                            Handles.DrawDottedLine(doorPoint2, doorPoint3, 4);
                            Handles.DrawDottedLine(doorPoint3, doorPoint4, 4);
                        }

                    }
                }

                // Offset guides
                if (BCMenu.showGuides) {
                    Handles.color = outline;
                    Vector3 direction = (nextPoint - thisPoint).normalized;
                    float wallLength = (nextPoint - thisPoint).magnitude;
                    Vector3 p1 = thisPoint + direction * currentBuilding.edgeOffset * wallLength/2;
                    Vector3 p2 = thisPoint + direction * (1 - currentBuilding.edgeOffset/2) * wallLength;
                    Handles.DrawLine(p1, p1 + Vector3.up * h, 2);
                    Handles.DrawLine(p2, p2 + Vector3.up * h, 2);
                    Handles.DrawLine(thisPoint + Vector3.up * currentBuilding.bottomOffset * h, 
                                    nextPoint + Vector3.up * currentBuilding.bottomOffset * h);
                    Handles.DrawLine(thisPoint + Vector3.up * (1 - currentBuilding.topOffset) * h, 
                                    nextPoint + Vector3.up * (1 - currentBuilding.topOffset) * h);
                }
            }
        
            // Center point
            if (BCMenu.mode == 2) {
                Handles.color = (currentBuildingIsSelected) ? Color.magenta : Color.gray;
                Vector3 center = currentBuilding.CenterPoint;
                Handles.DrawSolidDisc(center, Vector3.up, 3f);
                if (SelectionInfo.mouseIsBeingDragged) {
                    Handles.DrawDottedLine(SelectedBuilding.CenterPoint, mousePosition, 4f);
                }
            }

        }

        if (meshHasChanged) {
            ClearMesh();
            SetBuildingMeshes();
            UpdateComponents();
        }
        meshHasChanged = false;
        buildingHasChanged = false;
    }

    // Create/Set mesh of all buildings
    void SetBuildingMeshes() {
        for (int currentBuildingIndex = 0; currentBuildingIndex < BC.buildings.Count; currentBuildingIndex++) {
            // Current building in this context
            Building currentBuilding = BC.buildings[currentBuildingIndex];
            bool currentBuildingIsSelected = (currentBuildingIndex == SelectionInfo.buildingIndex);
            // Boolean to signify the winding order of the buildings points
            bool isClockWise = currentBuilding.points.ToXZ().FindArea2D() > 0;
            // List of triangles and vertices of the building and windows
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            // Height of the building
            Vector3 h = currentBuilding.height * Vector3.up;
            
            // Clear mesh
            currentBuilding.buildingMesh = new Mesh();
            currentBuilding.windowMesh = new Mesh();
            // currentBuilding.buildingMesh.Clear();
            // currentBuilding.windowMesh.Clear();
            
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
            currentBuilding.buildingMesh.vertices = vertices.ToArray();
            currentBuilding.buildingMesh.triangles = triangles.ToArray();

            // Windows
            if (currentBuilding.showWindowMesh && currentBuilding.windowHeight >= 5 && currentBuilding.windowWidth >= 5) {
                List<Vector3> verticesW = new List<Vector3>();
                List<int> trianglesW = new List<int>();
                for (int currentPointIndex = 0; currentPointIndex < currentBuilding.points.Count; currentPointIndex++) {
                    Vector3 thisPoint = currentBuilding.points.GetItem(currentPointIndex);
                    Vector3 nextPoint = currentBuilding.points.GetItem(currentPointIndex + 1);
                    Vector3 thisToNext = nextPoint - thisPoint;
                    Vector3 direction = thisToNext.normalized;
                    Vector3 perpendicular = isClockWise == currentBuilding.inverted ? 
                                            new Vector3(direction.z, 0, -direction.x):
                                            new Vector3(-direction.z, 0, direction.x);
                    float wallLength = thisToNext.magnitude;
                    // Available space
                    float Ax = wallLength * (1 - currentBuilding.edgeOffset);
                    float Ay = h.magnitude * (1 - (currentBuilding.topOffset + currentBuilding.bottomOffset));
                    // Window dimensions
                    float Wx = currentBuilding.windowWidth, Wy = currentBuilding.windowHeight;
                    // Window Gaps
                    float Gx = currentBuilding.horizontalGap, Gy = currentBuilding.verticalGap;
                    // # of windows
                    int Nx = Mathf.FloorToInt((Ax + Gx) / (Wx + Gx)), Ny = Mathf.FloorToInt((Ay + Gy) / (Wy + Gy));
                    // Actual used space
                    float Ux = Nx * (Wx + Gx) - Gx, Uy = Ny * (Wy + Gy) - Gy;
                    
                    // Starting point
                    Vector3 topLeft = thisPoint 
                                        + direction * ((wallLength - Ux)/2)
                                        + Vector3.up * (h.magnitude * (1 - currentBuilding.topOffset) - (Ay - Uy)/2);

                    for (int x = 0; x < Nx; x++) {
                        for (int y = 0; y < Ny; y++) {
                            Vector3 d = perpendicular * currentBuilding.windowDepth;
                            Vector3 p0 = topLeft + direction * (x * (Wx + Gx)) + Vector3.down * (y * (Wy + Gy));
                            Vector3 p1 = p0 + direction * Wx;
                            Vector3 p2 = p0 + Vector3.down * Wy;
                            Vector3 p3 = p1 + Vector3.down * Wy;
                            Vector3 p4 = p0 + d, p5 = p1 + d, p6 = p2 + d, p7 = p3 + d; 

                            List<Vector3> verts = new List<Vector3>{
                                p0, p4, p2, p6,
                                p1, p5, p0, p4,
                                p3, p7, p1, p5,
                                p2, p6, p3, p7,
                                p4, p5, p6, p7
                            };
                            List<int> tris = new List<int>{
                                0,   1,  2,  2,  1,  3,
                                4,   5,  6,  6,  5,  7,
                                8,   9, 10, 10,  9, 11,
                                12, 13, 14, 14, 13, 15,
                                16, 17, 18, 18, 17, 19
                            };
                            if (isClockWise != currentBuilding.inverted) {
                                tris.Reverse();
                            }
                            trianglesW.AddRange(tris.Map(a => a + verticesW.Count));
                            verticesW.AddRange(verts);
                        }
                    }

                }
                currentBuilding.windowMesh.vertices = verticesW.ToArray();
                currentBuilding.windowMesh.triangles = trianglesW.ToArray();
            }       
        }
    }

    // Update shared mesh
    void UpdateComponents() {
        BC.meshFilter.sharedMesh.Clear();
        List<CombineInstance> meshes = new List<CombineInstance>();

        for (int i = 0; i < BC.buildings.Count; i++) {
            CombineInstance buildingMesh = new CombineInstance();
            if (BC.buildings[i].buildingMesh && BC.buildings[i].showBuildingMesh) {
                buildingMesh.mesh = BC.buildings[i].buildingMesh;
                meshes.Add(buildingMesh);
            }
        }
        for (int i = 0; i < BC.buildings.Count; i++) {
            CombineInstance windowMesh = new CombineInstance();
            if (BC.buildings[i].windowMesh && BC.buildings[i].showWindowMesh) {
                windowMesh.mesh = BC.buildings[i].windowMesh;
                meshes.Add(windowMesh);
            }
        }

        BC.meshFilter.sharedMesh.CombineMeshes(meshes.ToArray(), false, false, false);
        BC.meshFilter.sharedMesh.RecalculateNormals();

        // Materials
        List<Material> materials = new List<Material>();
        materials.Clear();
        for (int i = 0; i < BC.buildings.Count; i++) {
            if (BC.buildings[i].buildingMaterial && BC.buildings[i].showBuildingMesh) {
                materials.Add(BC.buildings[i].buildingMaterial);
            }
        }
        for (int i = 0; i < BC.buildings.Count; i++) {
            if (BC.buildings[i].windowMaterial && BC.buildings[i].showWindowMesh) {
                materials.Add(BC.buildings[i].windowMaterial);
            }
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

}

