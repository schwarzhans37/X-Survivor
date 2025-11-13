using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 레거시 Text를 사용하기 위해 필요

public class PetDeathCountUI : MonoBehaviour
{
    public Text petDeathText; // Inspector에서 연결할 Text 컴포넌트

    private PetController petController; // 펫의 참조를 저장할 변수
    private GameObject petDeathGroup; // 부모 오브젝트 (펫이 없을 때 숨기기용)

    void Start()
    {
        // 펫 컨트롤러를 찾아서 변수에 저장해 둡니다.
        petController = FindObjectOfType<PetController>();

        // 이 스크립트가 붙어있는 오브젝트의 바로 위 부모를 가져옵니다. (Pet_Deaths 오브젝트)
        petDeathGroup = transform.parent.gameObject;

        // 게임 시작 시 펫이 없으면 UI 항목 전체를 숨깁니다.
        if (petController == null)
        {
            if (petDeathGroup != null)
            {
                petDeathGroup.SetActive(false);
            }
        }
    }

    // LateUpdate는 모든 게임 로직이 끝난 후 호출되므로, 최종 값을 표시하기에 좋습니다.
    void LateUpdate()
    {
        // 펫이 씬에 존재할 때만 텍스트를 업데이트합니다.
        if (petController != null)
        {
            // petController가 가지고 있는 deathCount 값을 가져와 텍스트에 표시합니다.
            petDeathText.text = $"{petController.deathCount}";
        }
    }
}