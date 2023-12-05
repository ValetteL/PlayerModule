using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlayerModule;

/*public class Authentication
{
    [JsonPropertyName("id")] public int ID { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("reward")] public int Reward { get; set; }

    [JsonPropertyName("progress_required")]
    public int ProgressRequired { get; set; }

    [JsonPropertyName("progress_per_minute")]
    public int ProgressPerMinute { get; set; }
}*/

[Serializable]
public class PlayerConf
{
    public PlayerConf()
    {

    }

    public PlayerConf(string username, bool creationDone, bool tutorialDone)
    {
        Username = username;
        CreationDone = creationDone;
        TutorialDone = tutorialDone;
    }

    /*[JsonProperty("username")]*/ public string? Username { get; set; }
    
    /*[JsonProperty("creationDone")]*/ public bool CreationDone { get; set; }

    /*[JsonProperty("tutorialDone")]*/ public bool TutorialDone { get; set; }
}

public class Body
{
    public Body()
    {
        
    }
    
    public bool isMaleGender;
    public int hairIndex;
    public int moustacheIndex;
    public int beardIndex;
    public string hairColor;
    public string skinColor;
    public string chestColor;
    //public string beltColor;
    public string shortColor;

    public override string ToString()
    {
        return "Body : [ \n" +
               "isMaleGender : " + isMaleGender + "\n" +
               "hairIndex : " + hairIndex + "\n" +
               "moustacheIndex : " + moustacheIndex + "\n" +
               "beardIndex : " + beardIndex + "\n" +
               "hairColor : " + hairColor + "\n" +
               "skinColor : " + skinColor + "\n" +
               "chestColor : " + chestColor + "\n" +
               //"beltColor" : " + beltColor + "\n" +
               "shortColor : " + shortColor + "\n" +
               "]";
    }
}

public class InitialisationResult
{
    [JsonConverter(typeof(StringEnumConverter))]
    public InitialisationStage Stage;
        
    public string Value;
}

public enum InitialisationStage
{
    Body,
    Progression,
    Wallet,
    Equipment,
    Inventory,
    CharacterStats,
    Character
}

public class PlayerProgression
{
    public int Level = 1;
    public int Experience = 0;
    public int MaxExperience
    {
        get
        {
            int solveForRequiredXp = 0;

            for (int levelCycle = 1; levelCycle <= Level; levelCycle++) {
                solveForRequiredXp += (int)Math.Floor(levelCycle + 300 * Math.Pow(2, levelCycle / 14));
            }

            return solveForRequiredXp / 4;            
        }
    }

    public int StatPoint = 0;
    public int Ranking = 150;
}

[Serializable]
public class Gear
{
    [JsonConverter(typeof(GearConverter))]
    public CharacterEquipment[] Equipments = new CharacterEquipment[Enum.GetNames(typeof(EquipmentSlot)).Length];

    public Gear()
    {
        for(int i = 0; i < Equipments.Length; i++)
        {
            Equipments[i] = new CharacterEquipment()
            {
                item = new Equipment()
                {
                    equipmentSlot = (EquipmentSlot) i,
                    itemType = ItemType.Equipment
                }
            };
        }
    }
    
    public override string ToString()
    {
        var result = "[ \n ";
        foreach (CharacterEquipment characterEquipment in Equipments) result += "\t " + characterEquipment + "\n";
        result += "]";

        return result;
    }
}

[Serializable]
public class Stat
{
    public int value;
        
    [JsonConverter(typeof(StringEnumConverter))]
    public StatType statType;
    
    private List<int> modifiers = new List<int>();

    public string GetStatType()
    {
        return statType.ToString();
    }
        
    public void SetStatType(StatType type)
    {
        statType = type;
    }
        
    public int GetValue()
    {
        int finalValue = value;
        modifiers.ForEach(modifier => finalValue += modifier);
        return finalValue;
    }

    public void SetValue(int val)
    {
        value = val;
    }
        
    public void AddModifiers(int modifier)
    {
        if(modifier != 0)
        {
            modifiers.Add(modifier);
        }
    }

    public void RemoveModifier(int modifier)
    {
        if(modifier != 0)
        {
            modifiers.Remove(modifier);
        }
    }

