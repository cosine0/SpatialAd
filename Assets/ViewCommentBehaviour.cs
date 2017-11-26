using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class viewCommentBehaviour : MonoBehaviour
{
    public string opjName;
    public string path;
    public GameObject ScrollViewGameObject;
    private UserInfo _userInfo;


    // Use this for initialization
    void Start()
    {
        
        //Cards is an array of data
        opjName = "optionComment";
        path = "Prefabs/" + opjName;
        
        for (int i = 0; i < 3; i++)
        {
            //ItemGameObject is my prefab pointer that i previous made a public property  
            //and  assigned a prefab to it
            GameObject card = Instantiate(Resources.Load(path)) as GameObject;

            //scroll = GameObject.Find("CardScroll");
            if (ScrollViewGameObject != null)
            {
                //ScrollViewGameObject container object
                card.transform.SetParent(ScrollViewGameObject.transform, false);
            }
        }
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

        using (UnityWebRequest www = UnityWebRequest.Post("http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/get_user_comment.php", checkCommentForm))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                Debug.Log(www.error);
            else
            {
                fromServJson = www.downloadHandler.text;
                
            }
        }
    }



}
