using UnityEngine;

public class NamePowerBase : MonoBehaviour
{
    protected Player player;
    protected virtual void Awake()
    {
        player = GetComponentInParent<Player>();
    }
}
