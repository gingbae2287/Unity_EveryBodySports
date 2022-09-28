using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HistoryBox : MonoBehaviour {
    [SerializeField] GameObject firstExplanationText;
    [SerializeField] Transform scrollViewParent;
    [SerializeField] GameObject scrollView;

    Vector3 objectPos;
    GameObject[] explanationText;
    string[] explanation;

    private void Awake() {
        explanationText=new GameObject[GameManager.Instance.maxPlayerOfLiarGame];
        explanation=new string[GameManager.Instance.maxPlayerOfLiarGame];
        InitHistoryBox();
    }

    void InitHistoryBox(){
        scrollView.SetActive(true);
        explanationText[0]=firstExplanationText;
        objectPos=firstExplanationText.transform.position;
        explanationText[0].SetActive(false);
        for(int i=1;i<GameManager.Instance.maxPlayerOfLiarGame;i++){
            explanation[i]="";
            explanationText[i]=Instantiate(firstExplanationText,objectPos,Quaternion.identity);
            explanationText[i].transform.SetParent(scrollViewParent,false);
            explanationText[i].SetActive(false);
        }
        scrollView.SetActive(false);
    }

    void start(){
        if(firstExplanationText==null) Debug.LogError("explanationText is null");
        if(scrollViewParent==null) Debug.LogError("scrollViewParent is null");
        if(scrollView==null) Debug.LogError("scrollView is null");
    }
    public void HistoryBoxButton(){
        scrollView.SetActive(!scrollView.activeSelf);
        if(scrollView.activeSelf) BoxOn();
    }

    public void SetExplanation(int PlayerNumber, string Explanation){
        string ex=(PlayerNumber+1)+". "+Explanation;
        for(int i=0;i<explanation.Length;i++){
            if(explanation[i]==null||explanation[i]=="") {
                explanation[i]=(PlayerNumber+1)+". "+Explanation;
                break;
            }
        }
        if(scrollView.activeSelf) BoxOn();
        
    }

    void BoxOn(){
        for(int i=0;i<explanationText.Length;i++){
            if((!explanationText[i].activeSelf)&&(explanation[i]!=null&&explanation[i]!="")) {
                explanationText[i].SetActive(true);
                explanationText[i].GetComponent<Text>().text=explanation[i];
                //use layour group
            }
        }
    }
    public void ClearHistoryBox(){
        for(int i=0;i<explanation.Length;i++){
            explanation[i]="";
            explanationText[i].SetActive(false);
        }
    }
}