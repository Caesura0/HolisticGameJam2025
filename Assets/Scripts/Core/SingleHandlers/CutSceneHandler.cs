using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class CutSceneHandler : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string fileName = "cutscene.mp4"; // put this in Assets/StreamingAssets/

    private void Start()
    {
        StartCoroutine(PlayCutScene());
        Time.timeScale = 0f; // pause the game while the cutscene plays
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnCutsceneEnded;
    }

    private string BuildStreamingAssetsUrl(string file)
    {
        var path = Path.Combine(Application.streamingAssetsPath, file);
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL already needs a regular HTTP(S) URL; Unity provides it here.
        return path; // e.g., https://<host>/Build/StreamingAssets/cutscene.mp4
#else
        // Editor/Windows: convert local path to file:// URL
        return new Uri(path).AbsoluteUri; 
#endif
    }

    private IEnumerator PlayCutScene()
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = BuildStreamingAssetsUrl(fileName);
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // optional: keep muted

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) yield return null;

        videoPlayer.Play();
        yield return new WaitUntil(() => !videoPlayer.isPlaying);

        videoPlayer.Stop();
        AudioManager.Instance.PlayGameplayMusic();
        Time.timeScale = 1f;
    }

    private void OnCutsceneEnded(VideoPlayer vp)
    {
        vp.loopPointReached -= OnCutsceneEnded; // clean up
        vp.Stop();
        AudioManager.Instance.PlayGameplayMusic();
        Time.timeScale = 1f;
        // optionally hide the video overlay:
        // vp.targetCameraAlpha = 0f;
    }
}
