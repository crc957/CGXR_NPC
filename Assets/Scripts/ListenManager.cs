using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.SceneManagement;  // �� ��ȯ�� ���� UnityEngine.SceneManagement ���ӽ����̽� �߰�

public class ListenManager : MonoBehaviour
{
    public Button uploadButton;
    public string flaskServerUrl = "https://d7ec-163-239-126-56.ngrok-free.app/upload"; // Flask ���� ���� �ּ� �Ǵ� ngrok �ּ�
    private string filePath;
    private bool isUploading = false; // ���ε� ���¸� ��Ÿ���� �÷���

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

        Debug.Log("Flask Server URL: " + flaskServerUrl); // Flask ���� URL ���
        uploadButton.onClick.AddListener(UploadToFlaskServer);
    }

    void UploadToFlaskServer()
    {
        if (File.Exists(filePath) && !isUploading)
        {
            isUploading = true; // ���ε� ���� �� �÷��� ����
            StartCoroutine(UploadWav(filePath)); // ���� ���ε� ��û
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

        // ���͸�ũ ���� ���� ���� (�׽�Ʈ�� true ����)
        bool isWatermarkEnabled = true; // �ʿ信 ���� true �Ǵ� false�� ����

        // List<IMultipartFormSection>�� ����Ͽ� ������ ������ �� �����͸� ����
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();
        form.Add(new MultipartFormFileSection("file", fileData, "recordedAudio.wav", "audio/wav"));
        Debug.Log("File section added to form");

        // ���͸�ũ ���� ���� �߰� - Boolean ���� ���ڿ� "true" �Ǵ� "false"�� ��ȯ�Ͽ� ����
        form.Add(new MultipartFormDataSection("watermarkCheck", isWatermarkEnabled.ToString().ToLower()));
        Debug.Log("Watermark check section added to form");

        // UnityWebRequest.Post�� ����Ͽ� ������ �� �����ͷ� ����
        UnityWebRequest request = UnityWebRequest.Post(flaskServerUrl, form);
        Debug.Log("UnityWebRequest created");

        yield return request.SendWebRequest();
        Debug.Log($"Request sent. Status: {request.result}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            // ���信�� ���͸�ũ�� ������ �����͸� �޾Ƽ� ����
            if (request.downloadHandler.data.Length > 0)
            {
                string watermarkedFilePath = Path.Combine(Application.persistentDataPath, "watermarkedAudio.wav");
                File.WriteAllBytes(watermarkedFilePath, request.downloadHandler.data);
                Debug.Log("File received successfully and saved to: " + watermarkedFilePath);

                // OKScene���� ��ȯ
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

        isUploading = false; // ���ε� �Ϸ� �� �÷��� ����
    }

    // �� ��ȯ �Լ�
    void GoOKScene()
    {
        Debug.Log("Navigating to OKScene...");
        SceneManager.LoadScene("OKScene");
    }
}
