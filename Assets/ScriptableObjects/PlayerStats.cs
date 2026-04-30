using UnityEngine;

[CreateAssetMenu(menuName = "Stats/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Health")]
    public int maxLives = 6;

    [Header("Combat")]
    public int baseDamage = 50;
    public float invincibleDuration = 1f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 2;

    [Header("Dash")]
    public float dashDistance = 4f;
    public float dashSpeed = 17f;
    public float dashCooldown = 1.5f;
}