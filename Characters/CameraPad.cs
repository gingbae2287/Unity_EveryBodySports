using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraPad : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Camera cam;
    //PlayerCamera playerCamera;
    IEnumerator cor;
    WaitForSeconds delay;
    Vector2 dragPos;
    bool isPlayerFind;
    private void Awake() {
        delay=new WaitForSeconds(0.1f);
        cor=FindPlayer();
    }
    private void OnEnable() {
        cam=Camera.main;
        cor=FindPlayer();
        StartCoroutine(cor);
        
    }
    private void OnDisable() {
        StopCoroutine(cor);
        isPlayerFind=false;
    }
    IEnumerator FindPlayer(){
        while(Player.LocalPlayerInstance==null){
            yield return delay;
        }
        isPlayerFind=true;
        //playerCamera=Player.LocalPlayerInstance.GetComponent<PlayerCamera>();
    }
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        dragPos=eventData.pressPosition;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if(!isPlayerFind) return;
        Player.LocalPlayerInstance.playerCamera.SetRotateVector(eventData.position-dragPos);
        dragPos=eventData.position;
        
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        Player.LocalPlayerInstance.playerCamera.SetRotateVector(Vector2.zero);
    }
}