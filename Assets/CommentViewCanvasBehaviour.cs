using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CommentViewCanvasBehaviour : MonoBehaviour {
    public int adNumber = -1;
    public GameObject inAppCanvas;
    public GameObject commentText;

    public void OnInit()
    {
        StaticCoroutine.DoCoroutine(CreateCommentCanvas());
        inAppCanvas.SetActive(false);
        this.gameObject.SetActive(true);
    }

    private IEnumerator CreateCommentCanvas()
    {
        WWWForm form = new WWWForm();
        form.AddField("adNumber", adNumber);

        using (UnityWebRequest www = UnityWebRequest.Post("http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/get_comment.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // - get comment list
                string responseJsonString = www.downloadHandler.text;
                JsonCommentDataArray commentList = JsonUtility.FromJson<JsonCommentDataArray>(responseJsonString);
                
                if (commentList.data.Length == 0)
                {
                    GameObject commentPanel = MonoBehaviour.Instantiate(Resources.Load("Prefabs/CommentViewPrefab") as GameObject) as GameObject;
                    // 부모 자식 관계 생성
                    commentPanel.transform.SetParent(this.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0), false); // parent properties inheritance false

                    commentPanel.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "No comment yet";
                    commentPanel.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "";
                }
                else
                {
                    for (int i = 0; i < commentList.data.Length; i++)
                    {
                        // comment 정보 채우기
                        // comment panel instantiate
                        GameObject commentPanel = MonoBehaviour.Instantiate(Resources.Load("Prefabs/CommentViewPrefab") as GameObject) as GameObject;
                        // 부모 자식 관계 생성
                        commentPanel.transform.SetParent(this.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0), false); // parent properties inheritance false
                                                                                                                 // comment panel object transform에서 Text Component Child 2개(id, comment)를 찾아 텍스트 수정.
                        commentPanel.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = commentList.data[i].userId;
                        commentPanel.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = commentList.data[i].comment;
                    }

                    // scroll view area calulate
                    if (commentList.data.Length > 7)
                    {
                        Rect _groupRect = this.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<Rect>();
                        _groupRect.yMax -= (commentList.data.Length - 7) * 250;
                    }
                }
            }
        }
        this.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Scrollbar>().value = 0; // 작동안됨
    }

    public void LeaveComment()
    {
        StartCoroutine(CommitComment());
    }

    private IEnumerator CommitComment()
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", GameObject.FindGameObjectWithTag("UserInfo").GetComponent<UserInfo>().UserId);
        form.AddField("adNumber", adNumber);
        form.AddField("comment", commentText.GetComponent<InputField>().text);

        using (UnityWebRequest www = UnityWebRequest.Post("http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/add_comment.php", form))
        {
            yield return www.SendWebRequest();

            // text field clear
            commentText.GetComponent<InputField>().text = "";
            // update comment list
            Transform _group = this.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0);
            foreach (Transform child in _group)
            {
                GameObject.Destroy(child.gameObject);
            }
            StaticCoroutine.DoCoroutine(CreateCommentCanvas());
        }
    }

    public void BackToInApp()
    {
        // - list clear
        Transform _group = this.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0);
        foreach (Transform child in _group)
        {
            GameObject.Destroy(child.gameObject);
        }
        adNumber = -1;
        // - canvas inactivate
        this.gameObject.SetActive(false);
        inAppCanvas.SetActive(true);
    }
}
