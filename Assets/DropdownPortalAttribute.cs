
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public delegate string GetItemNameCallback(object baseMaster, object master);
/// <summary>
/// [Dropdown(path, displayProperty)]
/// 
/// - path:            the path of the List
/// - displayProperty: the property you want to display in the dropdown selection
/// 
/// </summary>
public class DropdownPortalAttribute : PropertyAttribute
{
    public DropdownPortalAttribute()
    {
        Debug.Log("Inside Attribute");
    }
}
