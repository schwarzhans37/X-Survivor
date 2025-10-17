public enum PetRarity { Normal, Rare, Epic, Unique, Legendary }

public static class PetRarityUtil
{
    public static int Index(this PetRarity r) =>
        r == PetRarity.Normal ? 0 :
        r == PetRarity.Rare ? 1 :
        r == PetRarity.Epic ? 2 :
        r == PetRarity.Unique ? 3 : 4;
}
