using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace Extensions.CutsceneEngine.Editor
{
    /**
     * <summary>
     * High-level cutscene authoring window that provides:
     * - Scene actor browser
     * - Available actions per actor
     * - Quick Timeline clip creation
     * - Action library management
     * - Batch editing capabilities
     * </summary>
     */
    public class CutsceneEditorWindow : EditorWindow
    {
        [MenuItem("Window/Cutscene Engine/Cutscene Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<CutsceneEditorWindow>("Cutscene Editor");
            window.minSize = new Vector2(800, 600);
        }
        
        private PlayableDirector selectedDirector;
        private CutsceneDirector selectedCutsceneDirector;
        private TimelineAsset selectedTimeline;
        
        private Vector2 actorScrollPos;
        private Vector2 actionScrollPos;
        private Vector2 libraryScrollPos;
        
        private List<ICutsceneActor> sceneActors = new List<ICutsceneActor>();
        private ICutsceneActor selectedActor;
        private string[] availableActorMethods;
        
        private enum EditorTab
        {
            SceneActors,
            ActionLibrary,
            Settings
        }
        
        private EditorTab currentTab = EditorTab.SceneActors;
        
        private void OnEnable()
        {
            RefreshSceneActors();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            
            EditorGUILayout.Space(10);
            
            // Director Selection
            DrawDirectorSelection();
            
            EditorGUILayout.Space(10);
            
            // Tab Bar
            DrawTabBar();
            
            EditorGUILayout.Space(10);
            
            // Tab Content
            switch (currentTab)
            {
                case EditorTab.SceneActors:
                    DrawSceneActorsTab();
                    break;
                case EditorTab.ActionLibrary:
                    DrawActionLibraryTab();
                    break;
                case EditorTab.Settings:
                    DrawSettingsTab();
                    break;
            }
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Cutscene Editor", headerStyle, GUILayout.Height(30));
            EditorGUILayout.LabelField("Fast cutscene authoring for Timeline", EditorStyles.centeredGreyMiniLabel);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDirectorSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Cutscene Director", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            selectedDirector = (PlayableDirector)EditorGUILayout.ObjectField("Playable Director", selectedDirector, typeof(PlayableDirector), true);
            
            if (EditorGUI.EndChangeCheck() && selectedDirector != null)
            {
                selectedCutsceneDirector = selectedDirector.GetComponent<CutsceneDirector>();
                selectedTimeline = selectedDirector.playableAsset as TimelineAsset;
                
                if (selectedCutsceneDirector == null)
                {
                    EditorGUILayout.HelpBox("Selected director does not have a CutsceneDirector component. Add one to enable cutscene features.", MessageType.Warning);
                }
            }
            
            if (selectedDirector == null)
            {
                EditorGUILayout.HelpBox("Select a Playable Director to begin editing cutscenes.", MessageType.Info);
                
                if (GUILayout.Button("Find Director in Scene"))
                {
                    selectedDirector = FindObjectOfType<PlayableDirector>();
                    if (selectedDirector != null)
                    {
                        selectedCutsceneDirector = selectedDirector.GetComponent<CutsceneDirector>();
                        selectedTimeline = selectedDirector.playableAsset as TimelineAsset;
                    }
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Open Timeline"))
                {
                    if (selectedTimeline != null)
                    {
                        TimelineEditor.OpenEditor(selectedDirector);
                    }
                }
                
                if (GUILayout.Button("Play Cutscene"))
                {
                    if (EditorApplication.isPlaying && selectedCutsceneDirector != null)
                    {
                        selectedCutsceneDirector.Play();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Cannot Play", "Enter Play Mode to test cutscene playback.", "OK");
                    }
                }
                
                if (GUILayout.Button("Stop Cutscene"))
                {
                    if (EditorApplication.isPlaying && selectedCutsceneDirector != null)
                    {
                        selectedCutsceneDirector.Stop();
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTabBar()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Toggle(currentTab == EditorTab.SceneActors, "Scene Actors", EditorStyles.toolbarButton))
                currentTab = EditorTab.SceneActors;
            
            if (GUILayout.Toggle(currentTab == EditorTab.ActionLibrary, "Action Library", EditorStyles.toolbarButton))
                currentTab = EditorTab.ActionLibrary;
            
            if (GUILayout.Toggle(currentTab == EditorTab.Settings, "Settings", EditorStyles.toolbarButton))
                currentTab = EditorTab.Settings;
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSceneActorsTab()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Left: Actor List
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            DrawActorList();
            EditorGUILayout.EndVertical();
            
            // Right: Actions for Selected Actor
            EditorGUILayout.BeginVertical();
            DrawActorActions();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawActorList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene Actors", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Refresh", GUILayout.Width(70)))
            {
                RefreshSceneActors();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Found {sceneActors.Count} actors in scene", EditorStyles.miniLabel);
            
            EditorGUILayout.Space(5);
            
            actorScrollPos = EditorGUILayout.BeginScrollView(actorScrollPos);
            
            foreach (var actor in sceneActors)
            {
                var mono = actor as MonoBehaviour;
                if (mono == null) continue;
                
                bool isSelected = actor == selectedActor;
                
                EditorGUILayout.BeginHorizontal(isSelected ? EditorStyles.selectionRect : EditorStyles.helpBox);
                
                EditorGUILayout.ObjectField(mono, typeof(MonoBehaviour), true);
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    selectedActor = actor;
                    RefreshActorMethods();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActorActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (selectedActor == null)
            {
                EditorGUILayout.HelpBox("Select an actor from the list to view available actions.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            var mono = selectedActor as MonoBehaviour;
            EditorGUILayout.LabelField($"Actions for: {mono.gameObject.name}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {mono.GetType().Name}", EditorStyles.miniLabel);
            
            EditorGUILayout.Space(10);
            
            if (availableActorMethods == null || availableActorMethods.Length == 0)
            {
                EditorGUILayout.HelpBox($"No [CutsceneAction] methods found on {mono.GetType().Name}.\n\nAdd methods with [CutsceneAction(\"Display Name\")] attribute to make them available.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"{availableActorMethods.Length} Available Actions:", EditorStyles.boldLabel);
                
                actionScrollPos = EditorGUILayout.BeginScrollView(actionScrollPos);
                
                foreach (var methodName in availableActorMethods)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    
                    EditorGUILayout.LabelField(methodName, GUILayout.Width(200));
                    
                    if (GUILayout.Button("Add to Timeline", GUILayout.Width(120)))
                    {
                        AddActorMethodToTimeline(methodName);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActionLibraryTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Action Library", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Reusable action assets", EditorStyles.miniLabel);
            
            EditorGUILayout.Space(10);
            
            libraryScrollPos = EditorGUILayout.BeginScrollView(libraryScrollPos);
            
            // Find all CutsceneActionDefinition assets
            var guids = AssetDatabase.FindAssets("t:CutsceneActionDefinition");
            
            if (guids.Length == 0)
            {
                EditorGUILayout.HelpBox("No action definition assets found.\n\nCreate reusable actions via: Create > Cutscene > Action Definition", MessageType.Info);
            }
            else
            {
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<CutsceneActionDefinition>(path);
                    
                    if (asset != null)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        
                        EditorGUILayout.ObjectField(asset, typeof(CutsceneActionDefinition), false);
                        
                        if (GUILayout.Button("Add to Timeline", GUILayout.Width(120)))
                        {
                            // TODO: Implement adding definition to timeline
                            EditorUtility.DisplayDialog("Add to Timeline", "This feature will be implemented to drag actions into Timeline.", "OK");
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Create New Action Definition", GUILayout.Height(30)))
            {
                var asset = CreateInstance<CutsceneActionDefinition>();
                string path = EditorUtility.SaveFilePanelInProject("Create Action Definition", "NewAction", "asset", "Save action definition");
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    EditorGUIUtility.PingObject(asset);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSettingsTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Cutscene Engine Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox("Future settings:\n• Default action durations\n• Auto-sync clip lengths\n• Preview options\n• Validation rules", MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }
        
        private void RefreshSceneActors()
        {
            sceneActors.Clear();
            var actors = FindObjectsOfType<MonoBehaviour>().OfType<ICutsceneActor>();
            sceneActors.AddRange(actors);
        }
        
        private void RefreshActorMethods()
        {
            if (selectedActor == null)
            {
                availableActorMethods = new string[0];
                return;
            }
            
            var adapter = selectedActor.GetCutsceneAdapter();
            if (adapter == null)
            {
                Debug.LogWarning($"Actor {(selectedActor as MonoBehaviour)?.name} returned null adapter. Ensure GetCutsceneAdapter() initializes the adapter properly.");
                availableActorMethods = new string[0];
                return;
            }
            
            var actionNames = adapter.GetActionNames();
            if (actionNames == null)
            {
                Debug.LogWarning($"Actor {(selectedActor as MonoBehaviour)?.name} adapter returned null action names.");
                availableActorMethods = new string[0];
                return;
            }
            
            availableActorMethods = actionNames.ToArray();
        }
        
        private void AddActorMethodToTimeline(string methodName)
        {
            if (selectedTimeline == null || selectedDirector == null)
            {
                EditorUtility.DisplayDialog("Cannot Add Action", "No Timeline selected. Select a Playable Director first.", "OK");
                return;
            }
            
            if (selectedActor == null)
            {
                EditorUtility.DisplayDialog("Cannot Add Action", "No actor selected.", "OK");
                return;
            }
            
            // Find or create track for this actor
            var track = FindOrCreateTrackForActor(selectedActor);
            
            if (track == null)
            {
                EditorUtility.DisplayDialog("Cannot Add Action", "Failed to create or find track for actor.", "OK");
                return;
            }
            
            // Create clip - this is fast
            var timelineClip = track.CreateDefaultClip();
            timelineClip.displayName = methodName;
            
            // Get the CutsceneActionClip asset
            var cutsceneClip = timelineClip.asset as CutsceneActionClip;
            if (cutsceneClip != null)
            {
                // Initialize with ActorMethodAction
                cutsceneClip.action = new CutsceneActionReference
                {
                    Action = new ActorMethodAction
                    {
                        MethodName = methodName,
                        Parameters = new SerializedCutsceneParameter[0]
                    }
                };
                
                // Get the method to generate proper parameters
                var adapter = selectedActor.GetCutsceneAdapter();
                var method = adapter.GetMethod(methodName);
                
                if (method != null)
                {
                    var actorMethod = cutsceneClip.action.Action as ActorMethodAction;
                    var parameters = method.GetParameters();
                    actorMethod.Parameters = new SerializedCutsceneParameter[parameters.Length];
                    
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        actorMethod.Parameters[i] = new SerializedCutsceneParameter();
                        
                        // Set default type based on parameter type
                        System.Type paramType = parameters[i].ParameterType;
                        if (paramType == typeof(int)) actorMethod.Parameters[i].type = SerializedCutsceneParameter.ParamType.Int;
                        else if (paramType == typeof(float)) actorMethod.Parameters[i].type = SerializedCutsceneParameter.ParamType.Float;
                        else if (paramType == typeof(bool)) actorMethod.Parameters[i].type = SerializedCutsceneParameter.ParamType.Bool;
                        else if (paramType == typeof(string)) actorMethod.Parameters[i].type = SerializedCutsceneParameter.ParamType.String;
                        else if (paramType == typeof(Vector3)) actorMethod.Parameters[i].type = SerializedCutsceneParameter.ParamType.Vector3;
                        else if (paramType == typeof(GameObject)) actorMethod.Parameters[i].type = SerializedCutsceneParameter.ParamType.GameObject;
                    }
                }
                
                EditorUtility.SetDirty(cutsceneClip);
            }
            
            // Mark timeline dirty and save - do this once at the end
            EditorUtility.SetDirty(selectedTimeline);
            AssetDatabase.SaveAssets();
            
            // Refresh Timeline window on next frame to avoid lag
            EditorApplication.delayCall += () =>
            {
                TimelineEditor.RefreshTimeline();
            };
            
            Debug.Log($"Added action '{methodName}' to Timeline for {(selectedActor as MonoBehaviour).name}");
        }
        
        private TrackAsset FindOrCreateTrackForActor(ICutsceneActor actor)
        {
            if (selectedTimeline == null) return null;
            
            var mono = actor as MonoBehaviour;
            if (mono == null) return null;
            
            // Check if track already exists for this actor
            foreach (var output in selectedTimeline.GetOutputTracks())
            {
                if (output is CutsceneActionTrack)
                {
                    var bound = selectedDirector.GetGenericBinding(output);
                    if (bound == mono)
                    {
                        return output;
                    }
                }
            }
            
            // Create new track
            var newTrack = selectedTimeline.CreateTrack<CutsceneActionTrack>(null, mono.name);
            selectedDirector.SetGenericBinding(newTrack, mono);
            
            return newTrack;
        }
    }
    
    /**
     * <summary>
     * Helper class to open Timeline Editor programmatically.
     * Unity's TimelineEditor is not publicly exposed, so we use reflection.
     * </summary>
     */
    public static class TimelineEditor
    {
        public static void OpenEditor(PlayableDirector director)
        {
            if (director == null) return;
            
            Selection.activeGameObject = director.gameObject;
            
            // Try to open Timeline window
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
        }
        
        public static void RefreshTimeline()
        {
            // Force Timeline window to repaint
            var timelineWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in timelineWindows)
            {
                if (window.GetType().Name == "TimelineWindow")
                {
                    window.Repaint();
                }
            }
        }
    }
}

