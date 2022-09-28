using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
[System.Serializable]
public class StuffStruct{
    public GameObject spawnPointsObject;
    public string path="HideAndSeek/Objects/";
    Transform[] spawnPoints;
    public GameObject[] prefabs;
    string[] prefabNames;
    public bool isAllStuffSpawn{get; private set;}
    int posCount, prefabCount;
    public StuffStruct(){
    }
    public void Init(){
        if(!PhotonNetwork.IsMasterClient) return;
        spawnPoints=spawnPointsObject.GetComponentsInChildren<Transform>();
        posCount=spawnPoints.Length;
        prefabCount=prefabs.Length;
        prefabNames= new string[prefabCount];
        for(int i=0;i<prefabCount;i++){
            prefabNames[i]=prefabs[i].name;
        }
    }
    public void RandomSpawn(){
        if(!PhotonNetwork.IsMasterClient) return;
        if(isAllStuffSpawn) return;
        
        foreach(Transform trans in spawnPoints){
            if(Random.Range(0,10)>4) continue;
            int idx=Random.Range(0,prefabCount);
            PhotonNetwork.InstantiateRoomObject(path+"/"+prefabNames[idx], trans.position+Vector3.up*2, Quaternion.identity);
        }
        isAllStuffSpawn=true;
    }
}
public class StuffManager : MonoBehaviourPun
{
    [SerializeField]StuffStruct[] stuffsStructs;
    //====game var==
    public bool isAllStuffSpawn{get; private set;}
    string[] prefabNames;

    private void Awake() {
        
        InitGame();
    }
    void InitGame(){
        for(int i=0;i<stuffsStructs.Length;i++){
            stuffsStructs[i].prefabs=Resources.LoadAll<GameObject>(stuffsStructs[i].path);
            stuffsStructs[i].Init();
        }
    }
    private void Start() {
        for(int i=0;i<stuffsStructs.Length;i++){
            
            stuffsStructs[i].RandomSpawn();
            
        }
    }
    public bool CheckAllStuffSpawn(){
        for(int i=0;i<stuffsStructs.Length;i++){
            if(!stuffsStructs[i].isAllStuffSpawn) {
                isAllStuffSpawn=false;
                return isAllStuffSpawn;
            }
        }
        isAllStuffSpawn=true;
        return isAllStuffSpawn;
    }
}