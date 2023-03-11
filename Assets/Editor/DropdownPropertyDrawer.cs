using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(DropdownPortalAttribute))]
public class DropdownAttributeDrawer : PropertyDrawer
{

    List<PortalsInScene> sceneList = new List<PortalsInScene>();
    public DropdownAttributeDrawer()
    {
        List<Scene> scenesToUnload = new List<Scene>();
        var currentScenePath = EditorSceneManager.GetActiveScene().path;
        var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (var guid in guids)
        {
            
            var scenePath = AssetDatabase.GUIDToAssetPath(guid);
            var sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
            var label = $"{sceneAsset.name}";
            var portalsInScene = new PortalsInScene { sceneName = label, scenePath = scenePath };
            
            if (currentScenePath != scenePath)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                var scene = EditorSceneManager.GetSceneByPath(scenePath);
                foreach (var obj in scene.GetRootGameObjects())
                {
                    if(obj.CompareTag("SpawnPoint"))
                    {
                        portalsInScene.Portals.Add(obj.name);
                    }
                }
                
                scenesToUnload.Add(scene);
            }
            sceneList.Add(portalsInScene);
        }
        foreach(var scene in scenesToUnload)
        {
            EditorSceneManager.CloseScene(scene, true);
        }
        Debug.Log("Loaded DropDownDrawer");
    }
    private GUIStyle GetGUIColor(Color color)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label);
        style.normal.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.hover.textColor = color;
        return style;
    }
    private GUIStyle GetDropdownStyle(Color c)
    {
        GUIStyle style = EditorStyles.popup;
        return style;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Debug.Log("Inside ONGUI");
        try
        {
            if (sceneList.Count > 0)
            {
                var targetScene = property.FindPropertyRelative("targetScene").stringValue;
                var targetPortal = property.FindPropertyRelative("spawnPoint").stringValue;
                #region Draw the list dropdown

                int SelectedSceneID = -1;
                int SelectedPortalID = -1;
                String[] portalArray = new string[0];
                if (!String.IsNullOrWhiteSpace(targetScene))
                {
                    SelectedSceneID = sceneList.FindIndex(t => t.scenePath == targetScene);//Update the selectedID
                    if (!String.IsNullOrWhiteSpace(targetPortal))
                    {
                        SelectedPortalID = sceneList.FirstOrDefault(t => t.scenePath == targetScene).Portals.FindIndex(p => p == targetPortal);
                        portalArray = sceneList.FirstOrDefault(t => t.scenePath == targetScene).Portals.ToArray();
                    }
                }
                string[] arr = sceneList.Select(t => t.sceneName).ToArray();//convert the objects into name list using GetItemName delegate method
                if (arr == null)
                {
                    EditorGUI.LabelField(position, "Scene name", $"Cannot find property: ", GetGUIColor(Color.red));
                    return;
                }
                if (SelectedSceneID == -1 && arr.Length > 0) SelectedSceneID = 0;//Set it to 0 as default
                //int newSelectedID = EditorGUILayout.Popup(dropdown, SelectedID, arr);
                int newSelectedSceneID = EditorGUI.Popup(position, "Scene name", SelectedSceneID, arr);
                position.position = position.position + new Vector2(0, 25);
                if (portalArray == null)
                {
                    EditorGUI.LabelField(position, "Portal name", $"Cannot find property: ", GetGUIColor(Color.red));
                    return;
                }
                if (SelectedPortalID == -1 && portalArray.Length > 0) SelectedPortalID = 0;//Set it to 0 as default
                                                                                           //int newSelectedID = EditorGUILayout.Popup(dropdown, SelectedID, arr);
                int newSelectedPortalID = EditorGUI.Popup(position, "Portal name", SelectedPortalID, portalArray);
                if (newSelectedSceneID != SelectedSceneID)
                {
                    SelectedSceneID = newSelectedSceneID;
                    PortalsInScene selectedObject = sceneList[SelectedSceneID];
                    property.FindPropertyRelative("targetScene").stringValue = selectedObject.scenePath;
                }

                #endregion
            }
            else
            {
                EditorGUI.LabelField(position, property.name, $"The list size is 0", GetGUIColor(Color.red));
            }
        }
        catch (Exception e)
        {
            GUIStyle style = GetGUIColor(Color.red);
            style.wordWrap = true;//word wrap

            GUILayout.BeginHorizontal();

            GUILayout.Label(property.name + " [Dropdown ERROR] ");
            GUILayout.TextArea(e.ToString(), style, GUILayout.ExpandHeight(true));

            GUILayout.EndHorizontal();
            Debug.LogException(e);
        }
    }
    private class PortalsInScene
    {
        public String sceneName;
        public String scenePath;
        public List<string> Portals;
    }
}