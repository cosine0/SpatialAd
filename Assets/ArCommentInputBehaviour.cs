using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

public class ArCommentInputBehaviour : MonoBehaviour {
    private UserInfo _userInfo;
    private ClientInfo _clientInfo;
    public InputField Comment;

    public ArCommentInputBehaviour(InputField comment)
    {
        Comment = comment;
    }

    private void Start()
    {
        _clientInfo = GameObject.FindGameObjectWithTag("ClientInfo").GetComponent<ClientInfo>();
        _userInfo = GameObject.FindGameObjectWithTag("UserInfo").GetComponent<UserInfo>();
    }

    public void ShowCommentInputCanvas()
    {
        this.gameObject.SetActive(true);
    }

    public void HideCommentInputCanvas()
    {
        this.gameObject.SetActive(false);
    }

    public void CancelInput()
    {
        Comment.text = "";
        HideCommentInputCanvas();
    }

    public void OnCommit()
    {
        //GameObject CommentObject = MonoBehaviour.Instantiate((Resources.Load("Prefabs/ArComment") as GameObject)) as GameObject;
        //CommentObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        //CommentObject.transform.eulerAngles = new Vector3(0.0f, _clientInfo.CurrentBearing, 0.0f);
        //CommentObject.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        if (Comment.text.Length > 40)
            Comment.text = Comment.text.Remove(40);

        StartCoroutine(CommitArComment());
    }

    private IEnumerator CommitArComment()
    {
        WWWForm form = new WWWForm();
        form.AddField("typeName", "comment");
        form.AddField("user", _userInfo.UserId);
        form.AddField("content", Comment.text);
        form.AddField("latitude", _clientInfo.CurrentLatitude.ToString());
        form.AddField("longitude", _clientInfo.CurrentLongitude.ToString());
        form.AddField("altitude", _clientInfo.CurrentAltitude.ToString());
        form.AddField("bearing", _clientInfo.CurrentBearing.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post("http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/add_3d_comment.php", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
        }
        Comment.text = "";
        HideCommentInputCanvas();
    }

}
