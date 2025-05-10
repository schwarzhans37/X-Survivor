using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;  // 무기 ID
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

                attackSpeed = 0.5f;
                break;
        }

        // .. 테스트용 레벨업
        if (Input.GetButtonDown("Jump")) {
            LevelUp(20, 5);
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count  += count;

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
        damage = data.baseDamage;
        count = data.baseCount;

        for (int index = 0; index < GameManager.instance.pool.prefabs.Length; index++) {
            if (data.projectile == GameManager.instance.pool.prefabs[index]) {
                prefabId = index;
                break;
            }
        }
        switch (id) {
            case 0:
                attackSpeed = 150;
                Batch();
                break;
            default:
                attackSpeed = 0.5f;
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
            bullet.GetComponent<Bullet>().Init(damage, -1,Vector3.zero);     // -1은 여기서는 무한(-1)으로 관통하게끔 넣음
        }
    }

    void Fire() 
    {
        if (!player.scanner.nearestTarget)
            return;
        
        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = targetPos - transform.position;
        dir = dir.normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.GetComponent<Bullet>().Init(damage, count, dir);
    }
}
