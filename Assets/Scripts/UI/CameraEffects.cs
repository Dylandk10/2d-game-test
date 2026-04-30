using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    public static CameraEffects Instance;

    [SerializeField] private CinemachineCamera vcam;

    private float originalSize;
    private Coroutine zoomRoutine;

    void Awake()
    {
        Instance = this;
        originalSize = vcam.Lens.OrthographicSize;
    }

    public void HitZoom()
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(ZoomRoutine());
    }

    IEnumerator ZoomRoutine()
    {
        float zoomInTime = 0.3f;
        float zoomOutTime = 0.2f;

        float targetSize = originalSize * 0.85f;

        float t = 0f;

        // ZOOM IN
        while (t < zoomInTime)
        {
            t += Time.unscaledDeltaTime;

            var lens = vcam.Lens;
            lens.OrthographicSize = Mathf.Lerp(originalSize, targetSize, t / zoomInTime);
            vcam.Lens = lens;

            yield return null;
        }

        t = 0f;

        // ZOOM OUT
        while (t < zoomOutTime)
        {
            t += Time.unscaledDeltaTime;

            var lens = vcam.Lens;
            lens.OrthographicSize = Mathf.Lerp(targetSize, originalSize, t / zoomOutTime);
            vcam.Lens = lens;

            yield return null;
        }

        var finalLens = vcam.Lens;
        finalLens.OrthographicSize = originalSize;
        vcam.Lens = finalLens;
    }
}
