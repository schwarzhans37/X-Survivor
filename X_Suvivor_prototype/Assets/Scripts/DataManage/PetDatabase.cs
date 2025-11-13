using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PetDatabase", menuName = "Pet/Pet Database")]
public class PetDatabase : ScriptableObject
{
    public List<PetData> allPets;

    // ID로 펫 데이터를 찾는 함수
    public PetData GetPetByID(int id)
    {
        return allPets.FirstOrDefault(p => p.id == id);
    }

    // 에디터에서 데이터를 자동으로 찾아 리스트에 채워주는 기능
    [ContextMenu("Find and Add All Pets")]
    private void FindAndAddAllPets()
    {
#if UNITY_EDITOR
        allPets = new List<PetData>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:PetDataSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            PetData petData = UnityEditor.AssetDatabase.LoadAssetAtPath<PetData>(path);
            if (petData != null)
            {
                allPets.Add(petData);
            }
        }
        Debug.Log($"Found and added {allPets.Count} pets to the database.");
        #endif
    }
}
