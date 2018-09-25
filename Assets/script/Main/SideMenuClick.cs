using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SideMenuClick : MonoBehaviour {

	public void OnClick()
    {
        var text = GetComponentInChildren<Text>();
        UserData.instance.lastMode.Push(SceneManager.GetActiveScene().name);
        GetComponentInParent<SideMenu>().enable = false;
        switch (text.text)
        {
            case "ホーム":
                SceneManager.LoadScene("mainhome_");
                break;
            case "企画一覧":
                SceneManager.LoadScene("kikakuhome");
                break;
            case "タイムテーブル":
                SceneManager.LoadScene("tthome");
                break;
            case "マップ":
                SceneManager.LoadScene("kougaimap");
                break;
            case "基本方針":
                SceneManager.LoadScene("Meibo");
                break;
            case "アンケート":
                SceneManager.LoadScene("ankethome");
                break;
            case "実行委員名簿":
                UserData.instance.command = "meibo";
                SceneManager.LoadScene("Meibo");
                break;

        }
    }
}
