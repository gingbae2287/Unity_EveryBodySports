using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //GameObject player;
    Vector3 offset;
    Camera mainCam;
    private float camera_distanse=1f;
    float rotateSpeed=10f, moveSpeed=5f, moveRange=80f, offsetRange;
    float thirdOffsetRange=8f;
    float firstOffsetRange=0f;


    float mouseX,mouseY;

    Vector3 camPos, playerPos, angle;    //Camera Position
    Vector2 movePos;
    bool cameraReady, isFollowing;
    public bool isThirdPersonView{get; private set;}
    public bool freeCamMode{get; private set;}

    void Awake(){
        cameraReady=false;
        isFollowing=false;
        //offsetRange=0f;
        mainCam=Camera.main;
    }
    private void OnEnable() {
        //SetPlayerPosition();
    }
    void Start(){
        
    }
    public void LookPlayer(){
        if(GetRange()>144){
            offset=(transform.position-mainCam.transform.position).normalized*12;
            mainCam.transform.position=transform.position-offset;
           
        }
         mainCam.transform.LookAt(transform.position);
    }

    private void LateUpdate() {
        if(freeCamMode) return;
        if(!cameraReady) return;
        camPos=mainCam.transform.position;
        playerPos=transform.position;
        playerPos.y+=1;
        CameraStartMove();
        MoveCamera();
        
    }
    public void CameraReady(){
        cameraReady=true;
    }
    void CameraStartMove(){
        if(!cameraReady) return;
        if(isFollowing) return;
        if(GetRange()>64){
            mainCam.transform.position=Vector3.Lerp(camPos,playerPos,0.6f*Time.deltaTime);
        }
        else{
            offset=(playerPos-camPos).normalized*offsetRange;
            mainCam.transform.position=playerPos-offset;
            mainCam.transform.LookAt(playerPos);
            isFollowing=true;
        }
    }

    void MoveCamera(){
        if(!cameraReady) return;
        if(!isFollowing) return;
        mainCam.transform.position=playerPos-offset;
    }
    float GetRange(){
        return (transform.position-mainCam.transform.position).sqrMagnitude;
    }
    public void SetRotateVector(Vector2 pos){
        movePos=Vector2.ClampMagnitude(pos,moveRange);
        mouseX=movePos.x*Time.deltaTime * rotateSpeed;
        mouseY=movePos.y*Time.deltaTime * rotateSpeed;
        RotateCamera();
    }
    void RotateCamera(){
        if(!cameraReady) return;
        if(!isFollowing) return;
        if(GameManager.Instance.pause) return;
        //mouseX=Input.GetAxis("Mouse X")*Time.deltaTime * rotateSpeed;
        //mouseY=Input.GetAxis("Mouse Y")*Time.deltaTime * rotateSpeed;
        
        if(mouseX!=0 || mouseY!=0){
            if(freeCamMode) {
            angle=mainCam.transform.eulerAngles+new Vector3(-mouseY,mouseX);
            if(angle.x>80f && angle.x<180f) angle.x=80f;
            else if(angle.x<280f&&angle.x>180f) angle.x=280f;
            angle.z=0;
            mainCam.transform.eulerAngles=angle;
            return;
        }
            
            if(isThirdPersonView){
                
                mainCam.transform.position=playerPos-offset;
                mainCam.transform.RotateAround(playerPos, Vector3.up, mouseX);
                
                if(camPos.y<playerPos.y+0f) {
                    if(mouseY<0) mainCam.transform.RotateAround(playerPos, mainCam.transform.right, -mouseY);
                }
                else if(camPos.y>playerPos.y+6f) 
                {
                    if(mouseY>0) mainCam.transform.RotateAround(playerPos, mainCam.transform.right, -mouseY);
                }
                else{

                    mainCam.transform.RotateAround(playerPos, mainCam.transform.right, -mouseY);
                }
                
                camPos=mainCam.transform.position;
                offset=playerPos-camPos;
            }

            else{  
                angle=mainCam.transform.eulerAngles+new Vector3(-mouseY,mouseX);
                if(angle.x>80f && angle.x<180f) angle.x=80f;
                else if(angle.x<280f&&angle.x>180f) angle.x=280f;
                angle.z=0;
                mainCam.transform.eulerAngles=angle;
                
            }
        }
    }
    public void SetPlayerPosition(){
        if(isThirdPersonView){
        camPos=mainCam.transform.position;
        playerPos=transform.position;
        camPos.y=transform.position.y+1;
        offset=(playerPos-camPos).normalized*offsetRange;
        mainCam.transform.position=playerPos-offset;
        mainCam.transform.LookAt(playerPos);
        }
        else {
            isFollowing=true;
            mainCam.transform.position=transform.position;
            mainCam.transform.rotation=transform.rotation;
        }
    }
    public void SetThirdPersonView(bool isThird){
        if(isThird) offsetRange=thirdOffsetRange;
        else  offsetRange=firstOffsetRange;
        isThirdPersonView=isThird;
    }
    public virtual void SetFreeCamMode(bool isFreeCamMode){
        isFollowing=true;
        freeCamMode=isFreeCamMode;
    }
}