using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerCamera : MonoBehaviour
{
    //GameObject player;
    Vector3 offset;
    Camera mainCam;
    private float camera_distanse=1f;
    float rotateSpeed=200f, moveSpeed=5f, moveRange=100f,offsetRange;

    float mouseX,mouseY;

    Vector3 camPos, playerPos;    //Camera Position
    Vector2 movePos;
    bool cameraReady, isFollowing;
    public bool isThirdPersonView{get; private set;}

    void Awake(){
        cameraReady=false;
        isFollowing=false;
        offsetRange=0f;
    }
    void Start(){
        mainCam=Camera.main;
        if(mainCam==null) Debug.LogError("maincam is null");
        
        if(GetRange()>144){
            offset=(transform.position-mainCam.transform.position).normalized*12;
            mainCam.transform.position=transform.position-offset;
           
        }
         mainCam.transform.LookAt(transform.position);
    }

    private void LateUpdate() {
        camPos=mainCam.transform.position;
        playerPos=transform.position;
        playerPos.y+=1;
        CameraStartMove();
        MoveCamera();
    RotateCamera();

        
    }
    public void CameraReady(){
        cameraReady=true;
    }
    void CameraStartMove(){
        if(!cameraReady) return;
        if(isFollowing) return;
        if(GetRange()>64){
            offset=(playerPos-camPos).normalized*offsetRange;
            mainCam.transform.position=Vector3.Lerp(camPos,playerPos,0.4f*Time.deltaTime);
        }
        else{
            offset=(playerPos-camPos).normalized*offsetRange;
            mainCam.transform.position=playerPos-offset;
            mainCam.transform.LookAt(playerPos);
            isFollowing=true;
        }
    }

    void MoveCamera(){

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
        mouseX=Input.GetAxis("Mouse X")*Time.deltaTime * rotateSpeed;
        mouseY=Input.GetAxis("Mouse Y")*Time.deltaTime * rotateSpeed;
        
        if(mouseX!=0 || mouseY!=0){
            if(isThirdPersonView){
                mainCam.transform.position=playerPos-offset;
                mainCam.transform.RotateAround(transform.position, Vector3.up, mouseX);
                
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

            else{   //firstPersonView
                /*Vector3 ang=mainCam.transform.eulerAngles;
                ang+=new Vector3(-mouseY,mouseX);
                Mathf.Clamp(ang.x,)*/
                mainCam.transform.eulerAngles+=new Vector3(-mouseY,mouseX);
            }
        }
    }
}