using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class CutSceneHandler : MonoBehaviour
{
    [SerializeField] private VideoClip cutScene;
    [SerializeField] private VideoPlayer videoPlayer;

    private void Start()
    {
        StartCoroutine(PlayCutScene());
    }

    private IEnumerator PlayCutScene()
    {
        Debug.Log("Starting Cutscene!");
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.clip = cutScene;
        videoPlayer.isLooping = false;
        videoPlayer.Play();
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime((float)cutScene.length);
        videoPlayer.Stop();
        Time.timeScale = 1f;
    }
}
