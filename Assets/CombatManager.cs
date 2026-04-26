using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private bool hitRegisteredThisSwing;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Called when enemy starts attack
    public void BeginAttackWindow()
    {
        hitRegisteredThisSwing = false;
    }

    // Called by hitbox
    public void TryHitPlayer(int damage)
    {
        if (hitRegisteredThisSwing)
            return; // prevents multi-hit per swing

        hitRegisteredThisSwing = true;

        Player.Instance.TakeDamage(damage);
        hitRegisteredThisSwing = false;
    }

}
