using UnityEngine;

public class UIGameManager : MonoBehaviour
{
    public static UIGameManager Instance { get; private set; }
    [SerializeField] public Canvas GameOverCanvas;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optional: persist between scenes
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        GameOverCanvas.gameObject.SetActive(false);
    }

    public void ShowDeathMenu()
    {
        GameOverCanvas.gameObject.SetActive(true);
    }
}
