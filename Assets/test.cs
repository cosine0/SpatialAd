using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class test : MonoBehaviour
{
    public GameObject buttonObject;

    void Start()
    {
        buttonObject.GetComponent<Button>().onClick.AddListener(onClickMap);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onClickMap()
    {

        Debug.Log(EventSystem.current.currentSelectedGameObject.name);
    }
}