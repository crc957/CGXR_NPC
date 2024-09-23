using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContractManager : MonoBehaviour
{
    public Button[] buttons; // �����ϴ� ��ư
    public Color defaultColor = Color.white; // �⺻ ����
    public Color clickedColor = Color.green; // Ŭ�� �� ����
    public Toggle toggle; // ���͸�ũ ���

    private Image[] buttonImages;
    private bool[] isClicked;

    // ���͸�ũ �ɼ� ������ ���� ��ųʸ�
    private Dictionary<string, bool> watermarkOptions = new Dictionary<string, bool>();

    void Start()
    {
        // �� ��ư �⺻ �̹��� ������Ʈ ����
        buttonImages = new Image[buttons.Length];

        // �� ��ư Ŭ�� ���� ���� �迭
        isClicked = new bool[buttons.Length];

        // �� ��ư �⺻ ���� ���� �� Ŭ�� �̺�Ʈ ���
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttonImages[i] = buttons[i].GetComponent<Image>();
            buttonImages[i].color = defaultColor; // �⺻ ���� ����
            isClicked[i] = false; // ó���� Ŭ������ ���� ����

            // ��ư Ŭ�� �̺�Ʈ�� ���� �ٸ� �ε����� ���� �޼ҵ� ����
            buttons[i].onClick.AddListener(() => OnButtonClick(index));

            // ��ųʸ��� �� ��ư�� ���� �߰�(��ư �̸��� Ű�� ���)
            if (!watermarkOptions.ContainsKey(buttons[i].name))
            {
                watermarkOptions.Add(buttons[i].name, false);
            }
        }

        // Toggle�� ���°� �ٲ� �� ȣ��Ǵ� �̺�Ʈ ���
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnButtonClick(int index)
    {
        // ��ư�� �̹� Ŭ���Ǿ� �ʷϻ��̸� �⺻ ��������, �ƴϸ� �ʷϻ����� ����
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

    // Toggle ���� ���� �� ȣ��Ǵ� �޼ҵ�
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
