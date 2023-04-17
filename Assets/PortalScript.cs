using Mirror;
using UnityEngine;

public class PortalScript: MonoBehaviour
{
    [DropdownCrossSceneObject("SpawnPoint")]
    public CrossSceneObject PortalTarget;

    [ServerCallback]
    public void OnTriggerStay(Collider other)
    {
        if (other.transform.GetComponent<Player>())
        {
            Debug.Log("Collision");
        }
    }
}
