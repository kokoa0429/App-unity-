using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackHome : MonoBehaviour {

	public void OnClick()
    {
        UserData.instance.command = "";
        UserData.instance.lastMode.Clear();
        SceneManager.LoadScene("mainhome_");
    }
}
