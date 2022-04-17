using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    GameIng,
    GameEnd
}

public class GameMgr : MonoBehaviour
{
    public static GameState s_GameState = GameState.GameIng;

    //Text UI 항목 연결을 위한 변수
    public Text txtScore;
    public Text gameOverScore;
    //누적 점수를 기록하기 위한 변수
    private int totScore = 0;

    public Button BackBtn;

    [Header("-------- Monster Spawn --------")]
    //몬스터가 출현할 위치를 담을 배열
    public Transform[] points;
    //몬스터 프리팹을 할당할 변수
    public GameObject monsterPrefab;
    // 몬스터를 미리 생성해 저장할 리스트 자료형
    public List<GameObject> monsterPool = new List<GameObject>();

    //몬스터를 발생시킬 주기
    public float createTime = 2.0f;
    //몬스터의 최대 발생 개수
    public int maxMonster = 10;
    //게임 종료 여부 변수
    public bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        s_GameState = GameState.GameIng;
        //처음 실행 후 저장된 스코어 정보 로드
        DispScore(0);

        BackBtn.onClick.AddListener(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        });

        // 몬스터를 생성해 오브젝트 풀에 저장
        for(int i = 0; i < maxMonster; i++)
        {
            // 몬스터 프리팹을 생성
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
            // 생성한 몬스터의 이름 설정
            monster.name = "Monster_" + i.ToString();
            monster.SetActive(false);
            // 생성한 몬스터를 오브젝트 풀에 추가
            monsterPool.Add(monster);
        }

        //---------------- Monster Spawn
        //Hierarchy 뷰의 SpawnPoint를 찾아 하위에 있는 모든 Transform 컴포넌트를 찾아옴
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        if (points.Length > 0)
        {
            //몬스터 생성 코루틴 함수 호출
            StartCoroutine(this.CreateMonster());
        }
    }

    // Update is called once per frame
    void Update()
    {       
    }

    //점수 누적 및 화면 표시
    public void DispScore(int score)
    {
        totScore += score;
        txtScore.text = "SCORE <color=#ff0000>" + totScore.ToString() + "</color>";
        gameOverScore.text = "SCORE <color=#ff0000>" + totScore.ToString() + "</color>";
    }

    //몬스터 생성 코루틴 함수
    IEnumerator CreateMonster()
    {
        //게임 종료 시까지 무한 루프
        while (!isGameOver)
        {
            // 몬스터 생성 주기 시간만큼 메인 루프에 양보
            yield return new WaitForSeconds(createTime);

            //플레이어가 사망했을 때 코루틴을 종료해 다음 루틴을 진행하지 않음
            if (GameMgr.s_GameState == GameState.GameEnd)
                yield break;     //코루틴 함수에서 함수를 빠져나가는 명령 

            // 오브젝트 풀의 처음부터 끝까지 순회
            foreach(GameObject monster in monsterPool)
            {
                // 비활성화 여부로 스폰 가능한 몬스터를 판단
                if (!monster.activeSelf)
                {
                    // 몬스터를 출현시킬 위치의 인덱스값을 추출
                    int idx = Random.Range(1, points.Length);
                    // 몬스터의 출현위치를 설정
                    monster.transform.position = points[idx].position;
                    // 몬스터를 활성화함
                    monster.SetActive(true);
                    // 오브젝트 풀에서 몬스터 프리팹 하나를 활성화한 후 for 루프를 빠져나감
                    break;
                }
            }

            ////현재 생성된 몬스터 개수 산출
            //int monsterCount = (int)GameObject.FindGameObjectsWithTag("MONSTER").Length;

            ////몬스터의 최대 생성 개수보다 작을 때만 몬스터 생성
            //if(monsterCount < maxMonster)
            //{
            //    //몬스터의 생성 주기 시간만큼 대기
            //    yield return new WaitForSeconds(createTime);

            //    //불규칙적인 위치 산출
            //    int idx = Random.Range(1, points.Length);
            //    //몬스터의 동적 생성
            //    Instantiate(monsterPrefab, points[idx].position, points[idx].rotation);
            //}
            //else
            //{
            //    yield return null;
            //}

        }//while (!isGameOver)
    }
}
