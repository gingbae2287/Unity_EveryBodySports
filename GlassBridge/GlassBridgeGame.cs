using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GlassBridgeGame : MonoBehaviourPun
{
    private static GlassBridgeGame instance;
    public static GlassBridgeGame Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    float offsetX=3f;
    float offsetz=3f;
    int lineCount=10;
    [Header("Glass Start Point")]
    [SerializeField] float startX=-1.5f;
    [SerializeField] float startY=0.4f;
    [SerializeField] float startZ=4f;
    
    [SerializeField]GameObject glassObject;
    [SerializeField] GameObject glassObjectNonColl;
    [SerializeField] Transform goalZoneParent;
    [SerializeField] GameObject goalZoneObject;

    [Header("TestValue (default = 0.5)")] 
    [SerializeField] float GlassProperty=0.5f;
    Glass[,] glassScript;

    //player관련
    int maxPlayer=12;
    int playerNumber;
    float FallingPoint=-5;
    Vector3 startPoint;
    [SerializeField] Ranking ranking;

    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }
        maxPlayer=GameManager.Instance.maxPlayerOfGlassBridge;
        if(!PhotonNetwork.IsMasterClient) return;
        glassScript= new Glass[2,lineCount];
        //CreateGlasses();
        
    }
    

    void Start()
    {
        if(PhotonNetwork.IsMasterClient){
            CreateGlasses();
        }
        if(Player.LocalPlayerInstance==null) {
            
            SpawnPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SpawnPlayer(){
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++){
            if(PhotonNetwork.LocalPlayer==PhotonNetwork.PlayerList[i]){
                playerNumber=i;
                break;
            }
        }
        startPoint=new Vector3(maxPlayer*0.5f-playerNumber, 0, 0);
        
        playerNumber=PhotonNetwork.LocalPlayer.ActorNumber;
    
       PhotonNetwork.Instantiate("Character/TT_male", startPoint,Quaternion.identity);
       Player.LocalPlayerInstance.GameSettingForPlayer(startPoint,FallingPoint);
    }

    void CreateGlasses(){
        GameObject goalZone= PhotonNetwork.Instantiate(goalZoneObject.name,goalZoneParent.transform.position,Quaternion.identity);
        goalZone.transform.SetParent(goalZoneParent);
        for(int i=0;i<lineCount;i++){
            int tmp=Random.Range(0,1);
            GameObject obj=PhotonNetwork.Instantiate(glassObject.name, new Vector3(startX+offsetX*tmp,startY,startZ+offsetz*i),Quaternion.identity);
            GameObject obj2=PhotonNetwork.Instantiate(glassObjectNonColl.name, new Vector3(startX+offsetX*(1-tmp),startY,startZ+offsetz*i),Quaternion.identity);
            
            obj.transform.SetParent(transform);
            obj2.transform.SetParent(transform);
           
            
            /*
            glassScript[0,i]=obj.GetComponent<Glass>();
            glassScript[1,i]=obj2.GetComponent<Glass>();
            bool tmp=(Random.value > GlassProperty);
            glassScript[0,i].SetHard(tmp);
            glassScript[1,i].SetHard(!tmp);
            */

        }
    }

    //-----게임중----

    public void GoalInPlayer(string PlayerName){
        if(!PhotonNetwork.IsMasterClient) return;
        ranking.UpdateRanking(PlayerName);
        photonView.RPC("GoalInRpc", RpcTarget.AllViaServer, ranking.topRankPlayer);
    }
    [PunRPC]
    void GoalInRpc(string[] RankingList){
        ranking.SetRanking(RankingList);
    }
}
