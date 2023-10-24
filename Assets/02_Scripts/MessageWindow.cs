using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour {
    [SerializeField] private Image messageIcon;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI buttonText;

    public void ShowMeaagse(Sprite sprite = null, string message = "", string buttonMsg = "START") {
        if(messageIcon != null) {
            messageIcon.sprite = sprite;
        }
        if(messageText != null) {
            messageText.text = message;
        }
        if(buttonMsg != null) {
            buttonText.text = buttonMsg;
        }
    }
}
