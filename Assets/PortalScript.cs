using UnityEngine;

public class PortalScript : MonoBehaviour
{
    [DropdownCrossSceneObject("SpawnPoint")]
    public CrossSceneObject PortalTarget;

}
