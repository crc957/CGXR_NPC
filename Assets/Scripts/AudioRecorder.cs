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
    private Image buttonImage; // ��ư�� �̹��� ������Ʈ�� ����Ͽ� ������ ����

    public ContractManager contractManager; // ContractManager ��ũ��Ʈ ����

    void Start()
    {
        recordButton.onClick.AddListener(ToggleRecording);
        nextButton.onClick.AddListener(GoToListenScene);
        buttonImage = recordButton.GetComponent<Image>(); // ��ư�� �̹��� ������Ʈ�� ������
        buttonImage.color = Color.white; // �ʱ� ��ư ������ ������� ����
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
            recordedClip = Microphone.Start(null, false, 60, 44100); // 60�� ���� �ִ� ����
            Debug.Log("Recording started...");
            buttonImage.color = Color.red; // ���� ���� �� ��ư ������ ���������� ����
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
            buttonImage.color = Color.white; // ������ ������ �� ��ư ������ �ٽ� ������� ����
        }
    }

    void SaveRecording()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "recordedAudio.wav");
        SavWav.Save(filePath, recordedClip);
        PlayerPrefs.SetString("SavedAudioPath", filePath);
        PlayerPrefs.Save();
    }


    // ���� ListenScene���� ��ȯ
    void GoToListenScene()
    {
        SceneManager.LoadScene("ListenScene"); // ListenScene���� ��ȯ
    }

}
