using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player : MonoBehaviourPun
{

    private static Player localPlayerInstance;
    public static Player LocalPlayerInstance{
        get{
            if(localPlayerInstance==null) return null;
            return localPlayerInstance;
        }
    }
    protected float hori;     //horizontal axes
    protected float vert;     //vertical axes
    protected bool runState;
    protected bool jump;
    public bool canMove{get; private set;}
    
    public int playerNumber{get; private set;}
    public string playerName{get; private set;}
    //===이동관련===
    public const float playerSpeed=0.1f;
    public const float playerJumpPower=250;
    protected float speed=1f;
    protected float rotateSpeed=0.1f;
    protected float jumpPower=250;
    protected Vector3 dirForward,movement,dirRight;
    protected Quaternion newRotation;
    protected int jumpCount=1;      //점프 횟수 제한
    float fallingPoint=-5;
    Vector3 startPoint;
    //==기타===
    public bool isAttack{get; protected set;}
    protected float attackSpeed=1f;
    protected IEnumerator cor;
    protected WaitForSeconds attackDelay;
//===카메라 시점---
    public bool isThirdPerson{get; private set;}
    public bool freeCamMode{get; private set;}
    
//==component
    public PlayerCamera playerCamera{get; private set;}
    protected Rigidbody rigid; 
    
    protected Camera mainCam;

    

   protected virtual void Awake(){
        if(localPlayerInstance==null && photonView.IsMine){
            localPlayerInstance=this;
            //DontDestroyOnLoad(this.gameObject);

        }
        InitSetting();
    }
    private void OnEnable() {
        if(photonView.IsMine){
            localPlayerInstance=this;
           // DontDestroyOnLoad(this.gameObject);

        }
    }
    private void OnDisable() {
        if(localPlayerInstance==this && photonView.IsMine){
            localPlayerInstance=null;
            //Destroy(this);
        }
    }
    void InitSetting(){
        rigid=GetComponent<Rigidbody>();
        if(photonView.IsMine){
            if(playerCamera==null) playerCamera=gameObject.AddComponent<PlayerCamera>();
            mainCam=Camera.main;
            
            rigid.isKinematic=false;
            rigid.constraints=~RigidbodyConstraints.FreezePosition;
            rigid.useGravity=true;
            rigid.mass=40f;
            SetThirdPersonView(GameManager.Instance.isThirdPersonView);

            attackDelay=new WaitForSeconds(1f/attackSpeed);
        }
        else{
            rigid.isKinematic=true;
        }

    }
    void Start()
    {
        if(photonView.IsMine) {
            playerCamera.CameraReady();
            runState=false;
            jump=false;
            //시점 설정
        }

        //playerNumber=PhotonNetwork.LocalPlayer.ActorNumber;
        playerName=PhotonNetwork.LocalPlayer.NickName;
    }

    void FixedUpdate() {
        if(!photonView.IsMine) return;
        //hori=Input.GetAxis("Horizontal");
        //vert=Input.GetAxis("Vertical");
        Run();
        Turn();
    }
    void Update()
    {
        if(!photonView.IsMine) return;
        CheckFall();
        FreeCamMove();
       
    }
    //===========Move=========

    protected virtual void Run(){
        if(GameManager.Instance.pause) return;
        if(freeCamMode) return;
        
        if(!canMove){
            runState=false;
            return;
        }
        if(hori==0 && vert==0) {
            if(runState) {
                runState=false;
            }
            return;
        }
        if(!runState) {
            runState=true;
        }
        
        if(isThirdPerson) dirForward=transform.position-mainCam.transform.position;
        else   dirForward=mainCam.transform.forward;
        
        dirForward.y=0;
        dirRight=Quaternion.AngleAxis(90, Vector3.up) * dirForward;
        movement=(dirForward*vert + dirRight*hori).normalized*playerSpeed*speed;
        rigid.MovePosition(transform.position+movement);
    }
    public virtual void FreeCamMove(){
        if(freeCamMode) {
            dirForward=mainCam.transform.forward;
            dirRight=Quaternion.AngleAxis(90, mainCam.transform.up) * dirForward;
            movement=(dirForward*vert + dirRight*hori).normalized*10*Time.deltaTime;
            mainCam.transform.position+=movement;
            return;
        }
    }
    public void ChangeMoveVector(float Hori, float Vert){
        hori=Hori;
        vert=Vert;
    }
    void Turn(){
        if(GameManager.Instance.pause) return;
        if(freeCamMode) return;
        if(!canMove) return;

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

    public virtual void Jump(){
        if(GameManager.Instance.pause) return;
        if(!canMove) return;
        if(jump) return;
        jump=true;
        rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }
    public virtual bool Attack(){
        if(isAttack) return false;
        isAttack=true;
        cor=AttackDelay();
        StartCoroutine(cor);
        return isAttack;
    }
    public virtual IEnumerator AttackDelay(){

        yield return attackDelay;
        isAttack=false;
    }
    void CheckFall(){
        if(transform.position.y<fallingPoint) transform.position=startPoint;
    }
///////////////////////////////////////
    public void SetPlayerNumber(int num){
        if(!photonView.IsMine) return;
        playerNumber=num;
    }

    public void SetPlayerName(string Name){
        if(!photonView.IsMine) return;
        playerName=Name;
    }

    public void GameSettingForPlayer(Vector3 pos, float FallingPoint=-5f){
        if(!photonView.IsMine) return;
        this.fallingPoint=FallingPoint;
        startPoint=pos;

    }
    public virtual void StopMove(bool StopMove){
        if(!photonView.IsMine) return;
        canMove=!StopMove;
    }
    public void SetPlayerPosition(Vector3 pos){
        if(!photonView.IsMine) return;
        transform.position=pos;
        playerCamera.SetPlayerPosition();
    }
    public void SetPlayerSpeed(float Speed, float JumpPower=0){
        if(!photonView.IsMine) return;
        this.speed=Speed;
        if(JumpPower!=0) this.jumpPower=JumpPower;
    }

    private void OnTriggerEnter(Collider other) {
        if(!photonView.IsMine) return;
        if(jump){
                jump=false;
            }
        //if(other.gameObject.CompareTag("Ground")){
            
        //}
    }

    public virtual void SetThirdPersonView(bool isThird){
        if(!photonView.IsMine) return;
        isThirdPerson=isThird;
        playerCamera.SetThirdPersonView(isThirdPerson);
    }
    public virtual void SetFreeCamMode(bool isFreeCamMode){
        if(!photonView.IsMine) return;
        rigid.isKinematic=isFreeCamMode;
        freeCamMode=isFreeCamMode;
        playerCamera.SetFreeCamMode(freeCamMode);
    }

}
