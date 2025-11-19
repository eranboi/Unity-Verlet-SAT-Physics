using Environment.Rope;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class RopeToolWindow : EditorWindow
{
    private Rope _targetRope;
    private bool _isEditing;
    
    // --- YENİ EKLENEN AYARLAR ---
    private static GameObject _ropeBasePrefab; // Temel Rope objesi için kullanılacak prefab
    private static GameObject _pointPrefab;    // Noktalar için varsayılan prefab
    private static bool _showSettings = true;  // Ayarlar menüsünü açıp kapatmak için

    // Ayarları kaydetmek için kullanılacak anahtarlar
    private const string RopePrefabPathKey = "RopeTool_RopeBasePrefabPath";
    private const string PointPrefabPathKey = "RopeTool_DefaultPointPrefabPath";
    // --- BİTİŞ ---
    
    // --- DRAG CREATION VARIABLES ---
    private bool _isDragging;
    private Point _lastCreatedPoint;
    private Vector2 _lastMousePosition;
    private float _minDistance = 0.3f;
    // --- END ---

    [MenuItem("Tools/Rope Editor")]
    public static void ShowWindow()
    {
        GetWindow<RopeToolWindow>("Rope Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        
        // --- YENİ EKLENEN AYAR YÜKLEME ---
        // Editör her açıldığında kaydedilmiş prefab yollarını yükle
        string ropePath = EditorPrefs.GetString(RopePrefabPathKey, null);
        if (!string.IsNullOrEmpty(ropePath))
            _ropeBasePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ropePath);

        string pointPath = EditorPrefs.GetString(PointPrefabPathKey, null);
        if (!string.IsNullOrEmpty(pointPath))
            _pointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pointPath);
        // --- BİTİŞ ---

        OnSelectionChange();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSelectionChange()
    {
        var activeObject = Selection.activeGameObject;
        if (activeObject != null && activeObject.TryGetComponent<Rope>(out Rope rope))
        {
            _targetRope = rope;
        }
        else
        {
            _targetRope = null;
        }
        Repaint();
    }
    
    // --- BU METOT GÜNCELLENDİ ---
    private void CreateNewRope()
    {
        GameObject ropeObject;

        // Eğer kullanıcı bir "Rope Base Prefab" atamışsa, onu kullan.
        if (_ropeBasePrefab != null)
        {
            ropeObject = (GameObject)PrefabUtility.InstantiatePrefab(_ropeBasePrefab);
            ropeObject.name = _ropeBasePrefab.name;
        }
        else
        {
            // Atamamışsa, eskisi gibi boş bir GameObject oluştur.
            ropeObject = new GameObject("New Rope");
        }

        Undo.RegisterCreatedObjectUndo(ropeObject, "Create New Rope");
        
        // Rope component'inin var olduğundan emin ol. Prefab'da yoksa, ekle.
        _targetRope = ropeObject.GetComponent<Rope>() ?? ropeObject.AddComponent<Rope>();

        Selection.activeGameObject = ropeObject;
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Rope Editor", EditorStyles.boldLabel);
        
        // --- YENİ EKLENEN AYARLAR BÖLÜMÜ ---
        EditorGUILayout.Space(5);
        _showSettings = EditorGUILayout.Foldout(_showSettings, "Editor Settings", true);
        if (_showSettings)
        {
            EditorGUI.BeginChangeCheck();
            _ropeBasePrefab = (GameObject)EditorGUILayout.ObjectField("Rope Base Prefab", _ropeBasePrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                string path = _ropeBasePrefab != null ? AssetDatabase.GetAssetPath(_ropeBasePrefab) : null;
                EditorPrefs.SetString(RopePrefabPathKey, path);
            }

            EditorGUI.BeginChangeCheck();
            _pointPrefab = (GameObject)EditorGUILayout.ObjectField("Default Point Prefab", _pointPrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                string path = _pointPrefab != null ? AssetDatabase.GetAssetPath(_pointPrefab) : null;
                EditorPrefs.SetString(PointPrefabPathKey, path);
            }
        }
        // --- BİTİŞ ---

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Editing", EditorStyles.boldLabel);
        
        if (_targetRope == null)
        {
            EditorGUILayout.LabelField("No Rope Selected", EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button("Create New Rope Object"))
            {
                CreateNewRope();
            }
        }
        else
        {
            EditorGUILayout.LabelField("Editing Rope:", _targetRope.gameObject.name, EditorStyles.boldLabel);
        }
        
        EditorGUILayout.Space(5);
        
        GUI.backgroundColor = _isEditing ? Color.yellow : Color.white;
        if (GUILayout.Button(_isEditing ? "Stop Editing" : "Start Editing"))
        {
            _isEditing = !_isEditing;
            Debug.Log($"Editing mode changed to: {_isEditing}");
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;

        if (!_isEditing || _targetRope == null) return;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
        
        // Min distance setting for drag creation
        _minDistance = EditorGUILayout.Slider("Min Point Distance", _minDistance, 0.1f, 2f);

        if (GUILayout.Button("Connect Points Sequentially"))
        {
            ConnectPointsSequentially();
        }

        if (GUILayout.Button("Generate Cloth"))
        {
            Undo.RecordObject(_targetRope, "Generate Cloth");
            _targetRope.GenerateCloth();
            EditorUtility.SetDirty(_targetRope);
        }
        
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
        if (GUILayout.Button("Clear Entire Rope"))
        {
            if (EditorUtility.DisplayDialog("Clear Rope?",
                "Are you sure you want to delete all points and sticks from '" + _targetRope.name + "'?", "Yes", "No"))
            {
                _targetRope.ClearRope();
                EditorUtility.SetDirty(_targetRope);
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_isEditing) return;

        // Store the control ID once at the beginning
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlId);
        Event e = Event.current;

        if (_targetRope != null)
        {
            DrawPointHandles();
        }

        // Mouse Down - Start dragging
        if (e.type == EventType.MouseDown && e.button == 0 && !e.shift)
        {
            if (HandleUtility.nearestControl == controlId)
            {
                if (_targetRope == null)
                {
                    CreateNewRope();
                }

                // Get mouse position
                Vector2 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
                
                // Create first point
                Undo.RecordObject(_targetRope, "Add Rope Point");
                _lastCreatedPoint = _targetRope.Editor_AddPoint(mousePos);
                _lastMousePosition = mousePos;
                _isDragging = true;
                
                EditorUtility.SetDirty(_targetRope);
                e.Use();
            }
        }
        
        // Mouse Drag - Continue creating points
        if (e.type == EventType.MouseDrag && e.button == 0 && _isDragging && !e.shift)
        {
            Vector2 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            
            // Check if we've moved enough distance
            if (Vector2.Distance(mousePos, _lastMousePosition) > _minDistance)
            {
                Undo.RecordObject(_targetRope, "Add Rope Point");
                
                // Create new point
                var newPoint = _targetRope.Editor_AddPoint(mousePos);
                
                // Connect to last point if exists
                if (_lastCreatedPoint != null)
                {
                    _targetRope.Editor_AddStick(_lastCreatedPoint, newPoint);
                }
                
                _lastCreatedPoint = newPoint;
                _lastMousePosition = mousePos;
                
                EditorUtility.SetDirty(_targetRope);
                SceneView.RepaintAll();
            }
            e.Use();
        }
        
        // Mouse Up - Stop dragging
        if (e.type == EventType.MouseUp && e.button == 0 && _isDragging)
        {
            _isDragging = false;
            _lastCreatedPoint = null;
            e.Use();
        }
    }

    private void DrawPointHandles()
    {
        var points = _targetRope.Points.ToList();
        foreach (var point in points)
        {
            if (point.gameObject == null) continue;
            
            Vector2 currentPos = point.currentPos;
            
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift)
            {
                if (HandleUtility.DistanceToCircle(currentPos, 0.1f) < 10)
                {
                    Undo.RecordObject(_targetRope, "Remove Rope Point");
                    _targetRope.Points.Remove(point);
                    point.Destroy();
                    CleanupSticks();
                    EditorUtility.SetDirty(_targetRope);
                    Event.current.Use();
                    continue;
                }
            }
            
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(currentPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(point.gameObject.transform, "Move Rope Point");
                point.currentPos = new Vector2(newPos.x, newPos.y);
                point.prevPos = point.currentPos;
                point.UpdateRenderer();
                foreach (var stick in _targetRope.Sticks.Where(s => s.pointA == point || s.pointB == point))
                {
                    stick.UpdatePositions();
                }
                EditorUtility.SetDirty(_targetRope);
            }
            
            Handles.color = point.isFixed ? Color.red : Color.green;
            if (Handles.Button(currentPos, Quaternion.identity, _targetRope.PointSize * 0.5f, _targetRope.PointSize * 0.5f, Handles.SphereHandleCap))
            {
                Undo.RecordObject(point.gameObject, "Toggle Point Fixed");
                point.SetFixed(!point.isFixed);
                EditorUtility.SetDirty(_targetRope);
            }
            Handles.color = Color.white;
        }
    }

    private void ConnectPointsSequentially()
    {
        Undo.RecordObject(_targetRope, "Connect Points");
        var points = _targetRope.Points.ToList();
        if (points.Count < 2) return;
        for (int i = 0; i < points.Count - 1; i++)
        {
            _targetRope.Editor_AddStick(points[i], points[i + 1]);
        }
        EditorUtility.SetDirty(_targetRope);
    }
    
    private void CleanupSticks()
    {
        _targetRope.Sticks.RemoveWhere(stick => stick == null || stick.pointA == null || stick.pointA.gameObject == null || stick.pointB == null || stick.pointB.gameObject == null);
        EditorUtility.SetDirty(_targetRope);
    }
}