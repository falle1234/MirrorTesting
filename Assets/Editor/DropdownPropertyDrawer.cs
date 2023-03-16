using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
    
[CustomPropertyDrawer(typeof(DropdownCrossSceneObjectAttribute))]
public class DropdownCrossSceneObjectDrawer : PropertyDrawer
{
    private static readonly Dictionary<string, List<PortalsInScene>> sceneLists = new();
    public DropdownCrossSceneObjectDrawer()
    {
        Debug.Log("Loaded DropDownDrawer");
    }

    private GUIStyle GetGUIColor(Color color)
    {
        GUIStyle style = new(EditorStyles.label);
        style.normal.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.hover.textColor = color;
        return style;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 3 + EditorGUIUtility.standardVerticalSpacing*2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var tag = (attribute as DropdownCrossSceneObjectAttribute).Tag;
        if (!sceneLists.ContainsKey(tag))
        {
            LoadObjectsByTag(tag);
        }

        Debug.Log("Inside ONGUI");
        try
        { 
            EditorGUI.BeginProperty(position,label , property);
            
            if (sceneLists.ContainsKey(tag))
            {
                
                EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                var targetScene = property.FindPropertyRelative("targetScene").stringValue;
                var targetPortal = property.FindPropertyRelative("gameObject").stringValue;
                #region Draw the list dropdown
                EditorGUI.indentLevel = 1;
                int SelectedSceneID = -1;
                int SelectedPortalID = -1;
                String[] portalArray = new string[0];
                if (!String.IsNullOrWhiteSpace(targetScene))
                {
                    SelectedSceneID = sceneLists[tag].FindIndex(t => t.scenePath == targetScene);//Update the selectedID
                    portalArray = sceneLists[tag].FirstOrDefault(t => t.scenePath == targetScene).Portals.ToArray();
                    if (!String.IsNullOrWhiteSpace(targetPortal))
                    {
                        SelectedPortalID = sceneLists[tag].FirstOrDefault(t => t.scenePath == targetScene).Portals.FindIndex(p => p == targetPortal);
                    }
                }

                string[] arr = sceneLists[tag].Select(t => t.sceneName).ToArray();//convert the objects into name list using GetItemName delegate method
                if (arr == null)
                {
                    EditorGUI.LabelField(GetPosition(position,1), "Scene name", $"Cannot find property: ", GetGUIColor(Color.red));
                    return;
                }
                int newSelectedSceneID = EditorGUI.Popup(GetPosition(position, 1), "Scene name", SelectedSceneID, arr); 
                if (portalArray == null)
                {
                    EditorGUI.LabelField(GetPosition(position, 2), "Object name", $"Cannot find property: ", GetGUIColor(Color.red));
                    return;
                }
                int newSelectedPortalID = EditorGUI.Popup(GetPosition(position, 2), "Object name", SelectedPortalID > 0 ? SelectedPortalID : 0 , portalArray) ;
                if (newSelectedSceneID != SelectedSceneID)
                {
                    SelectedSceneID = newSelectedSceneID;
                    PortalsInScene selectedObject = sceneLists[tag][SelectedSceneID];
                    property.FindPropertyRelative("targetScene").stringValue = selectedObject.scenePath;
                }
                if (newSelectedPortalID != SelectedPortalID)
                {
                    SelectedPortalID = newSelectedPortalID;
                    PortalsInScene selectedObject = sceneLists[tag][SelectedSceneID];
                    property.FindPropertyRelative("gameObject").stringValue = selectedObject.Portals[SelectedPortalID];
                }
                #endregion
            }
            else
            {
                EditorGUI.LabelField(position, property.name, $"The list size is 0", GetGUIColor(Color.red));
            }
            EditorGUI.EndProperty();
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

    private static void LoadObjectsByTag(string tag)
    {
        List<PortalsInScene> sceneList = new();
        List<Scene> scenesToUnload = new();
        var currentScenePath = SceneManager.GetActiveScene().path;
        var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (var guid in guids)
        {

            var scenePath = AssetDatabase.GUIDToAssetPath(guid);
            var sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
            var sceneName = $"{sceneAsset.name}";
            var portalsInScene = new PortalsInScene { sceneName = sceneName, scenePath = scenePath };

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(scenePath);
            foreach (var obj in scene.GetRootGameObjects())
            {
                if (obj.CompareTag(tag))
                {
                    portalsInScene.Portals.Add(obj.name);
                }
            }
            if (currentScenePath != scenePath)
            {
                scenesToUnload.Add(scene);
            }
            sceneList.Add(portalsInScene);
        }
        foreach (var scene in scenesToUnload)
        {
            EditorSceneManager.CloseScene(scene, true);
        }
        sceneLists.Add(tag, sceneList);
    }

    private Rect GetPosition(Rect position, int number)
    {
        var pos = EditorGUI.IndentedRect(position);
        pos.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * number;
        return pos;
    }

    private class PortalsInScene
    {
        public String sceneName;
        public String scenePath;
        public List<string> Portals = new();
    }
}