using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ViewObjectBehaviour : MonoBehaviour
{
    public string opjName;
    public string path;
    public GameObject ScrollViewGameObject;
    private UserInfo _userInfo;

    JsonOptionObjectDataArray optionObjectList;
    public RawImage mapImage;

    // 안드로이드 Toast를 띄울 때 사용되는 임시 객체
    private string _toastString;
    private AndroidJavaObject _currentActivity;

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

        using (UnityWebRequest www = UnityWebRequest.Post("http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/user_3d_object.php", checkCommentForm))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                Debug.Log(www.error);
            else
            {
                fromServJson = www.downloadHandler.text;
                optionObjectList = JsonUtility.FromJson<JsonOptionObjectDataArray>(fromServJson);

                for (int i = 0; i < optionObjectList.data.Length; i++)
                {
                    GameObject optionObjectPanel = Instantiate(Resources.Load(path) as GameObject) as GameObject;
                    optionObjectPanel.transform.SetParent(ScrollViewGameObject.transform, false);
                    optionObjectPanel.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = optionObjectList.data[i].dateTime;
                    if(optionObjectList.data[i].typeName=="comment")
                        optionObjectPanel.transform.GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = optionObjectList.data[i].content;
                    else
                        optionObjectPanel.transform.GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "3D Object Type: " + optionObjectList.data[i].typeName;
                    optionObjectPanel.transform.name = "object" + i.ToString();
                    optionObjectPanel.transform.GetChild(0).GetChild(1).name = i.ToString();
                    optionObjectPanel.transform.GetChild(0).GetChild(2).name = i.ToString();
                    optionObjectPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(onClickMapBtn);
                    optionObjectPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(onClickDeleteBtn);
                }

                RectTransform _groupRect = ScrollViewGameObject.transform.GetComponent<RectTransform>();

                _groupRect.sizeDelta = new Vector2(1000, optionObjectList.data.Length * 240);



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



    public void onClickMapBtn()
    {
        int numN = Convert.ToInt32(EventSystem.current.currentSelectedGameObject.name);
        Debug.Log(numN);
        StaticGoogleMap.ApplyMapTexture(mapImage, float.Parse(optionObjectList.data[numN].latitude), float.Parse(optionObjectList.data[numN].longitude));
        Debug.Log(float.Parse(optionObjectList.data[numN].latitude) + " " + float.Parse(optionObjectList.data[numN].longitude));
    }

    public void onClickDeleteBtn()
    {
        //ShowToastOnUiThread("yo");
        int numN = Convert.ToInt32(EventSystem.current.currentSelectedGameObject.name);
        string objectN = optionObjectList.data[numN].object_no.ToString();
        Debug.Log("number:" + objectN);

        StartCoroutine(DeleteOptionObjectCoroutine(objectN));
    }

    private IEnumerator DeleteOptionObjectCoroutine(string objectN)
    {
        //string userID = _userInfo.UserId;

        //string comment = EventSystem.current.currentSelectedGameObject.name;


        WWWForm deleteCommentForm = new WWWForm();
        deleteCommentForm.AddField("objectN", objectN);

        using (UnityWebRequest www = UnityWebRequest.Post("http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/delete_3d_object.php", deleteCommentForm))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                Debug.Log(www.error);
            else
            {

                Debug.Log("delete");
                ShowToastOnUiThread("delete comment!");

            }
        }

        //int numN = Convert.ToInt32(EventSystem.current.currentSelectedGameObject.name);

        Destroy(GameObject.Find("object" + EventSystem.current.currentSelectedGameObject.name));

        //StartCoroutine(GetOptionCommentCoroutine());
        // 지도 없애기

    }

    void ShowToastOnUiThread(string toastString)
    {
        Debug.Log("Android Toast message: " + toastString);
        if (Application.platform != RuntimePlatform.Android)
            return;

        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        _currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        this._toastString = toastString;

        _currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(ShowToast));
    }

    void ShowToast()
    {
        Debug.Log("Running on UI thread");
        AndroidJavaObject context = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
        AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", _toastString);
        AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_SHORT"));
        toast.Call("show");
    }
}
