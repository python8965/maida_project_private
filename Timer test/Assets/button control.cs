using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button button3D;
    public Button buttonCam1;
    public Button buttonCam2;
    public Button buttonCam3;

    private Color blueColor;

    private void Start()
    {
        // Convert hex code to Color
        ColorUtility.TryParseHtmlString("#69ADFF", out blueColor);

        // Add listeners to buttons
        button3D.onClick.AddListener(() => OnButtonClicked(button3D));
        buttonCam1.onClick.AddListener(() => OnButtonClicked(buttonCam1));
        buttonCam2.onClick.AddListener(() => OnButtonClicked(buttonCam2));
        buttonCam3.onClick.AddListener(() => OnButtonClicked(buttonCam3));

        // Initialize button colors
        OnButtonClicked(button3D);
    }

    private void OnButtonClicked(Button clickedButton)
    {
        // Set all buttons to black
        SetButtonColor(button3D, Color.black);
        SetButtonColor(buttonCam1, Color.black);
        SetButtonColor(buttonCam2, Color.black);
        SetButtonColor(buttonCam3, Color.black);

        // Set the clicked button to blue
        SetButtonColor(clickedButton, blueColor);
    }

    private void SetButtonColor(Button button, Color color)
    {
        ColorBlock cb = button.colors;
        cb.normalColor = color;
        cb.selectedColor = color;
        cb.pressedColor = color;
        cb.highlightedColor = color;
        button.colors = cb;
    }
}
