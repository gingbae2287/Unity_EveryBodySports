using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour {
    [SerializeField] Image bgmBar;
    [SerializeField] Image soundBar;
    [SerializeField] GameObject logOutBtn;
    [SerializeField] GameObject leftGameBtn;
    void Start(){
        if(soundBar==null) Debug.LogError("null");
        if(bgmBar==null) Debug.LogError("null");
    }
    private void OnEnable() {
        leftGameBtn.SetActive(GameManager.Instance.isGaming);
        logOutBtn.SetActive(!GameManager.Instance.isGaming);
    }
    private void OnDisable() {
        GameManager.Instance.SettingUIOff();
    }

    public void ButtonClose(){
        gameObject.SetActive(false);
    }
    public void ButtonLeftGame(){
        GameManager.Instance.LeftRoom();
        gameObject.SetActive(false);
    }
    public void ButtonLogOut(){
        AuthManager.Instance.LogOut();
        gameObject.SetActive(false);
    }
    public void ShowLeaderBoard(){
        GooglePlayManager.Instance.ShowLeaderBoard();
    }
    public void BgmValueChange(){
        GameManager.Instance.ChangeBgmValue(bgmBar.fillAmount);
    }
    public void SoundValueChange(){
        GameManager.Instance.ChangeSoundValue(soundBar.fillAmount);
    }
}