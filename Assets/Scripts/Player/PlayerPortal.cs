using System.Collections;
using UnityEngine;

public class PlayerPortal : MonoBehaviour
{
    [SerializeField] private string poolKey = "Portal";

    private Animator animator;
    private bool isActive;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Activate(Vector3 position, bool facingRight)
    {
        transform.SetParent(null);
        Debug.Log("I am lord yaya");
        transform.position = position;

        transform.rotation = facingRight
            ? Quaternion.identity
            : Quaternion.Euler(0, 180, 0);

        isActive = true;

        // Reset scale (important for pooling)
        transform.localScale = Vector3.one;

        animator.Play("Portal_Open", 0, 0f);

        StartCoroutine(ReturnAfterDelay(0.5f));
    }

    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ReturnToPool();
    }

    // Animation Event at end
    public void ReturnToPool()
    {
        Debug.Log("RETURNING PORTAL");
        if (!isActive) return;

        isActive = false;

        PoolManager.Instance.Return(poolKey, gameObject);
    }
}
