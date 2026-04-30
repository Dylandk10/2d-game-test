using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Sight Range")]
    public float sightRangeX = 7f;
    public float sightRangeY = 2f;
    public float attackRangeX = 1.4f;
    public float attackRangeY = 1.3f;

    [Header("Attack")]
    public float attackDelay = 0.4f;
    public float attackCooldown = 1.5f;

    [Header("Death")]
    public float deathDestroyDelay = 1.2f;

    [Header("Patrol")]
    public float patrolRange = 4f;
    public float patrolSpeed = 1.5f;
    public float idleTime = 2f;
    public float patrolTime = 3f;
    [Range(0f, 1f)] public float patrolChance = 0.6f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.2f;

    [Header("Base Stats")]
    public int maxHealth = 100;
}