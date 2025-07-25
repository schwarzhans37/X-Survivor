using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // ==== 싱글톤 패턴 ====
    public static MenuManager instance;
    // ====================

    [Header("UI 패널 관리자")]
    public GameObject bg_Image; // menu_1, menu_2 전용 배경이미지
    public GameObject gacha_BackImage;
    public GameObject menu1_Panel;  // Game Start, Setting, Exit 버튼이 있는 패널
    public GameObject menu2_Panel;  // New Game, Load, Gacha, Back 버튼이 있는 패널
    public GameObject NewGame_Panel;
    public GameObject charSelect_Panel;    // 캐릭터 선택 패널
    public CharaSelectionUI charaSelectionUI;
    public GameObject Gacha_Panel;
    public GameObject PetSelect_Panel;     // 펫 선택 패널
    public PetSelectionUI petSelectionUI;

    void Awake()
    {
        // ===== 싱글톤 초기화 =====
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // ========================
    }

    void Start()
    {
        // 게임 시작 시 메인 메뉴(Menu1_panel)만 보이도록
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        bg_Image.SetActive(true);
        menu1_Panel.SetActive(true);
        menu2_Panel.SetActive(false);
        charSelect_Panel.SetActive(false);
        Gacha_Panel.SetActive(false);
        PetSelect_Panel.SetActive(false);
    }

    /* 
         ===== 여기서부터는 버튼 OnClick 이벤트 =====
    */

    // Menu_1의 'GAME START'버튼 클릭 시 호출
    public void GameStart()
    {
        Debug.Log("Game Start 버튼이 클릭됨");
        menu1_Panel.SetActive(false);
        menu2_Panel.SetActive(true);
    }

    /* Menu_1의 'SETTING'버튼 클릭 시 호출
    public void Setting()
    {
        Debug.Log("Setting 버튼이 클릭됨(아직 미구현 페이지)")
        차후 설정 창 띄우는 코드 추가
    }
    */

    // Menu_1의 'EXIT'버튼 클릭 시 호출
    public void Exit()
    {
        Debug.Log("게임 종료 버튼이 클릭됨");
        Application.Quit();
    }

    // Menu_2의 'NEW GAME'버튼 클릭 시 호출
    public void NewGame()
    {
        Debug.Log("New Game 버튼이 클릭됨");
        menu2_Panel.SetActive(false);
        NewGame_Panel.SetActive(true);
        charSelect_Panel.SetActive(true);
        bg_Image.SetActive(false);
    }

    /* Menu_2의 'LOAD'버튼 클릭 시 호출
    public void LoadSave()
    {
        Debug.Log("세이브 파일 로드 버튼이 클릭됨(아직 미구현 기능)");
        차후 세이브 파일 띄우는 페이지 추가
    }
    */

    // Menu_2의 'Gacha'버튼 클릭 시 호출
    public void Gacha()
    {
        Debug.Log("펫 가챠 페이지 버튼이 클릭됨");
        Gacha_Panel.SetActive(true);
        menu2_Panel.SetActive(false);
    }

    public void BackFromCGacha()
    {
        Debug.Log("Back 버튼이 클릭됨(이전 메뉴로 이동)");
        Gacha_Panel.SetActive(false);
        gacha_BackImage.SetActive(false);
        bg_Image.SetActive(true);
        menu2_Panel.SetActive(true);
    }

    // Menu_2의 'BACK'버튼 클릭 시 호출
    public void BackToMainMenu()
    {
        Debug.Log("Back 버튼이 클릭됨(메인 메뉴로 이동)");
        ShowMainMenu();
    }

    // 캐릭터 선택 화면의 캐릭터를 선택한 후 'NEXT'버튼 클릭 시 호출
    public void GoToPetSelect()
    {
        charaSelectionUI.ConfirmSelection();
    }

    // PetSelect 페이지로 넘어가는 실제 로직(ConfirmSelection에서 호출)
    public void ShowPetSelectPage()
    {
        Debug.Log("펫 선택 메뉴로 이동합니다.");
        charSelect_Panel.SetActive(false);
        PetSelect_Panel.SetActive(true);
    }

    // 펫 선택 후 'START' 버튼이 눌렸을 때
    public void StartGameFromPetSelect()
    {
        // 모든 로직을 PetSelectionUI에 위임
        petSelectionUI.ConfirmSelection();
    }

    // PetSelectionUI가 호출해 줄 실제 게임 시작 함수
    public void StartGame()
    {
        Debug.Log("게임이 시작됩니다.");
        SceneManager.LoadScene("GamePlayScene");
    }

    // 캐릭터 선택 화면의 'BACK'버튼 클릭 시 호출
    public void BackFromCharaSelect()
    {
        Debug.Log("Back 버튼이 클릭됨(이전 메뉴로 이동)");
        NewGame_Panel.SetActive(false);
        bg_Image.SetActive(true);
        menu2_Panel.SetActive(true);
    }

    // 펫 선택 페이지에서 'BACK'버튼 클릭 시 호출
    public void BackFromPetSelect()
    {
        Debug.Log("Back 버튼이 클릭됨 (캐릭터 선택으로 이동)");
        PetSelect_Panel.SetActive(false);
        charSelect_Panel.SetActive(true);
    }
}
