using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContractManager : MonoBehaviour
{
    public Button[] buttons; // 선택하는 버튼
    public Color defaultColor = Color.white; // 기본 색상
    public Color clickedColor = Color.green; // 클릭 후 색상
    public Toggle toggle; // 워터마크 토글

    private Image[] buttonImages;
    private bool[] isClicked;

    // 워터마크 옵션 저장을 위한 딕셔너리
    private Dictionary<string, bool> watermarkOptions = new Dictionary<string, bool>();

    void Start()
    {
        // 각 버튼 기본 이미지 컴포넌트 저장
        buttonImages = new Image[buttons.Length];

        // 각 버튼 클릭 상태 저장 배열
        isClicked = new bool[buttons.Length];

        // 각 버튼 기본 색상 설정 및 클릭 이벤트 등록
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttonImages[i] = buttons[i].GetComponent<Image>();
            buttonImages[i].color = defaultColor; // 기본 색상 설정
            isClicked[i] = false; // 처음엔 클릭되지 않은 상태

            // 버튼 클릭 이벤트에 각기 다른 인덱스를 가진 메소드 연결
            buttons[i].onClick.AddListener(() => OnButtonClick(index));

            // 딕셔너리에 각 버튼의 상태 추가(버튼 이름을 키로 사용)
            if (!watermarkOptions.ContainsKey(buttons[i].name))
            {
                watermarkOptions.Add(buttons[i].name, false);
            }
        }

        // Toggle의 상태가 바뀔 때 호출되는 이벤트 등록
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnButtonClick(int index)
    {
        // 버튼이 이미 클릭되어 초록색이면 기본 색상으로, 아니면 초록색으로 변경
        if (isClicked[index])
        {
            buttonImages[index].color = defaultColor;
            isClicked[index] = false;
        }
        else
        {
            buttonImages[index].color = clickedColor;
            isClicked[index] = true;
        }

        watermarkOptions[buttons[index].name] = isClicked[index];
    }

    // Toggle 상태 변경 시 호출되는 메소드
    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Toggle is On");
        }
        else
        {
            Debug.Log("Toggle is Off");
        }
    }

    public bool IsWatermarkEnabled()
    {
        return toggle.isOn;
    }
}
