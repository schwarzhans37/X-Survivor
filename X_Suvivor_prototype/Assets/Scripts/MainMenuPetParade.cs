using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPetParade : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("화면에 등장시킬 펫 워커 프리팹 배열 (순서대로 등장)")]
    public GameObject[] petWalkerPrefabs;

    [Tooltip("펫이 생성될 왼쪽 화면 밖 위치")]
    public Transform startPoint;

    [Tooltip("다음 펫이 생성될 트리거가 되는 중간 지점")]
    public Transform midPoint;

    [Tooltip("펫이 사라질 오른쪽 화면 밖 위치")]
    public Transform endPoint;

    [Tooltip("펫의 이동 속도")]
    public float moveSpeed = 1.5f;

    [Tooltip("첫 펫이 등장하기 전 딜레이 (초)")]
    public float initialDelay = 1f;

    // 현재 화면에 있는 펫들을 관리하기 위한 리스트
    private List<GameObject> activePets = new List<GameObject>();
    // 다음에 생성할 펫의 인덱스
    private int currentPetIndex = 0;
    // 다음 펫을 생성해도 되는지 여부 (중간 지점 도달 시 true가 됨)
    private bool canSpawnNext = false;

    void Awake()
    {
        // 씬이 로드될 때, 이전에 게임 시간이 멈췄을(Time.timeScale = 0) 수 있으므로
        // 무조건 시간을 1배속(정상)으로 되돌립니다.
        Time.timeScale = 1f;
    }

    IEnumerator Start()
    {
        // 시작 위치, 중간 위치, 끝 위치가 설정되지 않았으면 경고 후 종료
        if (startPoint == null || midPoint == null || endPoint == null)
        {
            Debug.LogError("시작, 중간, 끝 지점 Transform이 모두 설정되어야 합니다!");
            yield break; // 코루틴 중단
        }

        // 첫 펫 등장 전 딜레이
        yield return new WaitForSeconds(initialDelay);

        // 첫 펫은 바로 생성
        SpawnNextPet();

        // 메인 루프: 다음 펫 생성 조건이 될 때까지 기다렸다가 생성 반복
        while (true) // 무한 반복 (게임이 꺼질 때까지)
        {
            // canSpawnNext가 true가 될 때까지 (펫이 중간 지점에 도달할 때까지) 기다림
            yield return new WaitUntil(() => canSpawnNext);

            // 다음 펫 생성
            SpawnNextPet();
        }
    }

    // 다음 순서의 펫을 생성하는 함수
    void SpawnNextPet()
    {
        // 생성할 펫이 없으면 (배열이 비었으면) 함수 종료
        if (petWalkerPrefabs == null || petWalkerPrefabs.Length == 0) return;

        // 다음 펫 생성을 잠시 막음 (새로 생성된 펫이 중간에 도달해야 다시 true가 됨)
        canSpawnNext = false;

        // 배열 인덱스가 범위를 벗어나지 않도록 나머지 연산자(%) 사용 (순환 구조)
        currentPetIndex = currentPetIndex % petWalkerPrefabs.Length;

        // 프리팹 생성
        GameObject newPet = Instantiate(petWalkerPrefabs[currentPetIndex], startPoint.position, Quaternion.identity, transform); // 이 오브젝트의 자식으로 생성
        activePets.Add(newPet); // 활성 펫 리스트에 추가

        // PetWalker 스크립트 가져오기
        PetWalker walker = newPet.GetComponent<PetWalker>();
        if (walker != null)
        {
            // 워커 초기화 (속도, 지점 X좌표, 콜백 함수 전달)
            walker.Initialize(moveSpeed, midPoint.position.x, endPoint.position.x, OnPetReachedMidpoint, OnPetReachedEndpoint);
        }
        else
        {
            Debug.LogError($"{newPet.name} 프리팹에 PetWalker 스크립트가 없습니다!");
        }

        // 다음 생성할 펫 인덱스 증가
        currentPetIndex++;
    }

    // PetWalker가 중간 지점에 도달했을 때 호출할 함수 (콜백)
    void OnPetReachedMidpoint(GameObject pet)
    {
        // 다음 펫을 생성해도 좋다는 신호
        canSpawnNext = true;
    }

    // PetWalker가 끝 지점에 도달했을 때 호출할 함수 (콜백)
    void OnPetReachedEndpoint(GameObject pet)
    {
        // 리스트에서 제거하고 오브젝트 파괴
        if (activePets.Contains(pet))
        {
            activePets.Remove(pet);
        }
        Destroy(pet);
    }

    // (선택 사항) 에디터에서 경로를 시각적으로 보기 위한 Gizmos
    void OnDrawGizmos()
    {
        if (startPoint != null && midPoint != null && endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPoint.position, midPoint.position);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(midPoint.position, endPoint.position);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPoint.position, 0.3f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(midPoint.position, 0.3f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.3f);
        }
    }
}

