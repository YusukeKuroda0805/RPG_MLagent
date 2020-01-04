
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class goodfadeout : MonoBehaviour
{
    public GameObject panel;
    //public AudioClip SE;
    //AudioSource audioS;

    void Start()
    {
        panel = this.gameObject;
        //audioS = GetComponent<AudioSource>();
    }

    void Update()
    {
        panel.transform.DOScale(new Vector3(13, 13, 13), 0.1f);
        //audioS.PlayOneShot(SE);
        Invoke("destroy", 1.0f);
    }

    public void destroy()
    {
        Destroy(this.gameObject);
    }
}
