using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ranking : MonoBehaviour{

    //Player[] topRankPlayer;     //3Players of Top Rank
    public string[] topRankPlayer{get; private set;}
    [SerializeField] Text[] textOfRankObject;

    int maxRanker=3;

    void Awake(){
        //topRankPlayer=new Player[maxRanker];
        topRankPlayer=new string[maxRanker];
        //textOfRankObject=new Text[maxRanker];

        for(int i=0;i<maxRanker;i++){
            topRankPlayer[i]="";
        }
    }
    void Start(){
        if(textOfRankObject==null) Debug.LogError("textOfRankObject is null");
        InitRanking();
    }
    public int CheckRanking(int PlayerNumber){
        /*
        for(int i=0;i<maxRanker;i++)
        {
            if(topRankPlayer[i]==null){
                return i;
            }
        }
        return 0;
        */
        for(int i=0;i<maxRanker;i++)
        {
            if(topRankPlayer[i]==""){
                return i;
            }
        }
        return 0;
    }

    public void UpdateRanking(string PlayerName, int rank=0){

        if(rank>maxRanker) return;
        if(rank!=0) {
            if(topRankPlayer[rank-1]!="" && rank<maxRanker){
                topRankPlayer[rank]=topRankPlayer[rank-1];
            }
            topRankPlayer[rank-1]=PlayerName;
        }
        for(int i=0;i<maxRanker;i++)
        {
            if(topRankPlayer[i]==PlayerName) return;
            if(topRankPlayer[i]==""){
                topRankPlayer[i]=PlayerName;
                break;

            }
        }
        Debug.Log( topRankPlayer[0]+","+topRankPlayer[1]+","+topRankPlayer[2]);
    }

    public void SetRanking(string[] RankingList){
        for(int i=0;i<RankingList.Length;i++){
            if(i>=maxRanker) return;
            topRankPlayer[i]=RankingList[i];
            textOfRankObject[i].text=(i+1)+"등: "+topRankPlayer[i];
        }
    }

    void InitRanking(){
        for(int i=0;i<maxRanker;i++){
            topRankPlayer[i]="";
            textOfRankObject[i].text=(i+1)+"등";
        }
    }

    
}