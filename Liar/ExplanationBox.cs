using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplanationBox : MonoBehaviour{
    [SerializeField] Text guideText;
    [SerializeField] InputField explanation;

    private void OnEnable() {
        Player.LocalPlayerInstance.StopMove(true);
    }
    private void OnDisable() {
        Player.LocalPlayerInstance.StopMove(false);
    }
    private void Start() {
        NullCheck();
    }
    void NullCheck(){
        if(guideText==null) Debug.LogError("guideText is null");
        if(explanation==null) Debug.LogError("explanation is null");
    }

    public void SetGuideText(string str){
        guideText.text=str;
    }
    public string GetExplanation(){
        string str=explanation.text;
        explanation.text="";
        return str;
    }
}