    public override string ToString()
    {
        return "[ \n " +
               "\t statType : " + statType + "\n" +
               "\t value : " + GetValue() + "\n" +
               "]";
    }
}
    
public enum StatType
{
    Health,
    Armor,
    Strength,
    Agility,
    Intelligence,
    Luck
}

[Serializable]
public class Equipment : Item
{
    public List<Stat> stats;
        
    public EquipmentSlot equipmentSlot;

    public int spriteId;

    public int requiredLevel;

    public int GetStatValue(StatType statType)
    {
        return stats[(int)statType].GetValue();
    }

    public List<Stat> GetStats()
    {
        return stats;
    }

    public override string ToString()
    {
        var result = "equipment : [ \n " +
                     base.ToString() +
                     "equipmentSlot : " + equipmentSlot + "\n" +
                     //"spritePath : " + (sprites.Count != 0 ? AssetDatabase.GetAssetPath(sprites[0]) : null) + "\n" +
                     "spriteId : " + spriteId + "\n" +
                     "stats : [ \n";

        if (stats != null) foreach(Stat stat in stats) result += stat + "\n";

        result += "] \n" + 
                  "]";

        return result;
    }
}

public enum EquipmentSlot
{
    Helmet,
    Chest,
    Pants,
    Weapon,
    OffHand,
}

public enum WeaponType
{
    Hand = 0,
    Sword = 1,
    Staff = 2,
    Dagger = 3,
    MagicalDagger = 4
}

public enum OffHandType
{
    Hand = 0,
    Shield = 1,
    Book = 2,
    Dagger = 3
}

[Serializable]
public class CharacterEquipment : BaseCharacterItem<Equipment>
{
    public List<Stat> modifiers = new();

    public Stat GetStat(StatType statType)
    {
        Stat mergedStat = new()
        {
            statType = statType,
            value = item.GetStatValue(statType) 
                    + modifiers.Find(s => s.statType == statType).GetValue()
        };
        return mergedStat;
    }
    
    public List<Stat> GetStats()
    {
        List<Stat> mergedStats = item.GetStats().ToList();

        foreach (Stat stat in GetModifiers())
        {
            int existingStatIndex = mergedStats.FindIndex(s => s.statType == stat.statType);
            
            if (existingStatIndex != -1)
            {
                mergedStats[existingStatIndex].value += stat.value;
            }
            else
            {
                mergedStats.Add(stat);
            }
        }

        return mergedStats;
    }

    public Stat GetModifier(StatType statType)
    {
        return modifiers.Find(s => s.statType == statType);
    }

    public List<Stat> GetModifiers()
    {
        List<Stat> mergedStats = new List<Stat>();

        if (modifiers != null){
            foreach (Stat modifier in modifiers)
            {
                if (!mergedStats.Exists(s => s.statType == modifier.statType))
                {
                    mergedStats.Add(modifier);
                }
            }
        }

        return mergedStats;
    }

    public override string ToString()
    {
        var result = "characterEquipment: [ \n " +
                        "\t id : " + id + "\n" +
                        "\t " + item + "\n" +
                        "\t modifiers : [ \n";

        modifiers.ForEach(stat => result += "\t " + stat + "\n");
        
        result += "\t ] \n" +
                "]";

        return result;
    }
}

public interface IBaseCharacterItem
{
    public abstract string Id { get; }
    public abstract Item Item { get; }
        
    public abstract int Amount { get; set; }

    /*public abstract void Use();
    public abstract void Sell();*/
}

[Serializable]
public abstract class BaseCharacterItem<T> : IBaseCharacterItem where T : Item, new()
{
    public string id;
    public T item;
    public int amount = 1;

    string IBaseCharacterItem.Id => id;
        
    Item IBaseCharacterItem.Item => item;

    int IBaseCharacterItem.Amount
    {
        get => amount;
        set => amount = value;
    }

    /*public abstract void Use();
    public abstract void Sell();*/
}

