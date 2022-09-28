using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiarUI : MonoBehaviour{
    [SerializeField] Text announcement;
    [SerializeField] Text title;
    [SerializeField] Text category;
    [SerializeField] Text myOrder;
    [SerializeField] HistoryBox historyBox;
    [SerializeField] ExplanationBox explanationBox;

    void start(){
        NullCheck();
        InitUI();
        explanationBox.gameObject.SetActive(false);
    }
    void InitUI(){
        announcement.text="";
        title.text="";
        category.text="";
    }
    void NullCheck(){
        if(announcement==null) Debug.LogError("announcement is null");
        if(title==null) Debug.LogError("title is null");
        if(category==null) Debug.LogError("category is null");
        if(historyBox==null) Debug.LogError("historyBox is null");
        if(explanationBox==null) Debug.LogError("explanationBox is null");
        if(myOrder==null) Debug.LogError("myOrder is null");
    }
    public void SetMyOrder(int order){
        myOrder.text="My Number: "+order;
    }

    public void SetAnnouncement(string str){
        announcement.text=str;
    }
    public void ClearAnnouncment(){
        announcement.text="";
    }

    public void SetTitleText(string Category, string Title, bool IsLiar=false){
        category.text="카테고리: "+Category;
        if(IsLiar) title.text="당신은 라이어 입니다.";
        else title.text="주제: "+Title;
    }
    public void AddHistory(int PlayerNum, string History){
        historyBox.SetExplanation(PlayerNum, History);
    }

    public void PlayerTurn(string GuideText){
        explanationBox.gameObject.SetActive(true);
        explanationBox.SetGuideText(GuideText);
    }
    public string GetExplanation(){
        string str=explanationBox.GetExplanation();
        explanationBox.gameObject.SetActive(false);
        return str;
    }
    public void ClearHistoryBox(){
        historyBox.ClearHistoryBox();
    }


    public void LobbyButton(){
        GameManager.Instance.LeftRoom();
    }
}