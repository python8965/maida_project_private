using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public Canvas canvas1;
    public Canvas canvas2;
    public Canvas canvas3;

    public Button canvas1ToCanvas2Button;
    public Button canvas1ToCanvas2Button2;
    public Button canvas1ToCanvas2Button3;
    public Button canvas2ToCanvas3Button;
    public Button canvas3ToCanvas2Button;
    public Button canvas2ToCanvas1Button;
    public Button canvas2ToCanvas1Button2;
    public Button canvas2ToCanvas1Button3;
    public Button canvas2ToCanvas3Buttonsub;
    public Button canvas3ToCanvas2Buttonsub;

    public GameObject[] objectsToActivateOnCanvas2;

    private void Start()
    {
        // Initialize canvases
        ShowCanvas1();

        // Add listeners to buttons
        canvas1ToCanvas2Button.onClick.AddListener(ShowCanvas2);
        canvas1ToCanvas2Button2.onClick.AddListener(ShowCanvas2);
        canvas1ToCanvas2Button3.onClick.AddListener(ShowCanvas2);
        canvas2ToCanvas3Button.onClick.AddListener(ShowCanvas3);
        canvas3ToCanvas2Button.onClick.AddListener(ShowCanvas2);
        canvas2ToCanvas1Button.onClick.AddListener(ShowCanvas1);
        canvas2ToCanvas1Button2.onClick.AddListener(ShowCanvas1);
        canvas2ToCanvas1Button3.onClick.AddListener(ShowCanvas1);
        canvas2ToCanvas3Buttonsub.onClick.AddListener(ShowCanvas3);
        canvas3ToCanvas2Buttonsub.onClick.AddListener(ShowCanvas2);
    }

    private void ShowCanvas1()
    {
        canvas1.gameObject.SetActive(true);
        canvas2.gameObject.SetActive(false);
        canvas3.gameObject.SetActive(false);
        SetObjectsActive(false); // Ensure the objects are inactive
    }

    private void ShowCanvas2()
    {
        canvas1.gameObject.SetActive(false);
        canvas2.gameObject.SetActive(true);
        canvas3.gameObject.SetActive(false);
        SetObjectsActive(true); // Activate the objects
    }

    private void ShowCanvas3()
    {
        canvas1.gameObject.SetActive(false);
        canvas2.gameObject.SetActive(false);
        canvas3.gameObject.SetActive(true);
        SetObjectsActive(false); // Ensure the objects are inactive
    }

    private void SetObjectsActive(bool isActive)
    {
        foreach (GameObject obj in objectsToActivateOnCanvas2)
        {
            obj.SetActive(isActive);
        }
    }
}
