using System.Collections;
using System.IO;
using System.Collections.Generic;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.SceneManagement;
using MiniJSON;
using LitJson;
using System;
using UniRx;


public class UserData : MonoBehaviour
{
    public User user;
    public GameObject messageBoxObject;
    public Vector2 swipeDif;
    public long numberOfPeopleCame;
    public IList classTT;
    public List<ClassProjectList> classProject;
    public List<TT> kodoTT, stageTT;
    public List<ClubProjectList> clubProject;
    public Stack<string> lastMode;
    public string[] path = new string[3];
    public string[] versions = new string[5];
    public int[] touchVal;
    public string command;
    public static UserData instance;
    public static Vector2 screenSize;
    public FlickGesture flickGesture;
    public MessageBox messageBox;

    private Vector2 lastTouchPos;
    private Queue<string> notice;
    private float timeInterval;
    private float timeInterval2;
    private bool noticeflag;

    void Start()
    {
        screenSize = new Vector2(Screen.width, Screen.height);
#if UNITY_EDITOR
        Debug.Log("Unity Editor");
        path[0] = "./Assets\\script\\Data\\";
#elif UNITY_IPHONE
        Debug.Log("Unity iPhone");
        path[0] = Application.persistentDataPath;
#elif UNITY_ANDROID
        Debug.Log("Unity Android");
        path[0] = Application.persistentDataPath;
#endif
        instance = this;
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(messageBoxObject);
        messageBox = messageBoxObject.GetComponent<MessageBox>();
        lastMode = new Stack<string>();
        //StartCoroutine(CheckVersion());

        if (File.Exists(path[0]+"\\UserData.json") == false)
        {
            StartCoroutine(CreateUserID());
        }
        else
        {
            Load();
        }

        noticeflag = true;

        StartCoroutine(RequestProjects());
        StartCoroutine(RequestClubProjects());
        StartCoroutine(RequestCount());
        StartCoroutine(UpAnswer());
        StartCoroutine(RequestTT(true));
        StartCoroutine(RequestTT(false));
        touchVal = new int[] { 0, 0, 0 };
    }

    private void OnEnable()
    {
        flickGesture.Flicked += OnFlicked;
    }

    private void OnDisable()
    {
        flickGesture.Flicked -= OnFlicked;
    }

    private void OnFlicked(object sender, EventArgs e)
    {
        Debug.Log("フリックされた: " + flickGesture.ScreenFlickVector);
        if (flickGesture.ScreenFlickVector.x < 0)
        {
            touchVal[0] = 3;
        }
        else
        {
            touchVal[0] = 4;
        }
    }

