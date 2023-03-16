using UnityEngine;

public class DropdownCrossSceneObjectAttribute : PropertyAttribute
{
    public string Tag { get; set; }
    public DropdownCrossSceneObjectAttribute(string tag = "Portal")
    {
        Tag = tag;
        Debug.Log("Inside Attribute");
    }
}
