using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    [Header("# 플레이어 캐릭터 능력치")]
    public float speed; //실 적용 이동속도
    public float baseSpeed; // 기본 이동속도
    public int health;    // 현재 체력
    public int baseMaxHealth;   // 기본 최대 체력
    public int maxHealth;   // 최종 최대 체력(기본 + 장비)
    public float invincibleTime = 3f;   // 무적 시간
    private bool isInvincible = false;   // 현재 무적인지 확인

    [Header("# 아이템 스캐너")]
    public float baseScanRange; // 기본 아이템 획득 범위    

    [Header("# 플레이어 캐릭터 물리")]
    public Vector2 inputVec;    // 캐릭터 좌표
    public ItemScanner itemScanner;     // 캐릭터의 아이템 회수범위

    [Header("# 캐릭터 이미지 렌더")]
    public Hand[] hands;

    [Header("# 캐릭터별 애니메이션 컨트롤러")]
    public RuntimeAnimatorController[] spriteAnimCon; // 플레이어 캐릭터 애니메이션 컨트롤러

    public bool isDashing { get; set; } = false;
    public Rigidbody2D rigid { get; private set; }  // 충돌 판정 계산 리지드바디
    CapsuleCollider2D capColl;
    SpriteRenderer spriter; // 스프라이트(이미지)
    Animator anim;  // 캐릭터 애니메이션

    [Header("# 펫 상호작용 설정")]
    public float interactionRange = 2f;
    public LayerMask petLayer;

    [Header("게임 중 현재 보유한 무기 목록")]
    public List<WeaponBase> equippedWeapons;

    [Header("게임 중 현재 보유한 장비 목록")]
    public List<Gear> equippedGears;


    private float speedBonusRate = 0f; // 장비를 통한 속도 증가율

    // ====== Audio ======
    [Header("# Audio")]
    public AudioClip hitClip;                 // 맞았을 때 직접 재생할 클립(선택)
    public string hitSfxKey = "PlayerHit";    // SoundData 키 재생을 쓰고 싶으면 이름 입력
    public string deathSfxKey = "Lose"; // 사망 사운드 키(선택)
    [Range(0f, 3f)] public float sfxVolume = 1f;
    [Range(0.1f, 3f)] public float sfxPitch = 1f;


    void Awake()
    // 초기화(선언)
    {
        speed = baseSpeed;      // 시작 시 이동속도 초기화
        maxHealth = baseMaxHealth;
        health = baseMaxHealth;     // 시작 시 체력 초기화
        equippedWeapons = new List<WeaponBase>();   // 무기 리스트 초기화
        equippedGears = new List<Gear>();           // 장비 리스트 초기화
        
        rigid = GetComponent<Rigidbody2D>();
        capColl = GetComponent<CapsuleCollider2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        itemScanner = GetComponent<ItemScanner>();
        if (itemScanner != null) baseScanRange = itemScanner.scanRange;
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
        UpdateMaxHealth();
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
        if (!GameManager.instance.isLive || isDashing) {
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
                Collectible collectible = target.GetComponent<Collectible>();
                if (collectible != null)
                {
                    collectible.StartSeeking();
                }
            }
        }
        CheckForPetInteraction();
    }

    void OnCollisionEnter2D(Collision2D collision)
    // 캐릭터가 몬스터와 충돌할 경우 데미지를 받게 함
    {
        // 몬스터 외의 오브젝트(아이템, 경험치 등)와 충돌했을 경우는 제외
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
                TakeDamage(1);
        }
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
        float gearBonus = GetSpeedBonusFromGears();  // 장비로 인한 +값

        float mul = 1f;
        var sp = GetComponent<PlayerMoveSpeedBuffReceiver>();
        if (sp) mul = sp.TotalMultiplier;            // 토끼 스킬 등 버프 배수

        speed = (baseSpeed + gearBonus) * mul;
    }

    // 최대체력 최종 계산: 기본 + 장비 + (토끼 임시 보너스)
    private void UpdateMaxHealth()
    {
        int previousMaxHealth = maxHealth;

        int gearBonus = 0;
        foreach (Gear gear in equippedGears)
        {
            if (gear.type == GearData.GearType.Armor)
            {
                gearBonus += (int)gear.rate;
            }
        }

        // ★ 임시 최대체력 보너스(토끼 스킬 60초 하트)
        int tempBonus = 0;
        var temp = GetComponent<PlayerTempMaxHealthBuffReceiver>();
        if (temp) tempBonus = temp.TotalBonus;

        maxHealth = baseMaxHealth + gearBonus + tempBonus;

        if (maxHealth > previousMaxHealth)
        {
            int inc = maxHealth - previousMaxHealth;
            health = Mathf.Min(health + inc, maxHealth);  // 늘어난 하트만큼 회복
            Debug.Log($"최대 체력이 {inc} 증가 → 현재 체력도 회복");
        }
        else
        {
            health = Mathf.Clamp(health, 0, maxHealth);   // 감소 시 초과분 클램프
        }
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
        UpdateMaxHealth();
        UpdateItemScannerRange();

        // 2. 모든 무기 스탯 업데이트
        foreach (WeaponBase weapon in equippedWeapons)
        {
            weapon.ApplyStatusByLevel();
        }
        Debug.Log("모든 장비 효과를 스탯에 다시 적용했습니다.");
    }


    public void TakeDamage(int damage)
    {
        if (isInvincible || !GameManager.instance.isLive) return;

        health -= damage;
        Debug.Log("플레이어가 공격받았습니다.");

        if (health <= 0)
        {
            health = 0;
            Die();
        }
        else
        {
            // ★ 피격 SFX — 클립이 있으면 클립, 없으면 키로
            if (hitClip) PlaySfxClip(hitClip);
            else if (!string.IsNullOrEmpty(hitSfxKey)) PlaySfxKey(hitSfxKey);

            StartCoroutine(InvincibleRoutine(invincibleTime));
            Debug.Log("플레이어가 생명력 1개를 잃고 3초간 무적이 됩니다.");
        }
    }

    public void BecomeInvincible(float duration)
    {
        // 이미 무적상태가 아니라면 코루틴 실행
        if (!isInvincible)
        {
            StartCoroutine(InvincibleRoutine(duration));
        }
    }

    IEnumerator InvincibleRoutine(float duration)
    {
        isInvincible = true;
        capColl.isTrigger = true;

        // 1. 몬스터와의 충돌을 무시하기 위해 레이어 변경
        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");

        // 2. 깜빡임 효과
        Color originalColor = spriter.color;
        float blinkInterval = 0.2f;  //깜빡이는 간격
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 반투명하게
            spriter.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            yield return new WaitForSeconds(blinkInterval / 2);

            // 원래대로
            spriter.color = originalColor;
            yield return new WaitForSeconds(blinkInterval / 2);

            elapsedTime += blinkInterval;
        }

        // 3. 무적 시간이 끝나면 원상복구
        gameObject.layer = originalLayer;
        spriter.color = originalColor;
        capColl.isTrigger = false;

        isInvincible = false;
    }

    private void CheckForPetInteraction()
    {
        // 'E'키를 눌렀을 때만 실행
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 내 주변 interactionRange 반경 안에 있는 펫 콜라이더를 찾음
            Collider2D[] petsInRange = Physics2D.OverlapCircleAll(transform.position, interactionRange, petLayer);

            foreach (var petCollider in petsInRange)
            {
                PetController pet = petCollider.GetComponent<PetController>();

                // 펫을 찾았고, 그 펫이 죽은 상태라면
                if (pet != null && pet.isDead)
                {
                    // 펫의 Revive() 함수를 호출
                    pet.Revive();
                    break;
                }
            }
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
        Debug.Log("플레이어가 사망했습니다.");
    }

    void UpdateItemScannerRange()
    {
        if (!itemScanner) return;

        float rangeBonus = 0f;

        // 모든 장착된 장비 중 'Magnet' 타입의 효과를 합산
        foreach (Gear gear in equippedGears)
        {
            if (gear.type == GearData.GearType.Magnet)
            {
                rangeBonus += gear.rate;
            }
        }

        // 최종 아이템 획득 범위 계산
        itemScanner.scanRange = baseScanRange + rangeBonus;
        Debug.Log($"자석 효과 적용! 현재 아이템 획득 범위: {itemScanner.scanRange}");
    }

    public void Heal()
    {
        health = maxHealth;
    }

    void OnMove(InputValue value)
    {
        if (isDashing)
        {
            inputVec = Vector2.zero;
            return;
        }

        inputVec = value.Get<Vector2>();
    }

    // ★ 이동속도 버프 수치가 바뀔 때(토끼 스킬에서 SendMessage로 호출)
    void OnSpeedBuffChanged(float _)
    {
        UpdateSpeed();
    }

    // ★ 임시 최대체력(토끼 스킬)이 바뀔 때
    void OnTempMaxHealthChanged(int _)
    {
        UpdateMaxHealth();
    }

    // ★ 임시 하트 만료 시 초과 체력 컷(토끼 스킬에서 호출)
    void ClampHealthToMax()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    // ===== Audio helpers =====
    void PlaySfxClip(AudioClip clip)
    {
        if (!clip) return;
        if (AudioManager.instance) AudioManager.instance.PlaySfx(clip);
    }
    void PlaySfxKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (AudioManager.instance) AudioManager.instance.PlaySfx(key, sfxVolume, sfxPitch);
    }
}
