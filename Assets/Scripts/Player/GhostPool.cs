using System.Collections.Generic;
using UnityEngine;

public class GhostPool : MonoBehaviour
{
    public GameObject ghostPrefab;
    public int initialSize = 20;

    Queue<Ghost> pool = new Queue<Ghost>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateGhost();
        }
    }

    Ghost CreateGhost()
    {
        GameObject obj = Instantiate(ghostPrefab, transform);
        obj.SetActive(false);

        Ghost ghost = obj.GetComponent<Ghost>();
        pool.Enqueue(ghost);

        return ghost;
    }

    public Ghost GetGhost()
    {
        if (pool.Count == 0)
        {
            CreateGhost();
        }

        return pool.Dequeue();
    }

    public void ReturnToPool(Ghost ghost)
    {
        ghost.gameObject.SetActive(false);
        pool.Enqueue(ghost);
    }
}
