using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideWall
{
    public bool m_IsColl = false;
    public GameObject m_SideWalls = null;
    public Material m_WallMaterial = null;

    public SideWall()
    {
        m_IsColl = false;
        m_SideWalls = null;
        m_WallMaterial = null;
    }
}

public class FollowCam : MonoBehaviour
{
    public GameObject[] CharObjs;   //캐릭터 종류
    int CharType = 0;

    public Transform targetTr;      //추적할 타깃 게임오브젝트의 Transform 변수
    public float dist = 10.0f;      //카메라와의 일정 거리
    public float height = 3.0f;     //카메라의 높이 설정
    public float dampTrace = 20.0f; //부드러운 추적을 위한 변수
    private Transform tr;           //카메라 자신의 Transform 변수

    Vector3 a_PlayerVec = Vector3.zero;
    float rotSpeed = 10.0f;

    //---------- Side Wall 리스트 관련 변수
    Vector3 a_CacVLen = Vector3.zero;
    Vector3 a_CacDirVec = Vector3.zero;

    GameObject[] a_SideWalls = null;
    [HideInInspector] public LayerMask m_WallLyMask = -1;
    List<SideWall> m_SW_List = new List<SideWall>();
    //---------- Side Wall 리스트 관련 변수

    // Start is called before the first frame update
    void Start()
    {
        tr = GetComponent<Transform>();
        //카메라 자신의 Transform 컴포넌트를 tr에 할당

        dist = 4.0f;
        height = 2.8f;


        //----------Side Wall 리스트에 만들기...
        m_WallLyMask = 1 << LayerMask.NameToLayer("SideWall");
        //"SideWall"번 레이어만 피킹하려고 할 경우 

        a_SideWalls = GameObject.FindGameObjectsWithTag("SideWall");
        if (0 < a_SideWalls.Length)
        {
            SideWall a_SdWall = null;
            for (int a_ii = 0; a_ii < a_SideWalls.Length; a_ii++)
            {
                a_SdWall = new SideWall();
                a_SdWall.m_IsColl = false;
                a_SdWall.m_SideWalls = a_SideWalls[a_ii];
                a_SdWall.m_WallMaterial =
                    a_SideWalls[a_ii].GetComponent<Renderer>().material;
                WallAlphaOnOff(a_SdWall.m_WallMaterial, false);
                m_SW_List.Add(a_SdWall);
            }
        }// if (0 < a_SideWalls.Length)
        //----------Side Wall 리스트에 만들기...

        CharacterChange();
    }

    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        {
            height -= (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));
            if (height < 0.1f)
                height = 0.1f;

            if (5.7f < height)
                height = 5.7f;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CharacterChange();
        }//if(Input.GetKeyDown(KeyCode.C))
    }//void Update()

    void CharacterChange()
    {
        Vector3 a_Pos = CharObjs[CharType].transform.position;
        Quaternion a_Rot = CharObjs[CharType].transform.rotation;
        CharObjs[CharType].SetActive(false);
        CharType++;
        if (1 < CharType)
            CharType = 0;
        CharObjs[CharType].SetActive(true);
        CharObjs[CharType].transform.position = a_Pos;
        CharObjs[CharType].transform.rotation = a_Rot;
        targetTr = CharObjs[CharType].transform;
    }

    //Update 함수 호출 이후 한 번씩 호출되는 함수인 LateUpdate 사용
    //추적할 타깃의 이동이 종료된 이후에 카메라가 추적하기 위한 LateUpdare 사용
    // Update is called once per frame
    //void LateUpdate()
    void FixedUpdate()
    {
        a_PlayerVec = targetTr.position;
        a_PlayerVec.y = a_PlayerVec.y + 1.2f;

        //카메라의 위치를 추적대상의 dist 변수만큼 뒤쪽으로 배치하고
        //height 변수만큼 위로 올림
        tr.position = Vector3.Lerp(tr.position,
                                    targetTr.position
                                    - (targetTr.forward * dist)
                                    + (Vector3.up * height),
                                    Time.deltaTime * dampTrace);
        //카메라의 타깃 게임오브젝트를 바라보게 설정
        tr.LookAt(a_PlayerVec);

        //-------------- Wall 카메라 충돌 처리 부분 
        a_CacVLen = this.transform.position - targetTr.position;
        a_CacDirVec = a_CacVLen.normalized;
        GameObject a_FindObj = null;
        RaycastHit a_hitInfo;
        if (Physics.Raycast(targetTr.position + (-a_CacDirVec * 1.0f),
            a_CacDirVec, out a_hitInfo, a_CacVLen.magnitude + 4.0f, m_WallLyMask.value))
        {
            a_FindObj = a_hitInfo.collider.gameObject;
        }

        for (int a_ii = 0; a_ii < m_SW_List.Count; a_ii++)
        {
            if (m_SW_List[a_ii].m_SideWalls == null)
                continue;

            if (m_SW_List[a_ii].m_SideWalls == a_FindObj)
            {
                if (m_SW_List[a_ii].m_IsColl == false)
                {
                    WallAlphaOnOff(m_SW_List[a_ii].m_WallMaterial, true);
                    m_SW_List[a_ii].m_IsColl = true;
                }
            }//if(m_SW_List[a_ii].m_SideWalls == a_FindObj)
            else
            {
                if (m_SW_List[a_ii].m_IsColl == true)
                {
                    WallAlphaOnOff(m_SW_List[a_ii].m_WallMaterial, false);
                    m_SW_List[a_ii].m_IsColl = false;
                }
            }
        }//for (int a_ii = 0; a_ii < m_SW_List.Count; a_ii++)
        //-------------- Wall 카메라 충돌 처리 부분 

    } //void FixedUpdate()

    void WallAlphaOnOff(Material matl, bool isOn = true)
    {
        if(isOn == true)
        {
            matl.SetFloat("_Mode", 3); //Transparent
            matl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            matl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            matl.SetInt("_ZWrite", 0);
            matl.DisableKeyword("_ALPHATEST_ON");
            matl.DisableKeyword("_ALPHABLEND_ON");
            matl.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            matl.renderQueue = 3000;
            matl.color = new Color(1, 1, 1, 0.2f);
        }
        else
        {
            matl.SetFloat("_Mode", 0); //Opaque
            matl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            matl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            matl.SetInt("_ZWrite", 1);
            matl.DisableKeyword("_ALPHATEST_ON");
            matl.DisableKeyword("_ALPHABLEND_ON");
            matl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            matl.renderQueue = -1;
            matl.color = new Color(1, 1, 1, 1);
        }
    }
}
