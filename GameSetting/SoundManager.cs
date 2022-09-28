using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    //==Componenet
    AudioSource sound;
    private static SoundManager instance;
    public static SoundManager Instance{
        get{
            if(instance==null) return null;
            return instance;
        }
    }
    public AudioClip buttonSound;
    private void Awake() {
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
        sound=GetComponent<AudioSource>();
    }
    public void ButtonSound(){
        sound.clip=buttonSound;
        sound.Play();
    }
}