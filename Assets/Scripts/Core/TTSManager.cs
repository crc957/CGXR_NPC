using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using NAudio.Wave;

public class TTSManager : MonoBehaviour
{
    [SerializeField] private OpenAIWrapper openAIWrapper;
    [SerializeField] private TTSModel model = TTSModel.TTS_1;
    [SerializeField] private TTSVoice voice = TTSVoice.Alloy;
    [SerializeField, Range(0.25f, 4.0f)] private float speed = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        if (openAIWrapper == null)
        {
            Debug.LogError("openAIWrapper가 Inspector에서 할당되지 않았습니다.");
        }
       // if (audioSource == null)
       // {
       //     Debug.LogError("audioSource가 Inspector에서 할당되지 않았습니다.");
       // }
    }

    public async void SynthesizeAndPlay(string text)
    {
        try
        {
            if (openAIWrapper == null)
            {
                Debug.LogError("openAIWrapper is null. Make sure it is initialized before calling SynthesizeAndPlay.");
                return;
            }

            string cleanedText = CleanInput(text);

            if (string.IsNullOrEmpty(cleanedText))
            {
                Debug.LogWarning("Cleaned text is empty, skipping TTS.");
                return;
            }

            Debug.Log("Requesting TTS from OpenAI with text: " + cleanedText);

            byte[] audioData = await openAIWrapper.RequestTextToSpeech(cleanedText, model, voice, speed);

            if (audioData != null && audioData.Length > 0)
            {
                Debug.Log("TTS request successful, received audio data. Length: " + audioData.Length);

                // MP3 데이터를 WAV로 변환
                string wavFilePath = Path.Combine(Application.persistentDataPath, "GeneratedAudio.wav");
                openAIWrapper.ConvertMp3ToWav(audioData, wavFilePath);

                // WAV 파일에서 AudioClip 생성
                AudioClip clip = LoadWavFromFile(wavFilePath);
                Debug.Log("AudioClip loaded: " + (clip != null));
                Debug.Log($"AudioClip details: length={clip.length}, samples={clip.samples}, channels={clip.channels}");

                if (clip == null)
                {
                    Debug.LogError("Failed to load AudioClip from the generated WAV file.");
                    return;
                }

                // UMC_12_Casual_Shirt_Blue 오브젝트의 AudioSource 가져오기
                GameObject umcShirtBlue = GameObject.Find("UMC_12_Casual_Shirt_Blue");
                if (umcShirtBlue == null)
                {
                    Debug.LogError("UMC_12_Casual_Shirt_Blue 게임 오브젝트를 찾을 수 없습니다.");
                    return;
                }

                AudioSource shirtAudioSource = umcShirtBlue.GetComponent<AudioSource>();
                if (shirtAudioSource == null)
                {
                    Debug.LogError("UMC_12_Casual_Shirt_Blue 오브젝트에 AudioSource가 없습니다.");
                    return;
                }

                // AudioSource에 AudioClip 할당 및 재생
                try
                {
                    shirtAudioSource.clip = clip;
                    Debug.Log("AudioSource.clip assigned to UMC_12_Casual_Shirt_Blue: " + (shirtAudioSource.clip != null));
                    Debug.Log($"AudioClip length: {clip.length}, samples: {clip.samples}");
                    Debug.Log($"AudioSource settings: volume={shirtAudioSource.volume}, mute={shirtAudioSource.mute}, " +
                              $"clip length={shirtAudioSource.clip.length}, clip samples={shirtAudioSource.clip.samples}");

                    shirtAudioSource.Play();
                    Debug.Log($"Is AudioSource playing on UMC_12_Casual_Shirt_Blue? {shirtAudioSource.isPlaying}");
                }
                catch (Exception ex)
                {
                    Debug.LogError("An unexpected error occurred while playing audio: " + ex.Message);
                    Debug.LogError("Stack Trace: " + ex.StackTrace);
                }
            }
            else
            {
                Debug.LogError("Failed to get audio data from OpenAI. Audio data is null or empty.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("An unexpected error occurred: " + ex.Message);
            Debug.LogError("Stack Trace: " + ex.StackTrace);
        }
    }



    private void PlayOnOtherAudioSource(AudioClip clip, string gameObjectName)
    {
        GameObject targetObject = GameObject.Find(gameObjectName);
        if (targetObject != null)
        {
            Debug.Log("Found " + gameObjectName + " game object.");

            AudioSource targetAudioSource = targetObject.GetComponent<AudioSource>();
            if (targetAudioSource != null)
            {
                targetAudioSource.clip = clip;
                targetAudioSource.Play();
                Debug.Log("Playing audio on " + gameObjectName + "'s AudioSource.");
            }
            else
            {
                Debug.LogError(gameObjectName + "에 AudioSource가 없습니다.");
            }
        }
        else
        {
            Debug.LogError(gameObjectName + " 게임 오브젝트를 찾을 수 없습니다.");
        }
    }

    private AudioClip LoadWavFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("WAV file not found: " + path);
            return null;
        }

        using (var reader = new WaveFileReader(path))
        {
            int sampleCount = (int)reader.SampleCount;
            int channelCount = reader.WaveFormat.Channels;
            int sampleRate = reader.WaveFormat.SampleRate;

            float[] data = new float[sampleCount];
            int index = 0;

            while (index < sampleCount)
            {
                float[] frame = reader.ReadNextSampleFrame();
                for (int i = 0; i < channelCount; i++)
                {
                    data[index++] = frame[i];
                }
            }

            AudioClip audioClip = AudioClip.Create(Path.GetFileNameWithoutExtension(path), sampleCount, channelCount, sampleRate, false);
            audioClip.SetData(data, 0);

            return audioClip;
        }
    }

    private string CleanInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string cleanedText = input.Replace("\r", "").Replace("\n", " ").Replace("\t", " ");
        cleanedText = System.Text.RegularExpressions.Regex.Replace(cleanedText, @"[^a-zA-Z0-9\s.,!?]", "");

        return cleanedText;
    }

    public void SynthesizeAndPlay(string text, TTSModel model, TTSVoice voice, float speed)
    {
        this.model = model;
        this.voice = voice;
        this.speed = speed;
        SynthesizeAndPlay(text); // 매개변수 없는 버전을 호출
    }
}
