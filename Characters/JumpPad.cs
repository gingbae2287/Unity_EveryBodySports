using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JumpPad : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
{   
    Image btnImage;
    Color[] colors={
        new Color(1f,1f,1f,0.8f),
        new Color(1f,1f,1f,0.3f)
    };
    private void Awake() {
        btnImage=GetComponent<Image>();
        btnImage.color=colors[0];
    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        btnImage.color=colors[1];
        Player.LocalPlayerInstance.Jump();
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        btnImage.color=colors[0];
    }
}