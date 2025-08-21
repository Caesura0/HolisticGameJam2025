using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class EndGameScreenHandler : MonoBehaviour
{
    [SerializeField] private VideoClip died;
    [SerializeField] private VideoClip captured;
    [SerializeField] private VideoClip survived;
    [SerializeField] private float startDelay = 1f;

    private VideoPlayer videoPlayer;

    private void Start()
    {
        if (!TryGetComponent<VideoPlayer>(out videoPlayer))
            return;

        //Debug.Log(Camera.main);

        videoPlayer.targetCamera = Camera.main;

        GameplayManager.Instance.OnStarvedToDeath += HandleStarvedEnding;
        GameplayManager.Instance.OnGotCaptured += HandleCapturedEnding;
        GameplayManager.Instance.OnSurvivedTimer += HandleSurvivedEnding;
    }

    private void HandleStarvedEnding() => StartCoroutine(StartPlayingVideo(died));
    private void HandleCapturedEnding() => StartCoroutine(StartPlayingVideo(captured));
    private void HandleSurvivedEnding() => StartCoroutine(StartPlayingVideo(survived));

    private void PlayEndScreen(VideoClip chosenClip)
    {
        videoPlayer.clip = chosenClip;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    private IEnumerator StartPlayingVideo(VideoClip chosenClip, float startTime = 0f)
    {
        if (startTime == 0f)
        {
            yield return new WaitForSecondsRealtime(startDelay);
            Time.timeScale = 0f;
            PlayEndScreen(chosenClip);
        }

        yield return new WaitForSecondsRealtime(.1f);

        videoPlayer.targetCameraAlpha = startTime;
        startTime += .1f;

        Debug.Log("StartTime: " + startTime);

        if (startTime < 1f)
            StartCoroutine(StartPlayingVideo(chosenClip, startTime));
        else
            videoPlayer.targetCameraAlpha = 1f;
    }
}