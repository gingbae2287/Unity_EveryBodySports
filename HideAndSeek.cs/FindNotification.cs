using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FindNotification : MonoBehaviour {
    [SerializeField] Text seekerName;
    [SerializeField] Text hiderName;
    Image[] images;
    WaitForSeconds delay, fadeDelay;
    Color currentColor;
    IEnumerator cor;

    private void Awake() {
        delay=new WaitForSeconds(5f);
        fadeDelay=new WaitForSeconds(0.1f);
        images=GetComponentsInChildren<Image>();
    }
    private void OnEnable() { 

        foreach(Image image in images){
            currentColor=image.color;
            currentColor.a=0.8f;
            image.color=currentColor;
        }
        currentColor=seekerName.color;
        currentColor.a=0.8f;
        seekerName.color=currentColor;

        currentColor=hiderName.color;
        currentColor.a=0.8f;
        hiderName.color=currentColor;
        cor=FadeTimer();
        StartCoroutine(cor);
    }
    public void SetName(string SeekerName, string HiderName){
        seekerName.text=SeekerName;
        hiderName.text=HiderName;
    }

    IEnumerator FadeTimer(){
        yield return delay;

        while(currentColor.a>0){
    
            foreach(Image image in images){
                currentColor=image.color;
                currentColor.a-=0.1f;
                image.color=currentColor;
            }
            currentColor=seekerName.color;
            currentColor.a-=0.1f;
            seekerName.color=currentColor;

            currentColor=hiderName.color;
            currentColor.a-=0.1f;
            hiderName.color=currentColor;
            yield return fadeDelay;
        }
        gameObject.SetActive(false);
    }
}