using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("# Weapon Number")]
    public int id;  // 무기 ID

    [Header("# Weapon Data")]
    public int prefabId;    // 무기의 프리팹 ID
    public float damage;
    public int count;   // 무기 배치 개수
    public float attackSpeed; // 공격속도

    float timer;

    Player player;

    void Awake()
    {
        player = GameManager.instance.player;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isLive) {
            return;
        }
        switch (id) {
            case 0:
                transform.Rotate(Vector3.back * attackSpeed * Time.deltaTime);
                break;
            default:
                timer += Time.deltaTime;

                if (timer > attackSpeed) {
                    timer = 0f;
                    Fire();
                }

                //attackSpeed = 0.5f;
                break;
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage * Character.Damage;
        this.count += count + Character.Count;

        if (id == 0)
            Batch();

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    public void Init(ItemData data)
    {
        // 기본 설정
        name = "Weapon" + data.itemId;
        transform.parent = player.transform;
        transform. localPosition = Vector3.zero;

        // 프로퍼티 설정
        id = data.itemId;
        damage = data.baseDamage * Character.Damage;
        count = data.baseCount + Character.Count;

        for (int index = 0; index < GameManager.instance.pool.prefabs.Length; index++) {
            if (data.projectile == GameManager.instance.pool.prefabs[index]) {
                prefabId = index;
                break;
            }
        }
        switch (id) {
            case 0:
                attackSpeed = 150 * Character.WeaponSpeed;
                Batch();
                break;
            default:
                attackSpeed = 0.5f * Character.WeaponRate;
                break;
        }

        // 손 등장 여부 설정
        Hand hand = player.hands[(int)data.itemType];
        hand.spriter.sprite = data.hand;
        hand.gameObject.SetActive(true);

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    void Batch()
    {
        for (int index = 0; index < count; index++) {
            Transform bullet;

            if (index < transform.childCount) {
                bullet = transform.GetChild(index);
            }
            else {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            bullet.localPosition = Vector3.zero;        //위치 초기화(플레이어 위치로)
            bullet.localRotation = Quaternion.identity;  //회전 초기화

            Vector3 rotVec = Vector3.forward * 360 * index / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);
            bullet.GetComponent<Bullet>().Init(damage, -100,Vector3.zero);     // -100은 여기서는 무한(-1)으로 관통하게끔 넣음
        }
    }

    void Fire()
    {
        /*
            원거리 사격용 함수. 마우스의 스크린 상 좌표를 받아 해당 방향으로 투사체를 발사함.
         */
        // 1. 마우스의 스크린 좌표 가져오기
        Vector3 mousePos = Input.mousePosition;

        // 2. 마우스의 z좌표를 카메라와의 거리로 설정해야 올바르게 2D로 변환됨
        mousePos.z = Camera.main.nearClipPlane;

        // 3. 스크린 좌표를 월드 좌표로 변환
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        // 4. 마우스 월드 좌표를 향해 발사하는 벡터 계산
        Vector2 dir = (worldMousePos - transform.position).normalized;

        Transform bullet = GameManager.instance.pool.Get(4).transform;
        bullet.position = transform.position;

        bullet.rotation = Quaternion.FromToRotation(Vector2.up, dir);
        bullet.GetComponent<Bullet>().Init(damage, count, dir);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);

    }
}
