using MiniJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countMaster : MonoBehaviour {

    public GameObject one;
    public GameObject ten;
    public GameObject hund;
    public GameObject thou;
    public GameObject tthou;

    public GameObject[] list = new GameObject[5];

    public GameObject nokori;
    public GameObject people;

    public int number;

    public bool flag = false;

    private void Start()
    {
        list[0] = one;
        list[1] = ten;
        list[2] = hund;
        list[3] = thou;
        list[4] = tthou;
        if (System.DateTime.Now.Month == 9 && System.DateTime.Now.Day < 28)
        {
            flag = false;
            set(29 - System.DateTime.Now.Day);
            nokori.SetActive(true);
            people.SetActive(false);
        }
        else
        {
            flag = true;
            StartCoroutine("getPeople");
            nokori.SetActive(false);
            people.SetActive(true);
        }
        
    }

    private void Update()
    {
    } 

    private IEnumerator getPeople()
    {
        while (true)
        {
            WWW www = new WWW("https://api.kinensai.jp/count.php");
            yield return www;

            Dictionary<string, object> itemDic = Json.Deserialize(www.text) as Dictionary<string, object>;
            set(int.Parse(itemDic["count"].ToString()));

            yield return new WaitForSeconds(2.0f);
        }
        
    }

    public void set(int a)
    {
        number = a;
        string tmp = a.ToString();

        for(int i = 0; i < 5; i++)
        {
           if(tmp.Length > i) list[i].SetActive(true);
           else list[i].SetActive(false);
        }
        for(int ii = 0; ii < tmp.Length; ii++)
        {

            for(int iii = 0; iii <list[ii].transform.childCount; iii++)
            {
                list[ii].transform.GetChild(iii).GetComponent<MeshRenderer>().transform.gameObject.SetActive(false);
            }
            MeshRenderer boxCol2D = list[ii].transform.Find(tmp[tmp.Length - 1 - ii].ToString()).GetComponent<MeshRenderer>();
            boxCol2D.transform.gameObject.SetActive(true);
        }
        
    }

}
