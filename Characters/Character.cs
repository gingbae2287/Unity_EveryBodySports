using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Character : Player
{
    [SerializeField] GameObject[] hideObj;
    Animator anim;

    protected override void Awake() {
        base.Awake();
        anim=GetComponent<Animator>();
    }

    protected override void Run(){
        if(GameManager.Instance.pause) return;
        if(!canMove){
            runState=false;
            anim.SetInteger("RunState", 0);
            return;
        }
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
        anim.SetInteger("RunState", 2);

        if(isThirdPerson) dirForward=transform.position-mainCam.transform.position;
        //first Person
        else   dirForward=mainCam.transform.forward;

        dirForward.y=0;
        dirRight=Quaternion.AngleAxis(90, Vector3.up) * dirForward;
        movement=(dirForward*vert + dirRight*hori).normalized*speed*playerSpeed;
        rigid.MovePosition(transform.position+movement);
    }

    public override void Jump(){
        if(GameManager.Instance.pause) return;
        if(!canMove) return;
        if(jump) return;
            jump=true;
            anim.SetBool("Jump", true);
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }

    public override bool Attack(){
        if(isAttack) return false;
        isAttack=true;
        anim.SetFloat("AttackSpeed",this.attackSpeed);
        anim.SetTrigger("Attack");
        cor=AttackDelay();
        StartCoroutine(cor);
        return isAttack;
    }

    public override void StopMove(bool StopMove){
        base.StopMove(StopMove);
        anim.SetInteger("RunState", 0);
    }
    public override void SetThirdPersonView(bool isThird){
        base.SetThirdPersonView(isThird);
        foreach(GameObject obj in hideObj) obj.SetActive(isThirdPerson);
    }

    private void OnTriggerEnter(Collider other) {
        if(!photonView.IsMine) return;
        //if(other.gameObject.CompareTag("Ground")){
            
        //}
        if(jump){
                jump=false;
                anim.SetBool("Jump", false);
        }
    }

}