using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/*
    펫 데이터 하나를 받아, "카드 한 장" 으로 완벽하게 표기하는 담당
*/
public class GachaResultCard : MonoBehaviour
{
    [Header("UI 연결")]
    public Transform splashArtSpawnPoint;
    //public GameObejct newMarker;

    public void Setup(GachaResult result)
    {
        var data = result.petData;

        // 신규 획득 여부에 따른 "New" 마커 활성화
        // newMarker.SetActive(result.isNew);

        // PetData에 연결된 스플래시 아트 프리팹이 있다면
        if (data.splashArtPrefab != null)
        {
            // 이 카크의 자식으로 스플래시 아트를 생성
            Instantiate(data.splashArtPrefab, splashArtSpawnPoint);
        }
        else
        {
            Debug.LogWarning($"Pet ID {data.id}의 스플래시 아트 프리팹이 존재하지 않습니다.");   
        }
    }
}
