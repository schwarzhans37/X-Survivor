using System.Collections.Generic;
using UnityEngine;

public class MonsterDatabase : MonoBehaviour
{
    public List<MonsterData> monsters;
    public RuntimeAnimatorController[] animCon;

    void Awake()
    {
        LoadMonsterData();
        Debug.Log("[MonsterDatabase] Awake에서 데이터 로드 완료");
    }

    void LoadMonsterData()
    {
        TextAsset csv = Resources.Load<TextAsset>("MonsterData");
        string[] lines = csv.text.Split('\n');
        monsters = new List<MonsterData>();

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더라서 건너뜀
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] fields = lines[i].Split(',');

            for (int f = 0; f < fields.Length; f++)
            {
                fields[f] = fields[f].Trim();
            }

            if (fields.Length < 10) continue;

            MonsterData data = new MonsterData
            {
                Id = int.Parse(fields[0]),
                Name = fields[1],
                Category = fields[2],
                Maxhealth = int.Parse(fields[3]),
                AttackPower = int.Parse(fields[4]),
                AttackRange = int.Parse(fields[5]),
                Speed = float.Parse(fields[6]),
                Size = float.Parse(fields[7]),
                isElite = bool.Parse(fields[8]),
                isBoss = bool.Parse(fields[9])
            };

            monsters.Add(data);
            Debug.Log($"[MonsterDatabase] ID {data.Id} 로딩 완료: {data.Name}");
        }

        Debug.Log($"[MonsterDatabase] 몬스터 총 {monsters.Count}개 로드 완료");
    }


    public MonsterData GetByID(int id)
    {
        MonsterData result = monsters.Find(m => m.Id == id);
        if (result == null)
            Debug.LogError($"[MonsterDatabase] ID {id}에 해당하는 몬스터를 찾을 수 없습니다.");
        return result;
    }

    public List<MonsterData> GetByCategory(string category)
    {
        return monsters.FindAll(m => m.Category == category);
    }
}
