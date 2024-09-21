using NAudio.Wave;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OpenAIWrapper : MonoBehaviour
{
    [SerializeField, Tooltip("Your OpenAI API key. If you use a restricted key, please ensure that it has permissions for /v1/audio.")]
    private string openAIKey = "api-key";
    private readonly string outputFormat = "mp3";

    [System.Serializable]
    private class TTSPayload
    {
        public string model;
        public string input;
        public string voice;
        public string response_format;
        public float speed;
    }

    public async Task<byte[]> RequestTextToSpeech(string text, TTSModel model = TTSModel.TTS_1, TTSVoice voice = TTSVoice.Alloy, float speed = 1f)
    {
        Debug.Log(text);
        Debug.Log("Sending new request to OpenAI TTS.");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAIKey);

        TTSPayload payload = new TTSPayload
        {
            model = model.EnumToString(),
            input = text,
            voice = voice.ToString().ToLower(),
            response_format = this.outputFormat,
            speed = speed
        };

        string jsonPayload = JsonUtility.ToJson(payload);

        var httpResponse = await httpClient.PostAsync(
            "https://api.openai.com/v1/audio/speech",
            new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        );

        byte[] response = await httpResponse.Content.ReadAsByteArrayAsync();

        if (httpResponse.IsSuccessStatusCode)
        {
            Debug.Log("Received response from OpenAI TTS.");
            return response;
        }

        Debug.Log("Error: " + httpResponse.StatusCode);
        return null;
    }

    public void SetAPIKey(string openAIKey) => this.openAIKey = openAIKey;

    // MP3 데이터를 WAV 파일로 변환하는 함수
    public void ConvertMp3ToWav(byte[] mp3Data, string outputPath)
    {
        using (var mp3Stream = new MemoryStream(mp3Data))
        using (var mp3Reader = new Mp3FileReader(mp3Stream))
        using (var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader))
        using (var wavFileWriter = new WaveFileWriter(outputPath, waveStream.WaveFormat))
        {
            waveStream.CopyTo(wavFileWriter);
        }

        Debug.Log($"Converted MP3 data to WAV format and saved to: {outputPath}");
    }
}