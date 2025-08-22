using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class CutSceneHandler : MonoBehaviour
{
    [SerializeField] private VideoClip cutScene;
    [SerializeField] private VideoPlayer videoPlayer;

    [SerializeField] private string cutSceneUrl;

    private void Start()
    {
        StartCoroutine(PlayCutScene());
    }

    private IEnumerator PlayCutScene()
    {
//        Debug.Log("Starting Cutscene!");
//        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
//        videoPlayer.targetCamera = Camera.main;
//        videoPlayer.isLooping = false;
//        Time.timeScale = 0f;

//#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL path: use URL source
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = cutSceneUrl;
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) yield return null;
        videoPlayer.Play();

        // Wait until playback finishes
        yield return new WaitUntil(() => !videoPlayer.isPlaying);
//#else
//        // Non-WebGL path: use VideoClip as before
//        videoPlayer.source = VideoSource.VideoClip;
//        videoPlayer.clip = cutScene;
//        videoPlayer.Play();
//        yield return new WaitForSecondsRealtime((float)cutScene.length);
//#endif

        videoPlayer.Stop();
        AudioManager.Instance.PlayGameplayMusic();
        Time.timeScale = 1f;
    }
}
