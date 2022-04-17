using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMgr : MonoBehaviour
{
    public Button m_Start_Btn;

    // Start is called before the first frame update
    void Start()
    {
        if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartBtnClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("scPlay_Backup");
    }
}
