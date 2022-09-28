using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class HideStuff : MonoBehaviourPun{
    //synchronize
    PhotonTransformView transformSync;
    Vector3 pos,currentPos;
    bool isFixed;
    IEnumerator cor;
    Rigidbody rigid;
    Player player;
    public bool isHuman{get; private set;}
    bool isGameStart;
    WaitForSeconds delay;
    private void Awake() {
        delay=new WaitForSeconds(0.1f);
        transformSync=GetComponent<PhotonTransformView>();
    }

    private void Start() {
        if(!photonView.IsMine) return;
        if(!isHuman) StuffPos();
    }
    private void OnEnable() {
        if(!photonView.IsMine) return;
        if(isHuman) player.enabled=true;
        photonView.RPC("HideStuffEnable",RpcTarget.Others,true);
    }
    private void OnDisable() {
        if(!photonView.IsMine) return;
        if(isHuman) player.enabled=false;
        photonView.RPC("HideStuffEnable",RpcTarget.Others,false);
    }
    
    void StuffPos(){
        if(!photonView.IsMine) return;
        if(isFixed) return;
        if(isHuman) return;
        isFixed=false;
        int yRotation=Random.Range(-180,180);
        Vector3 rot=transform.eulerAngles;
        rot.y=yRotation;
        transform.eulerAngles=rot;
        if(!isFixed){
            cor=DownPos();
            StartCoroutine(cor);
        }
    }
    public void SetHuman(){
        if(!photonView.IsMine) return;
        isHuman=true;
        photonView.RPC("HiderStuffSetHuman", RpcTarget.Others);
        rigid=GetComponent<Rigidbody>();
    
        if(player!=null) player.enabled=true;
        else player=gameObject.AddComponent<Player>();
        rigid.isKinematic=false;
        rigid.constraints=RigidbodyConstraints.FreezeRotation;
        rigid.constraints=~RigidbodyConstraints.FreezePosition;
        rigid.useGravity=true;
        rigid.mass=40f;
        //==sync
        
    }
    [PunRPC]
    void HiderStuffSetHuman(){
        isHuman=true;
        if(HideAndSeek.Instance.isSeeker) gameObject.SetActive(false);
    }
    IEnumerator DownPos(){
        while(!isFixed){
            if(isHuman) break;;
            pos=transform.position;
            pos.y-=0.1f;
            transform.position=pos;
            yield return delay;
        }
    }
    public void GameStart(){
        if(isGameStart) return;
        if(!isHuman) return;
        if(!photonView.IsMine) return;
        isGameStart=true;
        photonView.RPC("HasCanSeeStuff",RpcTarget.Others);
    }
    [PunRPC]
    void HasCanSeeStuff(){
        if(photonView.IsMine) return;
        isGameStart=true;
        Debug.Log("1");
        if(HideAndSeek.Instance.isSeeker) {
            Debug.Log("2");
            gameObject.SetActive(true);
        }
        //transform.position=currentPos;
    }
    private void OnTriggerEnter(Collider other) {
        if(!photonView.IsMine) return;
        if(isHuman) return;
        if(isFixed) return;
        //|| other.gameObject.CompareTag("Stuff")
        if(other.gameObject.CompareTag("Ground"))  {
            isFixed=true;
            photonView.RPC("StuffFixed", RpcTarget.Others, transform.position, transform.eulerAngles);
        }
    }
    [PunRPC]
    void StuffFixed(Vector3 pos, Vector3 rot){
        transform.position=pos;
        transform.eulerAngles=rot;
        isFixed=true;
    }
    public void Catched(){
        if(!isHuman) return;

        photonView.RPC("HiderCatched",RpcTarget.AllViaServer,PhotonNetwork.NickName);
        
    }
    [PunRPC]
    void HiderCatched(string SeekerName){
        if(photonView.IsMine){
            if(HideAndSeek.Instance.isCatched) return;
            
            HideAndSeek.Instance.Catched(SeekerName);
            //gameObject.SetActive(false);
            //photonView.RPC("HideStuffEnable",RpcTarget.AllViaServer,false);
        }
    }
    [PunRPC]
    void HideStuffEnable(bool enable){
        if(!isHuman) return;
        if(HideAndSeek.Instance.isSeeker) return;
        gameObject.SetActive(enable);
    }
}