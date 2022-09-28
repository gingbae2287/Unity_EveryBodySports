using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Crosshair : MonoBehaviour,IPointerDownHandler{
    Camera cam;
    Ray ray;
    RaycastHit hit;
    [SerializeField]Image image;
    Vector3 camScreenPos;
    HideStuff stuffObj;
    bool isStuff;
    Color[] colors={
        new Color(0.8f,0.4f,0.4f,0.5f),
        new Color(1f,1f,1f,0.5f)
    };
    float rayRange=2f;
    private void Awake() {
        cam=Camera.main;

        camScreenPos=new Vector3(0.5f,0.5f,0);
        image.color=colors[1];
    }

    

    private void Update() {
        ray=cam.ViewportPointToRay(camScreenPos);
        if(Physics.Raycast(ray,out hit, rayRange)){
            if(!isStuff && hit.collider.gameObject.CompareTag("Stuff")) {
                stuffObj=hit.collider.gameObject.GetComponent<HideStuff>();
                image.color=colors[0];
                isStuff=true;
            }
            else if(isStuff && !hit.collider.gameObject.CompareTag("Stuff")){
                isStuff=false;
                stuffObj=null;
                image.color=colors[1];
            }
        }
        else {
            if(!isStuff) return;
            isStuff=false;
            stuffObj=null;
            image.color=colors[1];
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if(!Player.LocalPlayerInstance.Attack()) return;
        if(isStuff){
            if(stuffObj.isHuman){
                stuffObj.Catched();
            }
            else{
                Debug.Log("사람아님!");
                HideAndSeek.Instance.LifeMinus();
            }
        }
    }
}