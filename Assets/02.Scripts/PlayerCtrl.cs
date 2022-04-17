using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //UI 항목에 접근하기 위해 번드시 추가

//클래스에 System.Serializable 이라는 어트리뷰트(Attribute)를 명시해야
//Inspector 뷰에 노출됨
[System.Serializable]
public class Anim
{
    public AnimationClip idle;
    public AnimationClip runForward;
    public AnimationClip runBackward;
    public AnimationClip runRight;
    public AnimationClip runLeft;
}

public class PlayerCtrl : MonoBehaviour
{
    private float h = 0.0f;
    private float v = 0.0f;

    //접근해야 하는 컴포넌트는 반드시 변수에 할당한 후 사용
    private Transform tr;
    //이동 속도 변수
    public float moveSpeed = 10.0f;

    //회전 속도 변수
    public float rotSpeed = 100.0f;

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;

    //아래에 있는 3D 모델의 Animation 컴포넌트에 접근하기 위한 변수
    public Animation _animation;

    //Player의 생명 변수
    public int hp = 100;
    //Player의 생명 초깃값
    private int initHp;
    //Player의 Health bar 이미지
    public Image imgHpber;
    
    // 게임오버 판넬
    public GameObject gameOverPanel = null;

    // Start is called before the first frame update
    void Start()
    {
        //생명 초깃값 설정
        initHp = hp;

        //스크립트 처음에 Transform 컴포넌트 할당
        tr = GetComponent<Transform>();

        //자신의 하위에 있는 Animation 컴포넌트를 찾아와 변수에 할당
        _animation = GetComponentInChildren<Animation>();

        //Animation 컴포넌트의 애니메이션 클립을 지정하고 실행
        _animation.clip = anim.idle;
        _animation.Play();
    }

    // Update is called once per frame
    //void Update()
    void FixedUpdate()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
        {
            // gameOverPanel 활성화
            gameOverPanel.SetActive(true);
            return;
        }

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        //전후좌우 이동 방향 벡터 계산
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        //Translate(이동방향 * Time.deltaTime * 변위값 * 속도, 기준좌표);
        tr.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);

        if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        {
            //Vector3.up 축을 기준으로 rotSpeed만큼 속도로 회전
            tr.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X") * 3.0f);
        }

        //키보드 입력값을 기준으로 동작할 애니메이션 수행
        if(v >= 0.1f)
        {
            _animation.CrossFade(anim.runForward.name, 0.3f); //전진 애니메이션
        }
        else if(v <= -0.1f)
        {
            _animation.CrossFade(anim.runBackward.name, 0.3f); //후진 애니메이션
        }
        else if(h >= 0.1f)
        {
            _animation.CrossFade(anim.runRight.name, 0.3f);    //오른쪽 애니메이션
        }
        else if(h <= -0.1f)
        {
            _animation.CrossFade(anim.runLeft.name, 0.3f);     //왼쪽 애니메이션
        }
        else
        {
            _animation.CrossFade(anim.idle.name, 0.3f);         //정지시 idle애니메이션
        }
    }

    //충돌한 Collider의 IsTrigger 옵션이 체크됐을 때 발생
    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP 차감
        if(coll.gameObject.tag == "PUNCH")
        {
            if (hp <= 0.0f)
                return;

            hp -= 10;
            //Debug.Log("Player HP =  " + hp.ToString());

            if (imgHpber == null)
                imgHpber = GameObject.Find("Hp_Image").GetComponent<Image>();

            if (imgHpber != null)
                imgHpber.fillAmount = (float)hp / (float)initHp;

            //Player의 생명이 0이하이면 사망 처리
            if (hp <= 0)
            {
                PlayerDie();
            }
        }
    }

    //Player의 사망 처리 루틴
    void PlayerDie()
    {
        //MONSTER라는 Tag를 가진 모든 게임오브젝트를 찾아옴
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        foreach(GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        }

        _animation.Stop();

        GameMgr.s_GameState = GameState.GameEnd;
    }
}
