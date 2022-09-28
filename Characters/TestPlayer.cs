using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{

//===render====
    [SerializeField] GameObject[] hideObj;
    private static TestPlayer localPlayerInstance;
    public static TestPlayer LocalPlayerInstance{
        get{
            if(localPlayerInstance==null) return null;
            return localPlayerInstance;
        }
    }
    float hori;     //horizontal axes
    float vert;     //vertical axes
    bool runState;
    bool jump;
    public bool canMove{get; set;}
    public bool isThirdPerson{get; private set;}
    

    public int playerNumber{get; private set;}
    public string playerName{get; private set;}
    public bool gameEnd{get;private set;}
    
    
    //===move====
    [SerializeField] float speed=0.1f;
    [SerializeField] float rotateSpeed=0.1f;
    [SerializeField] float jumpPower;
    Quaternion newRotation;
    Vector3 dirForward;
    Vector3 movement;  
    Vector3 dirRight;
    int jumpCount=1;      //점프 횟수 제한
    float fallingPoint=-5;
    Vector3 startPoint;
    
    

    Rigidbody rigid; 
    Animator anim;
    TestPlayerCamera playerCamera;
    Camera mainCam;


    

    void Awake(){
        if(localPlayerInstance==null){
            localPlayerInstance=this;
            DontDestroyOnLoad(this.gameObject);

        }
        else{
        }

        
        rigid=GetComponent<Rigidbody>();
        anim=GetComponent<Animator>();
        playerCamera=GetComponent<TestPlayerCamera>();
        
        SetFirstPersonView();

    }
    void Start()
    {
        
            playerCamera.CameraReady();
            runState=false;
            jump=false;
        gameEnd=false;
        mainCam=Camera.main;

    }

    void FixedUpdate() {
        hori=Input.GetAxis("Horizontal");
        vert=Input.GetAxis("Vertical");
        Run();
        Turn();
        
        
        
    }
    void Update()
    {
        Jump();
        CheckFall();
       
    }
    //===========Move=========

    void Run(){
        if(hori==0 && vert==0) {
            if(runState) {
                runState=false;
                anim.SetInteger("RunState", 0);
            }
            return;
        }
        if(!runState) {
            runState=true;
            anim.SetInteger("RunState", 2);
        }
        if(isThirdPerson){
            anim.SetInteger("RunState", 2);
            dirForward=transform.position-mainCam.transform.position;
            dirRight=Quaternion.AngleAxis(90, Vector3.up) * dirForward;
            movement=(dirForward*vert + dirRight*hori);
            movement.y=0;
            movement=movement.normalized*speed;
            
        }
        else{   //first Person
            dirForward=mainCam.transform.forward;
            dirForward.y=0;
            dirRight=Quaternion.AngleAxis(90, Vector3.up) * dirForward;
            movement=(dirForward*vert + dirRight*hori).normalized*speed;
        }
        rigid.MovePosition(transform.position+movement);

    }
    public void Move(Vector3 movePos){
        if(!canMove){
            runState=false;
            anim.SetInteger("RunState", 0);
            return;
        }
        if(movePos==Vector3.zero){
            if(runState) {
                runState=false;
                anim.SetInteger("RunState", 0);
            }
            return;
        }
        if(!runState) runState=true;
        anim.SetInteger("RunState", 2);
        rigid.MovePosition(transform.position+movePos*speed);
    }
    public void ChangeMoveVector(float Hori, float Vert){
        hori=Hori;
        vert=Vert;
    }
    void Turn(){
        

        if(isThirdPerson) {
            if(hori==0 && vert==0) return;
            if(jump) return;
            newRotation=Quaternion.LookRotation(movement);
            rigid.rotation=Quaternion.Slerp(rigid.rotation, newRotation,rotateSpeed);
        }
            

        else {
            dirForward=mainCam.transform.forward;
            dirForward.y=0;
            newRotation=Quaternion.LookRotation(dirForward);
            rigid.rotation=Quaternion.Slerp(rigid.rotation, newRotation,rotateSpeed);
        }

        
        
    }

    public void Jump(){
        if(Input.GetButtonDown("Jump")){
            //if(jump) return;
            jump=true;
            anim.SetBool("Jump", true);
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }

    }
    public void Attack(){
        anim.SetTrigger("Attack");
    }

    void CheckFall(){
        if(transform.position.y<fallingPoint){
            transform.position=startPoint;
        }

    }
///////////////////////////////////////
    public void GameEnd(){
        gameEnd=true;
    }
    public void StopMove(){
        canMove=false;
        anim.SetInteger("RunState", 0);
    }

    void AnimationUpdate(){
        //if(runState) anim.SetBool("RunState", true);
        
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag=="Ground"){
            if(jump){
                jump=false;
                anim.SetBool("Jump", false);
            }
        }
    }
    private void OnTriggerExit(Collider other) {
        /*if(other.gameObject.tag=="Ground"){
                jump=true;
        }*/
    }

    public void SetThirdPersonView(){
        isThirdPerson=true;
        foreach(GameObject obj in hideObj) obj.SetActive(isThirdPerson);
    }
    public void SetFirstPersonView(){
        isThirdPerson=false;
        foreach(GameObject obj in hideObj) obj.SetActive(isThirdPerson);
    }
}
