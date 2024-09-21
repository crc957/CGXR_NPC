using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    public Button startButton;

    void Start()
    {
        startButton.onClick.AddListener(GoToRecordScene);
    }

    void GoToRecordScene()
    {
        SceneManager.LoadScene("RecordScene");
    }
}
