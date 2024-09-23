using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.SceneManagement;  // 씬 전환을 위해 UnityEngine.SceneManagement 네임스페이스 추가

public class ListenManager : MonoBehaviour
{
    public Button uploadButton;
    public string flaskServerUrl = "https://d7ec-163-239-126-56.ngrok-free.app/upload"; // Flask 서버 로컬 주소 또는 ngrok 주소
    private string filePath;
    private bool isUploading = false; // 업로드 상태를 나타내는 플래그

    void Start()
    {
        filePath = PlayerPrefs.GetString("SavedAudioPath", "");

        if (File.Exists(filePath))
        {
            Debug.Log("Audio file found at: " + filePath);
        }
        else
        {
            Debug.LogWarning("Audio file not found at: " + filePath);
        }

        Debug.Log("Flask Server URL: " + flaskServerUrl); // Flask 서버 URL 출력
        uploadButton.onClick.AddListener(UploadToFlaskServer);
    }

    void UploadToFlaskServer()
    {
        if (File.Exists(filePath) && !isUploading)
        {
            isUploading = true; // 업로드 시작 시 플래그 설정
            StartCoroutine(UploadWav(filePath)); // 파일 업로드 요청
        }
        else
        {
            if (isUploading)
            {
                Debug.LogWarning("Upload is already in progress.");
            }
            else
            {
                Debug.LogError("File not found, cannot upload.");
            }
        }
    }

    IEnumerator UploadWav(string filePath)
    {
        Debug.Log("UploadWav coroutine started");

        byte[] fileData = File.ReadAllBytes(filePath);
        Debug.Log($"File data read. Size: {fileData.Length} bytes");

        // 워터마크 적용 여부 수집 (테스트로 true 설정)
        bool isWatermarkEnabled = true; // 필요에 따라 true 또는 false로 설정

        // List<IMultipartFormSection>을 사용하여 파일을 포함한 폼 데이터를 생성
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();
        form.Add(new MultipartFormFileSection("file", fileData, "recordedAudio.wav", "audio/wav"));
        Debug.Log("File section added to form");

        // 워터마크 적용 여부 추가 - Boolean 값을 문자열 "true" 또는 "false"로 변환하여 전송
        form.Add(new MultipartFormDataSection("watermarkCheck", isWatermarkEnabled.ToString().ToLower()));
        Debug.Log("Watermark check section added to form");

        // UnityWebRequest.Post를 사용하여 파일을 폼 데이터로 전송
        UnityWebRequest request = UnityWebRequest.Post(flaskServerUrl, form);
        Debug.Log("UnityWebRequest created");

        yield return request.SendWebRequest();
        Debug.Log($"Request sent. Status: {request.result}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 응답에서 워터마크된 파일의 데이터를 받아서 저장
            if (request.downloadHandler.data.Length > 0)
            {
                string watermarkedFilePath = Path.Combine(Application.persistentDataPath, "watermarkedAudio.wav");
                File.WriteAllBytes(watermarkedFilePath, request.downloadHandler.data);
                Debug.Log("File received successfully and saved to: " + watermarkedFilePath);

                // OKScene으로 전환
                GoOKScene();
            }
            else
            {
                Debug.Log("No file data received.");
            }
        }
        else
        {
            //Debug.LogError("Error uploading audio: " + request.error);
            //Debug.LogError("Response text: " + request.downloadHandler.text);
        }

        isUploading = false; // 업로드 완료 후 플래그 해제
    }

    // 씬 전환 함수
    void GoOKScene()
    {
        Debug.Log("Navigating to OKScene...");
        SceneManager.LoadScene("OKScene");
    }
}
