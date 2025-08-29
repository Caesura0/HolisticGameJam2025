using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class EndGameScreenHandler : MonoBehaviour
{
    // These may be EITHER full URLs (https://...) OR just filenames that exist under Assets/StreamingAssets/
    [Header("WebGL URLs or StreamingAssets filenames")]
    [SerializeField] private string diedUrl;
    [SerializeField] private string capturedUrl;
    [SerializeField] private string survivedUrl;

    [Header("Non-WebGL Clips")]
    [SerializeField] private VideoClip diedClip;
    [SerializeField] private VideoClip capturedClip;
    [SerializeField] private VideoClip survivedClip;

    [SerializeField] private float startDelay = 1f;

    private VideoPlayer videoPlayer;

    private void Start()
    {
        if (!TryGetComponent(out videoPlayer)) return;

        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.isLooping = true;

        GameManager.Instance.OnStarvedToDeath += HandleStarvedEnding;
        GameManager.Instance.OnGotCaptured += HandleCapturedEnding;
        GameManager.Instance.OnSurvivedTimer += HandleSurvivedEnding;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnStarvedToDeath -= HandleStarvedEnding;
        GameManager.Instance.OnGotCaptured -= HandleCapturedEnding;
        GameManager.Instance.OnSurvivedTimer -= HandleSurvivedEnding;
    }

    private void HandleStarvedEnding()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(StartPlayingUrl(diedUrl));
#else
        StartCoroutine(StartPlayingClip(diedClip));
#endif
    }

    private void HandleCapturedEnding()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(StartPlayingUrl(capturedUrl));
#else
        StartCoroutine(StartPlayingClip(capturedClip));
#endif
    }

    private void HandleSurvivedEnding()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(StartPlayingUrl(survivedUrl));
#else
        StartCoroutine(StartPlayingClip(survivedClip));
#endif
    }

    // --- Shared fade-in routine ---
    private IEnumerator FadeInAlpha()
    {
        videoPlayer.targetCameraAlpha = 0f;
        float a = 0f;
        while (a < 1f)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            a += 0.1f;
            videoPlayer.targetCameraAlpha = Mathf.Min(1f, a);
        }
    }

    private IEnumerator PrepareThenPlay()
    {
        yield return new WaitForSecondsRealtime(startDelay);
        Time.timeScale = 0f;

        // Keep video running while game is paused & avoid autoplay AudioContext issues
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) yield return null;

        videoPlayer.Play();
        AudioManager.Instance?.PlayGameEndMusic();

        yield return FadeInAlpha();
    }

    // --- Clip path (Editor/Standalone) ---
    private IEnumerator StartPlayingClip(VideoClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("EndGameScreenHandler: VideoClip is not assigned.");
            yield break;
        }

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        yield return PrepareThenPlay();
    }

    // --- URL path (WebGL). Accepts full URLs or StreamingAssets filenames ---
    private IEnumerator StartPlayingUrl(string urlOrFile)
    {
        if (string.IsNullOrWhiteSpace(urlOrFile))
        {
            Debug.LogWarning("EndGameScreenHandler: URL/filename is empty. Assign a direct mp4 URL or a StreamingAssets filename.");
            yield break;
        }

        string u = urlOrFile.Trim();

        // If not an absolute URL, treat it as a file inside StreamingAssets
        if (!(u.StartsWith("http://") || u.StartsWith("https://")))
            u = Application.streamingAssetsPath + "/" + u;

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = u;

        yield return PrepareThenPlay();
    }
}
