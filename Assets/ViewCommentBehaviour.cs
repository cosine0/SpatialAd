using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ViewCommentBehaviour : MonoBehaviour
{
    public string opjName;
    public string path;
    public GameObject ScrollViewGameObject;
    private UserInfo _userInfo;
    
    JsonOptionCommentDataArray optionCommentList;

    public RawImage mapImage;

    // Use this for initialization
    void Start()
    {
        _userInfo = GameObject.FindGameObjectWithTag("UserInfo").GetComponent<UserInfo>();

        //Cards is an array of data
        opjName = "optionComment";
        path = "Prefabs/" + opjName;

        StartCoroutine(GetOptionCommentCoroutine());
        StaticGoogleMap.ApplyMapTexture(mapImage, 37.450700f, 126.657100f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToOptionScene()
    {
        SceneManager.LoadScene("Option");
    }

    private IEnumerator GetOptionCommentCoroutine()
    {
        string userID = _userInfo.UserId;

        string fromServJson;
        WWWForm checkCommentForm = new WWWForm();
        checkCommentForm.AddField("Input_user", userID);

        using (UnityWebRequest www = UnityWebRequest.Post("http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/user_comment.php", checkCommentForm))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                Debug.Log(www.error);
            else
            {
                fromServJson = www.downloadHandler.text;
                optionCommentList = JsonUtility.FromJson<JsonOptionCommentDataArray>(fromServJson);

                for (int i = 0; i < optionCommentList.data.Length; i++) {
                    GameObject optionCommentPanel = Instantiate(Resources.Load(path) as GameObject) as GameObject;
                    optionCommentPanel.transform.SetParent(ScrollViewGameObject.transform, false);
                    optionCommentPanel.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = optionCommentList.data[i].dateTime;
                    optionCommentPanel.transform.GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = optionCommentList.data[i].comment;
                    optionCommentPanel.transform.name = "comment" + i.ToString();
                    optionCommentPanel.transform.GetChild(0).GetChild(1).name=i.ToString();
                }

                RectTransform _groupRect = ScrollViewGameObject.transform.GetComponent<RectTransform>();

                _groupRect.sizeDelta = new Vector2(1000, optionCommentList.data.Length * 240);
                


                //// comment 생성
                //for (int i = 0; i < 3; i++)
                //{
                //    //ItemGameObject is my prefab pointer that i previous made a public property  
                //    //and  assigned a prefab to it
                //    GameObject card = Instantiate(Resources.Load(path)) as GameObject;

                //    //scroll = GameObject.Find("CardScroll");
                //    if (ScrollViewGameObject != null)
                //    {
                //        //ScrollViewGameObject container object
                //        card.transform.SetParent(ScrollViewGameObject.transform, false);
                //    }
                //}


            }
        }
    }

    private void onClickMapBtn() {
        int numN = Convert.ToInt32(EventSystem.current.currentSelectedGameObject.name);
        
        StaticGoogleMap.ApplyMapTexture(mapImage, float.Parse(optionCommentList.data[numN].latitude), float.Parse(optionCommentList.data[numN].latitude));
    }

    private IEnumerator DeleteOptionCommentCoroutine()
    {
        //string userID = _userInfo.UserId;

        string comment = EventSystem.current.currentSelectedGameObject.name;

        WWWForm deleteCommentForm = new WWWForm();
        deleteCommentForm.AddField("comment", comment);

        using (UnityWebRequest www = UnityWebRequest.Post("http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/delete_comment.php", deleteCommentForm))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                Debug.Log(www.error);
            else
            {
                Debug.Log("delete");

            }
        }

        int numN = Convert.ToInt32(EventSystem.current.currentSelectedGameObject.name);

        Destroy(GameObject.Find("comment" + EventSystem.current.currentSelectedGameObject.name));
        //StartCoroutine(GetOptionCommentCoroutine());
        // 지도 없애기

    }
}
