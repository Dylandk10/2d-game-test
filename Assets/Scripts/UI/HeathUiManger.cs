using UnityEngine;
using UnityEngine.UI;

public class HeathUiManger : MonoBehaviour
{
    [Header("Health Images")]
    [SerializeField] public Image[] healthImages;
    [SerializeField] public Sprite Heart;
    [SerializeField] public Sprite BlackHeart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckHealth();
    }

    private void CheckHealth()
    {
        int lives = Player.Instance.GetLives();
        int maxLives = Player.Instance.GetMaxLives();

        for (int i = 0; i < healthImages.Length; i++)
        {
            if (i < lives)
            {
                // Player still has this life → red heart
                healthImages[i].sprite = Heart;
            }
            else
            {
                // Life lost → black heart
                healthImages[i].sprite = BlackHeart;
            }

            // Optional: disable extra hearts if array is bigger than maxLives
            if (i >= maxLives)
            {
                healthImages[i].enabled = false;
            }
            else
            {
                healthImages[i].enabled = true;
            }
        }
    }
}
