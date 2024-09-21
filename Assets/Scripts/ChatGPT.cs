using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Text;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;
        [SerializeField] private TTSManager ttsManager;
      
        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "You are an immigration officer who is welcoming people into the United States. You are known for being kind and friendly in your interactions with those entering the country. just say a one-sentence response for each person. Don't break character. Don't ever mention that you are an AI model.";

        private void Start() { }

       
        private void AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            ChatMessage defaultMsg = default(ChatMessage);
            foreach (var msg in messages)
            {
                if (msg.Equals(defaultMsg))
                {
                    Debug.LogError("One of the ChatMessage objects is in its default state.");
                    return;
                }

            }

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            Debug.Log("Item is: " + item.name);

            MessageHandler messageHandler = item.GetComponent<MessageHandler>();

            Debug.Log("messageHandler assigned: " + (messageHandler != null));

            if (item == null)
            {
                Debug.LogError("Item is null, cannot set anchoredPosition.");
                return;
            }

            if (messageHandler == null)
            {
                Debug.LogError("messageHandler is null.");
                return;
            }

           // if (messageHandler.textMeshPro == null)
            //{
            //    Debug.LogError("textMeshPro is null.");
            //    return;
            //}

            if (message.Content == null)
            {
                Debug.LogError("Message content is null.");
                return;
            }

            //messageHandler.textMeshPro.text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);

            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;

            if (ttsManager != null && message.Role != "user") // "user"가 아닌 메시지만 음성으로 변환합니다.
            {
                // TTS에 전달하기 전에 텍스트를 정제합니다.
                string cleanedText = CleanInput(message.Content);
                ttsManager.SynthesizeAndPlay(cleanedText);
            }
        }


        // 텍스트를 정제하는 메서드 추가
        private string CleanInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 제어 문자 및 특수 문자 제거
            string cleanedText = input.Replace("\r", "").Replace("\n", " ").Replace("\t", " ");

            // 알파벳, 숫자, 구두점만 남기기
            cleanedText = System.Text.RegularExpressions.Regex.Replace(cleanedText, @"[^a-zA-Z0-9\s.,!?]", "");

            // UTF-8로 인코딩 후 디코딩
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(cleanedText);
            string utf8String = Encoding.UTF8.GetString(utf8Bytes);

            // UTF-16로 인코딩 후 디코딩
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf8String);
            string utf16String = Encoding.Unicode.GetString(utf16Bytes);

            return utf16String;
        }

        public void ReceiveTextFromWhisper(string text)
        {
            SendReply(text); // 응답을 보내는 기존 메소드 호출
        }

        private async void SendReply(string text)
        {
            // 새로운 메시지 생성 및 추가
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = text // 직접 받은 텍스트 사용
            };
            AppendMessage(newMessage);

            // 첫 번째 메시지에 프롬프트 추가 (필요한 경우)
            if (messages.Count == 0) newMessage.Content = prompt + "\n" + text;

            messages.Add(newMessage);

            // GPT 모델로부터 응답 받기
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-4",
                Messages = messages
            });

            // 응답 처리
            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
    }
}
