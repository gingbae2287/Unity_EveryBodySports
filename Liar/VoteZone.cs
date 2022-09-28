using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class VoteZone : MonoBehaviourPun {

    [SerializeField] GameObject numberObjParent;
    public int playerNumberToVote{get; private set;}
    bool[] isPlayerInVoteZone;
    public string numberObjPath{get; private set;}
    GameObject numberObj;
    IEnumerator cor;
    void Awake(){
        isPlayerInVoteZone= new bool[GameManager.Instance.maxPlayerOfLiarGame];
        
    }

    private void OnEnable() {
        object[] data=photonView.InstantiationData;
        playerNumberToVote=(int)data[0];
       
        if(numberObj==null){
            numberObjPath="Liar/"+(playerNumberToVote+1).ToString();
            numberObj= Instantiate(Resources.Load<GameObject>(numberObjPath), numberObjParent.transform.position, Quaternion.identity);
            //numberObj.transform.position=numberObjParent.transform.position;
        }
        else numberObj.SetActive(true);
        if(PhotonNetwork.IsMasterClient)  
            photonView.RPC("PunSetActive", RpcTarget.OthersBuffered,true);
    }
    private void OnDisable() {
        
        numberObj.SetActive(false);
        if(PhotonNetwork.IsMasterClient)
            photonView.RPC("PunSetActive", RpcTarget.OthersBuffered,false);
    }
    [PunRPC]
    void PunSetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    void Update(){
            numberObj.transform.Rotate(Vector3.up,Time.deltaTime*30);
        
    }
    private void OnTriggerEnter(Collider other) {
        if(!(other.gameObject.tag=="Player")) return;
        if(LiarGame.Instance.isVote==true) return;
        if(!other.gameObject.GetComponent<Player>().photonView.IsMine) return;
        LiarGame.Instance.isVote=true;
        photonView.RPC("VoteRPC", RpcTarget.AllBufferedViaServer, LiarGame.Instance.playerNumber, playerNumberToVote);
    }
    private void OnTriggerExit(Collider other) {
        if(!(other.gameObject.tag=="Player")) return;
        if(!other.gameObject.GetComponent<Player>().photonView.IsMine) return;
        if(LiarGame.Instance.isVote==false) return;
        LiarGame.Instance.isVote=false;
        photonView.RPC("VoteRPC", RpcTarget.AllBufferedViaServer, LiarGame.Instance.playerNumber, -1);
    }

    [PunRPC]
    //RPC for Master
    void VoteRPC(int PlayerNumber, int VoteNumber){
        LiarGame.Instance.VotePlayer(PlayerNumber,VoteNumber);

    }
    public void SetVoteNumber(int num){
        playerNumberToVote=num;
    }
}
