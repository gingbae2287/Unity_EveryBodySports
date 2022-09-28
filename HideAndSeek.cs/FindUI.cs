using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FindUI : MonoBehaviour {
    [SerializeField] GameObject findObjPrefab;
    GameObject[] findObjs;

    private void Awake() {
        findObjs=new GameObject[6];
        for(int i=0;i<findObjs.Length;i++){
            findObjs[i]=Instantiate(findObjPrefab,Vector3.zero,Quaternion.identity);
            findObjs[i].transform.SetParent(transform);
            findObjs[i].SetActive(false);
        }
    }
    public void FindUIOn(string seekerName, string hiderName){
        foreach(GameObject obj in findObjs){
            if(!obj.activeSelf){
                obj.SetActive(true);
                obj.GetComponent<FindNotification>().SetName(seekerName,hiderName);
                break;
            }
        }
    }


}