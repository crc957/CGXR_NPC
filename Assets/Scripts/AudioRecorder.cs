using Samples.Whisper;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class AudioRecorder : MonoBehaviour
{
    public Button recordButton;
    public Button nextButton;
    public string flaskServerUrl = "https://cfd9-163-239-126-56.ngrok-free.app/upload";
    public AudioSource audioSource;
    private bool isRecording = false;
    private AudioClip recordedClip;
    private Image buttonImage; // 버튼의 이미지 컴포넌트를 사용하여 색상을 변경

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
        StartCoroutine(UploadWav(filePath));
    }

    // Flask 서버로 오디오 파일 업로드 함수
    IEnumerator UploadWav(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);

        // List<IMultipartFormSection>을 사용하여 파일을 포함한 폼 데이터를 생성
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();
        form.Add(new MultipartFormFileSection("file", fileData, "recordedAudio.wav", "audio/wav"));

        // UnityWebRequest.Post를 사용하여 파일을 폼 데이터로 전송
        UnityWebRequest request = UnityWebRequest.Post(flaskServerUrl, form);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 응답에서 워터마크된 파일의 데이터를 받아서 저장
            string watermarkedFilePath = Path.Combine(Application.persistentDataPath, "watermarkedAudio.wav");
            File.WriteAllBytes(watermarkedFilePath, request.downloadHandler.data);
            Debug.Log("File received successfully and saved to: " + watermarkedFilePath);
            GoToListenScene();  // 파일 업로드가 성공적으로 완료된 후 씬 전환
        }
        else
        {
            Debug.LogError("Error uploading audio: " + request.error);
        }
    }


    // 씬을 ListenScene으로 전환
    void GoToListenScene()
    {
        SceneManager.LoadScene("ListenScene"); // ListenScene으로 전환
    }

}
