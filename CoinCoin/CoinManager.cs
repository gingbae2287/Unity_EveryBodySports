using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class CoinManager : MonoBehaviourPunCallbacks
{   

    //===for master===
    public bool coinCreated{get; private set;}
    public bool isCoinStart{get; private set;}
    int maxCoins=35, blockSize=3;
    Vector3 coinPos=new Vector3(5,50,5);
    //bool[,] coinMap=new bool[10,10];
    
    string coinPath="CoinCoin/Coin";
    Coin[] coins;
    GameObject[] coinObjs;
    IEnumerator cor;
    List<int> coinMap=new List<int>(100);   
    //코인 위치 중복 생성 방지
    //각 요소는 10x10으로 나눈 맵의 index로 coin에 할당되면 list에서 빠지고 coin이 먹히면 해당 index를 다시 추가해주는 방식
    void Awake(){
        coins=new Coin[maxCoins];
        coinObjs=new GameObject[maxCoins];
        for(int i=0;i<100;i++){
            coinMap.Add(i);
        }
    }
    private void Start() {
        if(!PhotonNetwork.IsMasterClient) return;

    }
    public void CreateCoins(){
        if(!PhotonNetwork.IsMasterClient) return;
        if(coinCreated) return;
        
        for(int i=0;i<maxCoins;i++){
            coinObjs[i]=PhotonNetwork.InstantiateRoomObject(coinPath,coinPos,Quaternion.identity);
            coins[i]=coinObjs[i].GetComponent<Coin>();
            coinObjs[i].SetActive(false);
        }
        coinCreated=true;
    }
    public void CoinStart(){
        if(!PhotonNetwork.IsMasterClient) return;
        if(isCoinStart) return;
        isCoinStart=true;
        cor=RandomCoin();
        StartCoroutine(cor);
    }
    IEnumerator RandomCoin(){
        
        while(!CoinCoin.Instance.isGamePlaying) yield return new WaitForSeconds(0.1f);
        int idx=0;
        float y=0f;
        while(CoinCoin.Instance.isGamePlaying){
            for(int i=0;i<maxCoins;i++){
                if(!coins[i].gameObject.activeSelf){
                    idx=Random.Range(0,coinMap.Count);
                    y=Random.Range(0.5f,2f);
                    Vector3 pos=new Vector3(coinMap[idx]%10*blockSize,y,coinMap[idx]/10*blockSize);
                    coins[i].gameObject.transform.position=pos;
                    coins[i].gameObject.SetActive(true);
                    coins[i].ActiveCoin(coinMap[idx]);
                    coinMap.RemoveAt(idx);
                    yield return new WaitForSeconds(0.7f);
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
    public void GetCoin(int mapIdx){
        coinMap.Add(mapIdx);
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if(!PhotonNetwork.IsMasterClient) return;
        if((CoinCoin.GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"]==CoinCoin.GameState.Gaming){
            coins=FindObjectsOfType<Coin>();
            foreach(Coin coin in coins){
                //맵에 배치된 코인들의 mapindex를 가져와 coinmap에서 빼준다
                if(coin.gameObject.activeSelf) coinMap.Remove(coin.mapIndex);
            }
            cor=RandomCoin();
            StartCoroutine(cor);
        }
    }

}