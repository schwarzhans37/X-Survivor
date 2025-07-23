/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PetDatabase : MonoBehaviour
{
    public static PetDatabase instance; // 싱글톤으로 만들어 외부 접근 가능하게 만들기
    public List<PetData> pets = new List<PetData>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        LoadPetData();
    }

    public void LoadPetData()
    {
        TextAsset csv = Resources.Load<TextAsset>("PetData");

        pets.Clear();

        StringReader reader = new StringReader(csv.text);
        bool isFirstLine = true;

        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            if (isFirstLine) { isFirstLine = false; continue; }

            string[] values = line.Split(',');

            if (values.Length < 7) continue;

            PetData pet = new PetData
            {
                Id = int.Parse(values[0].Trim()),
                PetType = values[1].Trim(),
                Grade = values[2].Trim(),
                HasSkill = bool.Parse(values[3].Trim()),
                BuffDescription = values[4].Trim(),
                SkillDescription = values[5].Trim(),
                SkillCount = int.Parse(values[6].Trim())
            };

            pets.Add(pet);
        }

        Debug.Log($"[PetDatabase] {pets.Count}개의 펫 데이터를 불러왔습니다.");
    }

    // ID를 통해 펫 데이터 찾기
    public PetData GetByID(int id)
    {
        return pets.Find(p => p.Id == id);
    }
}
*/