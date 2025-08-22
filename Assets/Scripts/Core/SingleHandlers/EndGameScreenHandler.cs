using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class EndGameScreenHandler : MonoBehaviour
{
    // --- WebGL sources (direct .mp4 URLs; H.264 + AAC recommended) ---
    [Header("WebGL URLs (use direct mp4 links)")]
    [SerializeField] private string diedUrl;
    [SerializeField] private string capturedUrl;
    [SerializeField] private string survivedUrl;

    // --- Editor/Standalone sources ---
    [Header("Non-WebGL Clips")]
    [SerializeField] private VideoClip diedClip;
    [SerializeField] private VideoClip capturedClip;
    [SerializeField] private VideoClip survivedClip;

    [SerializeField] private float startDelay = 1f;

    private VideoPlayer videoPlayer;

    private void Start()
    {
        if (!TryGetComponent(out videoPlayer)) return;

        // Render on camera so targetCameraAlpha can fade in
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.isLooping = true;

        // Optional: route audio via AudioSource if you want mixer control
        // (Create/assign an AudioSource on this GameObject)
        // videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        // videoPlayer.EnableAudioTrack(0, true);
        // videoPlayer.SetTargetAudioSource(0, GetComponent<AudioSource>());

        GameplayManager.Instance.OnStarvedToDeath += HandleStarvedEnding;
        GameplayManager.Instance.OnGotCaptured += HandleCapturedEnding;
        GameplayManager.Instance.OnSurvivedTimer += HandleSurvivedEnding;
    }

    private void OnDestroy()
    {
        if (GameplayManager.Instance == null) return;
        GameplayManager.Instance.OnStarvedToDeath -= HandleStarvedEnding;
        GameplayManager.Instance.OnGotCaptured -= HandleCapturedEnding;
        GameplayManager.Instance.OnSurvivedTimer -= HandleSurvivedEnding;
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
        // Optional pause of gameplay and music cue
        yield return new WaitForSecondsRealtime(startDelay);
        Time.timeScale = 0f;

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) yield return null;

        videoPlayer.Play();
        AudioManager.Instance?.PlayGameEndMusic();

        // Smooth fade-in
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

    // --- URL path (WebGL) ---
    private IEnumerator StartPlayingUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning("EndGameScreenHandler: URL is empty. Assign a direct mp4 URL in the inspector.");
            yield break;
        }

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url.Trim();
        yield return PrepareThenPlay();
    }
}
