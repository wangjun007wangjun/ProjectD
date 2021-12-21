using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideo : MonoBehaviour
{
    VideoPlayer videoPlayer;
    AudioSource auidoSource;

    float mLifeTime = 0f;
    float mElapseTime = 0f;

    Action mCallBack = null;
    Action mTrackCallBack = null;

    float mTrackClip = 0;//打点间隔
    float mTrackNextTime = 0;//打点时间

    bool mStart = false;

    public void VideoPlay(string p_video, float p_time, Action callback, Action trackCallback, float track_clip)
    {
        mLifeTime = p_time;
        mCallBack = callback;

        mStart = false;
        mElapseTime = 0;

        mTrackCallBack = trackCallback;
        mTrackClip = track_clip;
        mTrackNextTime = track_clip;

        gameObject.SetActive(true);

        PlayVideoPlayer(p_video, p_time);
    }

    public void PauseVideo()
    {
        if (videoPlayer != null)
            videoPlayer.Pause();
    }
    public void ContinueVideo()
    {
        if (videoPlayer != null)
            videoPlayer.Play();
    }
    public void StopVideo()
    {
        Debug.Log("############ StopVideo!");

        if (videoPlayer != null)
            videoPlayer.Stop();

        gameObject.SetActive(false);

        if(mCallBack != null)
            mCallBack.Invoke();

        mTrackCallBack = null;
    }

    void Update()
    {
        if (videoPlayer == null)
            return;
        if (mStart == false)
            return;
        if (videoPlayer.isPlaying)
        {
            mElapseTime += Time.deltaTime;
            if (mTrackCallBack != null)
            {
                if (mElapseTime > mTrackNextTime)
                {
                    mTrackNextTime = mElapseTime + mTrackClip;
                    // mTrackCallBack.Invoke(mElapseTime);
                }
            }
            if (mElapseTime >= mLifeTime)
            {
                mStart = false;
                StopVideo();
            }
        }
        else
        {
            // mStart = false;
            // StopVideo();
        }
    }

    void PlayVideoPlayer(string p_video, float p_time)
    {
        RawImage rawImg = transform.Find("RawImage").gameObject.GetComponent<RawImage>();
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 5);
        rawImg.texture = videoPlayer.targetTexture;
        auidoSource = transform.GetChild(0).GetComponent<AudioSource>();

        Canvas canvas = transform.parent.GetComponent<Canvas>();

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        //videoPlayer.targetCamera = canvas.worldCamera;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, "Video/" + p_video + ".mp4");
        videoPlayer.aspectRatio = GetVideoAspectRatio();
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.SetTargetAudioSource(0, auidoSource);
        videoPlayer.prepareCompleted += Prepared;
        videoPlayer.Prepare();
        videoPlayer.errorReceived += ErrorReceived;
        videoPlayer.frameDropped += FrameDropped;
    }

    VideoAspectRatio GetVideoAspectRatio()
    {
        if (Screen.width / Screen.height >= 1920f / 1080)
            return VideoAspectRatio.FitHorizontally;
        else
            return VideoAspectRatio.FitVertically;
    }

    void Prepared(VideoPlayer vPlayer)
    {
        Debug.Log("############ Prepared!");
        vPlayer.Play();
        mStart = true;
    }
    void ErrorReceived(VideoPlayer vPlayer, string msg)
    {
        Debug.Log("############ Error Received! " + msg);
    }
    void FrameDropped(VideoPlayer vPlayer)
    {
        Debug.Log("############ Frame Dropped! ");
    }
}
