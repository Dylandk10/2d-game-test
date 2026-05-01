using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private PortalSO portalData;
    [SerializeField] private EnemySpawner spawner;

    private BoxCollider2D colliderTrigger;
    private bool canSpawn = true;

    private void Awake()
    {
        colliderTrigger = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canSpawn) return;

        if (other.CompareTag("Player"))
        {
            Activate();
            StartCoroutine(SpawnCooldown());
        }
    }

    private void Activate()
    {
        EnemySpawner.Instance.SpawnWaveMinorsOnly(transform.position, 5);
    }

    private IEnumerator SpawnCooldown()
    {
        canSpawn = false;
        colliderTrigger.enabled = false;

        yield return new WaitForSeconds(portalData.spawnCoolDown);

        colliderTrigger.enabled = true;
        canSpawn = true;
    }
}
