using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dpad : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler {
    Vector3 padPos;
    [SerializeField]GameObject pad;
    Image backgroundImage, padImage;
    RectTransform rect;
    float padRange=50f;
    bool isClick;
    
    Color[] colors={
        new Color(1f,1f,1f,0.8f),
        new Color(1f,1f,1f,0.3f)
    };
    private void Awake() {
        backgroundImage=GetComponent<Image>();
        padImage=pad.GetComponent<Image>();
        rect=GetComponent<RectTransform>();
        backgroundImage.color=colors[0];
        padImage.color=colors[0];
        
    }
    private void OnEnable() {
        isClick=false;
    }
    void SetPadSize(bool click){
        float size=150f;
        if(click) size=180f;
        rect.sizeDelta= new Vector2(size, size);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(GameManager.Instance.pause) return;
        if(isClick) return;
        isClick=true;
        backgroundImage.color=colors[1];
        padImage.color=colors[1];
        SetPadSize(isClick);
        
    }
    public void OnDrag(PointerEventData eventData)
    {
        //드래그 중이지만 포인트가 움직였을때 만 호출된다. 꾹 누르고 가만히있음 호출 X
        if(GameManager.Instance.pause) return;
        if(Player.LocalPlayerInstance==null) return;
        padPos=eventData.position-eventData.pressPosition;
        padPos=Vector3.ClampMagnitude(padPos,padRange);
        pad.transform.localPosition=padPos;

        Player.LocalPlayerInstance.ChangeMoveVector(padPos.x/padRange, padPos.y/padRange); //nomalized
        
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        isClick=false;
        padPos=Vector3.zero;
        Player.LocalPlayerInstance.ChangeMoveVector(0,0);
        pad.transform.localPosition=padPos;
        backgroundImage.color=colors[0];
        padImage.color=colors[0];
        SetPadSize(isClick);
    }
}