    void Update()
    {
        timeInterval += Time.deltaTime;
        timeInterval2 += Time.deltaTime;
        if (Input.touchCount > 0 && touchVal[0] < 3)
        {
            touchVal[1] = Input.touchCount;
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchVal[0] = 1;
                lastTouchPos = Input.mousePosition;
            }
            else if (touch.phase == TouchPhase.Stationary)
            {
                touchVal[0] = 2;
                swipeDif = (lastTouchPos - touch.position) / screenSize;
                lastTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                touchVal[0] = 2;
                swipeDif = (lastTouchPos - touch.position) / screenSize;
                lastTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchVal[0] = 0;
            }
        }
        else
        {
            touchVal[1] = 0;
        }
        if (timeInterval >= 3f && noticeflag && notice.Count > 0)
        {
            noticeflag = false;
            //StartCoroutine(Notice());
            timeInterval = 0f;
        }
        if (Application.platform == RuntimePlatform.Android)
        {
            // エスケープキー取得
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                command = "";
                lastMode.Clear();
                SceneManager.LoadScene("mainhome_");
            }
        }
        if (timeInterval2 >= 60f)
        {
            StartCoroutine(RequestProjects());
            StartCoroutine(RequestClubProjects());
            StartCoroutine(RequestCount());
            StartCoroutine(RequestTT(true));
            StartCoroutine(RequestTT(false));
            Save();
            timeInterval2 = 0f;
        }
    }

    public void Save()
    {
        //user.Encode();
        string SaveData = JsonUtility.ToJson(user);
        File.WriteAllText(path[0] + "\\UserData.json", SaveData);
    }

    public void Load()
    {
        string LoadData = File.ReadAllText(path[0] + "\\UserData.json");
        JsonUtility.FromJsonOverwrite(LoadData, user);
        notice = new Queue<string>(user.booking);
        //user.Encode();
    }

    public IEnumerator CreateUserID()
    {
        int cnt = 0;
        while (true)
        {
            WWWForm form = new WWWForm();
            form.AddField("from", "app");
            WWW www = new WWW("https://api.kinensai.jp/AddUser.php", form);
            yield return www;
            if (www.error != null)
            {
                yield return StartCoroutine(messageBox.PrintMessage("通信エラー", "接続状況を確認してください", true, true));
                cnt++;
                if (cnt >= 3)
                {
                    yield return StartCoroutine(messageBox.PrintMessage("通信エラー", "情報の取得に失敗しました\nアプリを終了し時間を空けて\nやり直してください。", false, true));
                }
                continue;
            }
            else
            {
                IList jsonlist = (IList)Json.Deserialize("[" + www.text + "]");
                foreach (IDictionary param in jsonlist)
                {
                    user.id = (long)param["COUNT(*)"] + 1;
                }
                Save();
                break;
            }
        }
    }

    public IEnumerator RequestCount()
    {
        WWW www = new WWW("https://api.kinensai.jp/count.php");
        yield return www;
        if (www.error != null)
        {
            StartCoroutine(messageBox.PrintMessage("通信エラー", "接続状況を確認してください", true, true));
        }
        else
        {
            IList jsonlist = (IList)Json.Deserialize("[" + www.text + "]");
            foreach (IDictionary param in jsonlist)
            {
                numberOfPeopleCame = (long)param["count"];
            }
        }
    }

    public IEnumerator RequestProjects()
    {
        WWW www = new WWW("https://api.kinensai.jp/ClassProject.php");
        //WWW www = new WWW("http://localhost/ClassProject.php");
        yield return www;
        if (www.error != null)
        {
            StartCoroutine(messageBox.PrintMessage("通信エラー", "接続状況を確認してください", true, true));
        }
        else
        {
            classProject = JsonMapper.ToObject<List<ClassProjectList>>("["+www.text+"]");
        }
    }
    
    public IEnumerator RequestClubProjects()
    {
        WWW www = new WWW("https://api.kinensai.jp/ClubProject.php");
        //WWW www = new WWW("http://localhost/ClubProject.php");

        yield return www;
        if (www.error != null)
        {
            StartCoroutine(messageBox.PrintMessage("通信エラー", "接続状況を確認してください", true, true));
        }
        else
        {
            Debug.Log(www.text);
            clubProject = JsonMapper.ToObject<List<ClubProjectList>>("[" + www.text + "]");
        }

    }

    public IEnumerator RequestTT(bool place)
    {
        WWWForm form = new WWWForm();
        if (place) form.AddField("place", "講堂");
        else       form.AddField("place", "ステージ");
        WWW www = new WWW("https://api.kinensai.jp/TT.php", form);
        //WWW www = new WWW("http://localhost/TT.php", form);
        yield return www;
        if (www.error != null)
        {
            StartCoroutine(messageBox.PrintMessage("通信エラー", "接続状況を確認してください", true, true));
        }
        else
        {
            Debug.Log(www.text);
            if (place)
                kodoTT = JsonMapper.ToObject<List<TT>>("[" + www.text + "]");
            else
                stageTT = JsonMapper.ToObject<List<TT>>("[" + www.text + "]");
        }

    }

    public IEnumerator UpAnswer()
    {
        Save();
        //user.Encode();
        WWWForm form = new WWWForm();
        form.AddField("from", "app");
        form.AddField("ID", user.id.ToString());
        form.AddField("a1", user.anke1);
        if (user.anke23[0] != "") form.AddField("a1_5", user.anke23[0]);
        else form.AddField("a1_5", "null");
        if (user.anke23[1] != "") form.AddField("a2", user.anke23[1]);
        else form.AddField("a2", "null");
        if (user.anke23[2] != "") form.AddField("a3", user.anke23[2]);
        else form.AddField("a3", "null");
        form.AddField("a4", user.anke4.ToString());
        WWW www = new WWW("https://api.kinensai.jp/Answer.php", form);
        yield return www;
        if (www.error != null)
        {
            StartCoroutine(messageBox.PrintMessage("通信エラー", "接続状況を確認してください", true, true));
        }
        else
        {
            Debug.Log(www.text);
        }

    }
    /*
    public void Notice(string Name,)
    {
        noticeflag = true;
    }
    */
}
