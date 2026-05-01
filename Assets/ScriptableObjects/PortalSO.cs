using UnityEngine;

[CreateAssetMenu(fileName = "PortalSO", menuName = "Scriptable Objects/PortalSO")]
public class PortalSO : ScriptableObject
{
    [Header("Portal Config")]
    public float spawnCoolDown = 120f;
}
