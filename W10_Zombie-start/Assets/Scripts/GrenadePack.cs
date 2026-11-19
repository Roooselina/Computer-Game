using UnityEngine;

public class GrenadePack : MonoBehaviour, IItem {
    public int amount = 2;
    public void Use(GameObject target) {
        var grenades = target.GetComponent<PlayerGrenades>();
        if (!grenades || grenades.count >= grenades.maxCount) return;
        grenades.AddGrenades(amount);
        Destroy(gameObject);
    }
}
