using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public Button buttonA;
    public Button buttonB;
    public Button buttonC;
    public Button buttonA_Activate;
    public Button buttonB_Activate;
    public Button buttonC_Activate;
    public GameObject scriptA;
    public GameObject scriptB;
    public GameObject scriptC;

    void Start()
    {
        buttonA.onClick.AddListener(() => ActivateButtonAndScript(buttonA_Activate, scriptA));
        buttonB.onClick.AddListener(() => ActivateButtonAndScript(buttonB_Activate, scriptB));
        buttonC.onClick.AddListener(() => ActivateButtonAndScript(buttonC_Activate, scriptC));

        // 초기 상태에서는 모든 활성화 버튼과 스크립트를 비활성화
        DeactivateAll();
    }

    void ActivateButtonAndScript(Button activateButton, GameObject script)
    {
        // 모든 버튼과 스크립트를 비활성화
        DeactivateAll();

        // 선택된 활성화 버튼과 스크립트만 활성화
        activateButton.gameObject.SetActive(true);
        script.SetActive(true);
    }

    void DeactivateAll()
    {
        buttonA_Activate.gameObject.SetActive(false);
        buttonB_Activate.gameObject.SetActive(false);
        buttonC_Activate.gameObject.SetActive(false);
        scriptA.SetActive(false);
        scriptB.SetActive(false);
        scriptC.SetActive(false);
    }
}
