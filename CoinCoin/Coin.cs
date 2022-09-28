using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Coin : MonoBehaviourPun
{
    bool isActive;
    public int mapIndex{get; private set;}
    float rotateSpeed=50f;
    public int score=1;
    Vector3 pos;
    private void Start() {
        transform.rotation=Quaternion.Euler(90,0,0);
    }
    void Update(){
        //회전
        transform.Rotate(new Vector3(0,0,rotateSpeed*Time.deltaTime));
    }
    private void OnEnable() {
        if(PhotonNetwork.IsMasterClient){
            score=Random.Range(0,10);
            if(score<6) score=1;
            else if(score<9) score=2;
            else score=4;
            transform.localScale=new Vector3(0.6f+0.2f*score,0.05f,0.6f+0.2f*score);
            pos=transform.position;
            photonView.RPC("CoinActive", RpcTarget.Others,pos,mapIndex,score);
        }
        
    }
    private void OnDisable() {
        mapIndex=-1;
        //if(!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("CoinActiveOff", RpcTarget.Others);
    }
    public void ActiveCoin(int mapIdx){
        mapIndex=mapIdx;
    }
    private void OnTriggerEnter(Collider other) {
        if(!(other.gameObject.tag=="Player")) return;
        if(!other.gameObject.GetComponent<Player>().photonView.IsMine) return;
        isActive=false;
        CoinCoin.Instance.GetScore(score);
        photonView.RPC("GetCoin",RpcTarget.MasterClient,mapIndex);
        pos=new Vector3(5,50,5);
        gameObject.SetActive(false);
    }
    [PunRPC]
    void GetCoin(int mapIdx){
        if(!PhotonNetwork.IsMasterClient) return;
        CoinCoin.Instance.coinManager.GetCoin(mapIdx);
    }
    [PunRPC]
    void CoinActive(Vector3 pos, int MapIdx, int Score){
        gameObject.SetActive(true);
        mapIndex=MapIdx;
        transform.position=pos;
        score=Score;
        transform.localScale=new Vector3(0.6f+0.2f*score,0.05f,0.6f+0.2f*score);

    }
    [PunRPC]
    void CoinActiveOff()
    {   mapIndex=-1;
        gameObject.SetActive(false);
    }
}