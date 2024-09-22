using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class ListenManager : MonoBehaviour
{
    public Button playButton;
    public Button yesButton;
    public Button noButton;
    public AudioSource audioSource;
    private string filePath;

    void Start()
    {
        // PlayerPrefs에서 저장된 오디오 파일 경로 불러오기
        filePath = PlayerPrefs.GetString("SavedAudioPath", "");

        if (File.Exists(filePath))
        {
            // 오디오 파일을 로드하고 재생할 준비
            StartCoroutine(LoadAudio(filePath));
        }
        else
        {
            Debug.LogWarning("No audio file found at: " + filePath);
        }

        playButton.onClick.AddListener(PlayAudio);

        yesButton.onClick.AddListener(GoYesScene);

        noButton.onClick.AddListener(GoNoScene);
    }

    IEnumerator LoadAudio(string path)
    {
        using (WWW www = new WWW("file://" + path))
        {
            yield return www;

            audioSource.clip = www.GetAudioClip(false, false);
            Debug.Log("Audio loaded from: " + path);
        }
    }

    void PlayAudio()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log("Playing audio...");
        }
        else
        {
            Debug.LogWarning("No audio clip loaded to play.");
        }
    }

    void GoYesScene()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();  // 오디오 재생을 멈추고 씬 전환
        }
        SceneManager.LoadScene("YesScene");
    }

    void GoNoScene()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();  // 오디오 재생을 멈추고 씬 전환
        }
        SceneManager.LoadScene("NoScene");
    }
}