[Serializable]
public class Item
{
    public string? ID = null;
    public string name = "New Item";
    //[JsonIgnore]
    //public Sprite icon;
    //public bool isDefaultItem = true;
    public int cost = 0;
    public ItemType itemType = ItemType.Item;
    public ItemQuality itemQuality = ItemQuality.Normal;

    public override string ToString()
    {
        return "item : [ \n " +
               "name : " + name + "\n" +
               //"isDefaultItem : " + isDefaultItem + "\n" +
               /*"iconPath : " + AssetDatabase.GetAssetPath(icon) + "\n" +*/
               "cost : " + cost + "\n" +
               "itemType : " + itemType + "\n" +
               "itemQuality : " + itemQuality + "\n" +
               "]";
    }
}
    
[JsonConverter(typeof(StringEnumConverter))]
public enum ItemType
{
    [EnumMember(Value = "item")]
    Item,
    [EnumMember(Value = "lootBox")]
    LootBox,
    [EnumMember(Value = "equipment")]
    Equipment,
}

[System.Text.Json.Serialization.JsonConverter(typeof(StringEnumConverter))]
public enum ItemQuality
{
    [EnumMember(Value = "normal")]
    Normal,
    [EnumMember(Value = "rare")]
    Rare,
    [EnumMember(Value = "epic")]
    Epic,
    [EnumMember(Value = "legendary")]
    Legendary
}

[Serializable]
public class Currency
{
    public CurrencyType currencyType;

    public int amount = 0;

    public override string ToString()
    {
        return "currency : { \n" +
               " type : " + currencyType + "\n" +
               " amount : " + amount + "\n" +
               "}";

    }
}

public enum CurrencyType
{
    Gold,
    Crystal
}

public class Wallet
{
    public Currency[] Currencies = new Currency[Enum.GetNames(typeof(CurrencyType)).Length];

    public Currency GetCurrency(CurrencyType currencyType)
    {
        return Currencies[(int)currencyType];
    }
        
    public int GetAmount(CurrencyType currencyType)
    {
        return Currencies[(int)currencyType].amount;
    }

    public void SpendCurrency(CurrencyType currencyType, int amount)
    {
        Currencies[(int)currencyType].amount -= amount;
    }

    public void AddCurrency(CurrencyType currencyType, int amount)
    {
        Currencies[(int)currencyType].amount += amount;
    }
}

[Serializable]
public class Inventory
{
    public List<IBaseCharacterItem> Items = new();
    public int space = 28;

    public void AddItem(IBaseCharacterItem characterItem)
    {
        if (characterItem.Item.itemType == ItemType.Equipment)
        {
            if (Items.Count >= space)
            {
                //Debug.Log("Not enough space.");
                return;
            }

            Items.Add(characterItem);
        }
        else
        {
            var itemAlreadyInInventory = false;
            foreach (IBaseCharacterItem inventoryItem in Items)
            {
                //Debug.Log("inventory Item id : " + inventoryItem.Item.ID + " || character item id : " + characterItem.Item.ID);
                
                if (inventoryItem.Item.ID != characterItem.Item.ID) continue;
                inventoryItem.Amount += characterItem.Amount;
                itemAlreadyInInventory = true;
            }

            if (!itemAlreadyInInventory)
            {
                Items.Add(characterItem);
            }
        }
    }

    public void RemoveItem(IBaseCharacterItem item)
    {
        int index;
        
        //if (!Items.Contains(item)) return;
        if (item.Item.itemType is ItemType.Equipment) 
        {
            index = Items.FindIndex(characterItem => characterItem.Id == item.Id); 
        } 
        else 
        {
            index = Items.FindIndex(characterItem => characterItem.Item.ID == item.Item.ID);
        }

        if (index != -1)
        {
            Items[index].Amount--;

            if (Items[index].Amount < 1)
            {
                Items.RemoveAt(index);
            }
        }
        else
        {
            //Debug.LogError(item.Item.name + " not found in inventory while removing");
        }
    }
    
    public override string ToString()
    {
        string result = "inventory : { \n" +
                        " items : [";

        foreach (IBaseCharacterItem item in Items)
        {
            result += item + "\n";
        }

        result += "] \n" +
                  "space : " + space + "\n" +
                  "]}";

        return result;
    }
}