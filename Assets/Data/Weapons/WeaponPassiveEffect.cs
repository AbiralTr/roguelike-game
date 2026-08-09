using UnityEngine;

public abstract class WeaponPassiveEffect : ScriptableObject
{
    public abstract void OnHit(GameObject wielder, GameObject target);
}
