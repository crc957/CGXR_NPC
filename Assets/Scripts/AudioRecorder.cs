using Samples.Whisper;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioRecorder : MonoBehaviour
{
    public Button recordButton;
    public Button nextButton;
    public AudioSource audioSource;
    private bool isRecording = false;
    private AudioClip recordedClip;
    private Image buttonImage; // 버튼의 이미지 컴포넌트를 사용하여 색상을 변경

    public ContractManager contractManager; // ContractManager 스크립트 참조

    void Start()
    {
        recordButton.onClick.AddListener(ToggleRecording);
        nextButton.onClick.AddListener(GoToListenScene);
        buttonImage = recordButton.GetComponent<Image>(); // 버튼의 이미지 컴포넌트를 가져옴
        buttonImage.color = Color.white; // 초기 버튼 색상을 흰색으로 설정
    }

    void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    void StartRecording()
    {
        if (Microphone.devices.Length > 0)
        {
            isRecording = true;
            recordedClip = Microphone.Start(null, false, 60, 44100); // 60초 동안 최대 녹음
            Debug.Log("Recording started...");
            buttonImage.color = Color.red; // 녹음 중일 때 버튼 색상을 빨간색으로 변경
        }
        else
        {
            Debug.LogWarning("No microphone detected!");
        }
    }

    void StopRecording()
    {
        if (isRecording)
        {
            Microphone.End(null);
            isRecording = false;
            Debug.Log("Recording stopped.");
            SaveRecording();
            buttonImage.color = Color.white; // 녹음이 끝났을 때 버튼 색상을 다시 흰색으로 변경
        }
    }

    void SaveRecording()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "recordedAudio.wav");
        SavWav.Save(filePath, recordedClip);
        PlayerPrefs.SetString("SavedAudioPath", filePath);
        PlayerPrefs.Save();
    }


    // 씬을 ListenScene으로 전환
    void GoToListenScene()
    {
        SceneManager.LoadScene("ListenScene"); // ListenScene으로 전환
    }

}
