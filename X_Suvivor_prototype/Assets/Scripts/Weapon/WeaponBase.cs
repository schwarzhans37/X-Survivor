using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    // 모든 무기가 가지는 핵심 데이터
    public WeaponData weaponData;
    public int currentLevel;

    // 현재 레벨에 맞는 최종 스텟;
    protected float currentDamage;
    protected int currentCount;
    protected int currentPenetration;
    protected float currentCooldown;
    protected float currentProjectileSpeed;

    protected float timer;
    protected Player player;
    protected float gearCooldownRate = 0f;  // 장비로 인한 쿨다운 감소율

    protected virtual void Awake()
    {
        player = GameManager.instance.player;
    }

    public virtual void Init(WeaponData data)
    {
        weaponData = data;
        currentLevel = 0;
        ApplyStatusByLevel();
    }

    public virtual void LevelUp()
    {
        if (currentLevel >= weaponData.damages.Length) return;

        currentLevel++;
        ApplyStatusByLevel();

    }

    // 현재 레벨에 맞춰 WeaponData에서 최종 스탯을 가져오기
    public virtual void ApplyStatusByLevel()
    {
        // 레벨 0은 base 스탯을 사용
        if (currentLevel == 0)
        {
            currentDamage = weaponData.baseDamage;
            currentPenetration = weaponData.basePenetration;
            currentCount = weaponData.baseCount;
            currentCooldown = weaponData.baseCooldown;
            currentProjectileSpeed = weaponData.baseProjectileSpeed;
        }
        else // 레벨 1 이상부터는 배열 값을 사용
        {
            // 레벨 1은 인덱스 0, 레벨 2는 인덱스 1...
            int levelIndex = currentLevel - 1;

            if (levelIndex < weaponData.damages.Length)
                currentDamage = weaponData.damages[levelIndex];

            if (levelIndex < weaponData.penetrations.Length)
                currentPenetration = weaponData.penetrations[levelIndex];

            if (levelIndex < weaponData.counts.Length)
                currentCount = weaponData.counts[levelIndex];

            if (levelIndex < weaponData.cooldowns.Length)
                currentCooldown = weaponData.cooldowns[levelIndex];

            // 투사체 속도는 현재 WeaponData에 레벨별 배열이 없으므로 일단 base 값 유지
            if (levelIndex < weaponData.projectileSpeeds.Length)
                currentProjectileSpeed = weaponData.projectileSpeeds[levelIndex];
        }

        // 장갑으로 인한 모든 무기 최종데미지 추가 적용 로직
        currentDamage += player.GetDamageBonusFromGears();
    }

    protected virtual void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        if (timer >= currentCooldown)
        {
            timer = 0f;
            Attack();
        }
    }

    protected abstract void Attack();
}
