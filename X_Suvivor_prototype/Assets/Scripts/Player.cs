using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("# 플레이어 캐릭터 능력치")]
    public float speed; //실 적용 이동속도
    public float baseSpeed; // 기본 이동속도
    public float health;    // 현재 체력
    public float maxHealth;   // 최대 체력
    //스킬은 쿨타임제로 , 기초 스텟에 미반영

    [Header("# 플레이어 캐릭터 물리")]
    public Vector2 inputVec;    // 캐릭터 좌표
    public ItemScanner itemScanner;     // 캐릭터의 아이템 회수범위

    [Header("# 캐릭터 이미지 렌더")]
    public Hand[] hands;

    [Header("캐릭터별 애니메이션 컨트롤러")]
    public RuntimeAnimatorController[] spriteAnimCon; // 플레이어 캐릭터 애니메이션 컨트롤러

    Rigidbody2D rigid;  // 충돌 판정 계산 리지드바디
    SpriteRenderer spriter; // 스프라이트(이미지)
    Animator anim;  // 캐릭터 애니메이션

    [Header("게임 중 현재 보유한 무기 목록")]
    public List<WeaponBase> equippedWeapons;

    [Header("게임 중 현재 보유한 장비 목록")]
    public List<Gear> equippedGears;


    private float speedBonusRate = 0f; // 장비를 통한 속도 증가율


    void Awake()
    // 초기화(선언)
    {
        speed = baseSpeed;      // 시작 시 이동속도 초기화
        health = maxHealth;     // 시작 시 체력 초기화
        equippedWeapons = new List<WeaponBase>();   // 무기 리스트 초기화
        equippedGears = new List<Gear>();           // 장비 리스트 초기화
        
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        itemScanner = GetComponent<ItemScanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    public void Init(int charId)
    {
        speed = baseSpeed;

        Debug.Log("[Player.OnEnable] 함수 호출됨!"); // <--- 호출 시점 확인용 로그 추가

        // GameManager에 기록된 playerId를 바탕으로 올바른 애니메이션 컨트롤러를 설정
        int charaId = GameManager.instance.playerId;
        Debug.Log($"[Player.OnEnable] GameManager로부터 받은 캐릭터 ID: {charaId}"); // <--- 확인용 로그 추가
        int controllerIndex = charaId - 1;
        Debug.Log($"[Player.OnEnable] 계산된 컨트롤러 인덱스: {controllerIndex}"); // <--- 확인용 로그 추가

        if (controllerIndex >= 0 && controllerIndex < spriteAnimCon.Length && spriteAnimCon[controllerIndex] != null)
        {
            anim.runtimeAnimatorController = spriteAnimCon[controllerIndex];
            Debug.Log($"[Player.OnEnable] {spriteAnimCon[controllerIndex].name} 컨트롤러 적용 완료!"); // <--- 확인용 로그 추가
        }
        else
        {
            Debug.LogError($"캐릭터 ID: {charaId}에 해당하는 오버라이드 컨트롤러를 찾을 수 없거나 할당되지 않았습니다.");
        }

        UpdateSpeed();
    }
    
    void FixedUpdate()
    /* 
        물리(Physics)계산용 업데이트 함수
        플레이어의 위치 이동을 위해 사용됨
        - 프레임 속도 영향 안받음(컴퓨터 성능 영향 안받음)
        - 고정된 시간 간격으로 호출함
        - Rigidbody
     */
    {
        if (!GameManager.instance.isLive) {
            return;
        }

        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        // 위치 이동
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    /* 
        한 프레임의 모든 Update 함수가 실행 된 후 호출
        오브젝트의 이동, 로직 등의 처리가 끝난 최종 결과를 가지고 사용
        - 프레임 속도에 따라 가변적임
        - 애니메이션 후처리, 카메라 추적 등 사용
     */
    {
        // 애니메이션 플립 로직
        anim.SetFloat("speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }

        // ItemScanner의 아이템 흡수 로직
        if (itemScanner != null && itemScanner.Targets.Length > 0)
        {
            // 모든 감지된 아이템에 대해 반복
            foreach (Collider2D target in itemScanner.Targets)
            {
                ExpOrb expOrb = target.GetComponent<ExpOrb>();
                if (expOrb != null)
                {
                    expOrb.StartSeeking();
                }
            }

        }
    }

    void OnCollisionStay2D(Collision2D collision)
    // 캐릭터가 몬스터와 충돌할 경우 데미지를 받게 함
    {
        // 몬스터 외의 오브젝트(아이템, 경험치 등)와 충돌했을 경우는 제외
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        TakeDamage(Time.deltaTime * 10);
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogError("장착하려는 WeaponData가 null입니다.");
            return;
        }
        if (weaponData.controllerPrefab == null)
        {
            Debug.LogError($"WeaponData '{weaponData.weaponName}'에 ControllerPrefab이 연결되지 않았습니다.");
            return;
        }

        // 무기 오브젝트를 플레이어의 자식으로 생성
        GameObject newWeaponObj = Instantiate(weaponData.controllerPrefab, transform);
        newWeaponObj.transform.localPosition = Vector3.zero;

        // 생성된 오브젝트에서 WeaponBase 컴포넌트를 가져옴
        WeaponBase weaponComponent = newWeaponObj.GetComponent<WeaponBase>();

        // 컴포넌트를 성공적으로 찾으면, 초기화하고 목록에 추가
        if (weaponComponent != null)
        {
            weaponComponent.Init(weaponData); // Init 함수 호출
            equippedWeapons.Add(weaponComponent); // 리스트에 추가
            Debug.Log($"[{weaponData.weaponName}] 무기를 장착했습니다.");
        }
        else
        {
            Debug.LogError($"'{weaponData.controllerPrefab.name}' 프리팹에 WeaponBase를 상속받는 스크립트(Melee, Projectile 등)가 없습니다.");
        }
    }

    public WeaponBase FindEquippedWeapon(WeaponData weaponeData)
    {
        if (weaponeData == null) return null;
        return equippedWeapons.Find(w => w.weaponData.weaponId == weaponeData.weaponId);
    }

    public void EquipGear(GearData gearData)
    {
        if (gearData == null) 
        {
            Debug.LogError("장착하려는 GearData가 null입니다.");
            return;
        }

        // 1. Gear 컴포넌트를 가진 GameObject를 생성합니다.
        GameObject newGearObj = new GameObject();
        newGearObj.transform.parent = transform; // 플레이어의 자식으로 설정
        newGearObj.transform.localPosition = Vector3.zero;

        Gear gearComponent = newGearObj.AddComponent<Gear>();
        
        // 2. 생성된 Gear를 장비 목록에 **먼저** 추가합니다.
        equippedGears.Add(gearComponent);
        
        // 3. Init 함수를 호출하여 데이터를 설정합니다.
        gearComponent.Init(gearData);
        
        // 4. 새 장비가 추가되었으니, 모든 관련 스탯을 즉시 업데이트합니다.
        UpdateAllStatsFromGears();
        
        Debug.Log($"[{gearData.gearName}] 장비를 장착했습니다.");
    }

    public Gear FindEquippedGear(GearData gearData)
    {
        if (gearData == null) return null;
        return equippedGears.Find(g => g.gearData.gearId == gearData.gearId);
    }

    public float GetSpeedBonusFromGears()
    {
        float totalBonus = 0f;
        foreach (Gear gear in equippedGears)
        {
            if (gear.type == GearData.GearType.Shoe)
            {
                totalBonus += gear.rate;
            }
        }
        return totalBonus;
    }

    private void UpdateSpeed()
    {
        // 기본 속도에 보너스 증가율을 적용
        float finalSpeedBonusRate = GetSpeedBonusFromGears();
        speed = baseSpeed + finalSpeedBonusRate;
    }

    // 최종데미지를 올려주는 장비를 얻은 경우, 최종데미지를 무기의 데미지에 합산하여 반환
    public float GetDamageBonusFromGears()
    {
        float totalBonus = 0f;
        foreach (Gear gear in equippedGears)
        {
            // 장비 타입이 장갑(Glove)이라면, 그 장비의 현재 효과 수치(rate)를 더함
            if (gear.type == GearData.GearType.Glove)
            {
                totalBonus += gear.rate;
            }
        }
        return totalBonus;
    }

    // ===== 장비 변경 기 캐릭터의 스텟을 갱신하도록 요청
    public void UpdateAllStatsFromGears()
    {
        // 1. 플레이어 스탯 업데이트 (현재는 속도)
        UpdateSpeed();
        // UpdateArmor(); // 나중에 방어력 등이 추가되면 여기에 호출

        // 2. 모든 무기 스탯 업데이트
        foreach (WeaponBase weapon in equippedWeapons)
        {
            weapon.ApplyStatusByLevel();
        }
        Debug.Log("모든 장비 효과를 스탯에 다시 적용했습니다.");
    }


    public void TakeDamage(float damage)
    {
        if (!GameManager.instance.isLive) return;

        health -= damage;

        if (health <= 0)
        {
            health = 0;
            Die();  //캐릭터 사망 시 죽음 처리 함수 호출
        }
    }

    void Die()
    // 플레이어 사망 시 내부 처리 로직
    {  
        for (int index = 2; index < transform.childCount; index++)
        {
            transform.GetChild(index).gameObject.SetActive(false);
        }

        // 플레이어 캐릭터 사망 애니메이션 활성화
        anim.SetTrigger("dead");

        // 게임 매니저에게 '사망했음'을 알림
        GameManager.instance.GameOver();
    }

    public void Heal()
    {
        health = maxHealth;
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }
}
