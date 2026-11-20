using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using Newtonsoft.Json;

public struct InventoryContentsParam
{
    public enum EType
    {
        Refresh,
    }

    public EType Type;

    public InventoryContentsParam(EType type)
    {
        Type = type;
    }
}

public class InventoryContents : IManagableContents, GlobalEvent<InventoryContentsParam>.IManagableHandler
{
    private GlobalEvent<InventoryContentsParam> m_GlobalEvent = new();
    //TODO: 개별 EItemType이 아닌 Inventory Category로 분류 고려
    private Dictionary<EItemType, List<Item>> m_OwnedItemsDic = new();


    public void Initialize()
    {
        InitInventory();
        LoadEquip();
        LoadStackable();
    }

    public void UnInitialize()
    {
        m_OwnedItemsDic.Clear();
    }

    public void OnUpdate(float deltaTime)
    {
        m_GlobalEvent.OnUpdate();
    }

    public void RegisterHandler(GlobalEvent<InventoryContentsParam>.IEventHandler handler)
    {
        m_GlobalEvent.RegisterHandler(handler);
    }

    public void UnRegisterHandler(GlobalEvent<InventoryContentsParam>.IEventHandler handler)
    {
        m_GlobalEvent.UnRegisterHandler(handler);
    }

    public void Send(InventoryContentsParam parameter)
    {
        m_GlobalEvent.Send(parameter);
    }

    private void InitInventory()
    {
        foreach (EItemType itemType in Enum.GetValues(typeof(EItemType)))
        {
            int i = (int)itemType;
            if (i == 0 || itemType == EItemType.Currency || itemType == EItemType.Exp
                || (i & (i - 1)) != 0) //None, 재화, 경험치, 카테고리 제외
                continue;
            m_OwnedItemsDic[itemType] = new List<Item>();
        }
    }

    private void LoadEquip()
    {
        var jsonData = DataManager.Instance.GetJsonData(GlobalVariable.CONTENTS_INVENTORY_EQUIP_KEY);
        if (jsonData == null)
            return;

        var weaponDic = JsonConvert.DeserializeObject<Dictionary<long, string>>(jsonData["weapons"].ToJson());
        AddEquipItem(weaponDic);
        var wearableDic = JsonConvert.DeserializeObject<Dictionary<long, string>>(jsonData["wearables"].ToJson());
        AddEquipItem(wearableDic);
    }

    private void LoadStackable()
    {
        var jsonData = DataManager.Instance.GetJsonData(GlobalVariable.CONTENTS_INVENTORY_STACK_KEY);
        if (jsonData == null)
            return;

        var dic = JsonConvert.DeserializeObject<Dictionary<int, int>>(jsonData["items"].ToJson());
        AddStackableItem(dic);
    }

    public void AddStackableItem(Dictionary<int, int> items)
    {
        foreach (var p in items)
        {
            if (ItemID.IsCurrency(p.Key))
            {
                UserManager.Instance.AddBalance(p.Key, p.Value);
            }
            else //stackable
            {
                var itemData = BDatabase.ItemTable.ItemDataDic[p.Key];
                var list = m_OwnedItemsDic[itemData.Base.Type];
                var ownedItem = list.Find(i => i.Info.Base.ItemID == p.Key);
                if (ownedItem != null)
                {
                    ownedItem.CurCount += p.Value;
                }
                else
                {
                    Item newItem = itemData is ConsumableItemData consumableData
                        ? new ConsumableItem(consumableData, p.Value)
                        : new Item(itemData, p.Value);
                    list.Add(newItem);
                }
            }
        }
    }

    public void AddEquipItem(Dictionary<long, string> items)
    {
        foreach (var p in items)
        {
            var parts = p.Value.Split('|');
            int itemID = int.Parse(parts[0]);
            var itemData = BDatabase.ItemTable.GetItemData(itemID);
            var list = m_OwnedItemsDic[itemData.Base.Type];
            if (ItemID.IsWearable(itemID))
            {
                var newItem = new WearableItem(p.Key, parts, itemData as WearableItemData);
                list.Add(newItem);
            }
            else // weapon
            {
                var newItem = new WeaponItem(p.Key, parts, itemData as WeaponItemData);
                list.Add(newItem);
            }
        }
    }

    public void AddWeaponItem(long instanceID, int itemID)
    {
        var itemData = BDatabase.ItemTable.GetItemData(itemID);
        var list = m_OwnedItemsDic[itemData.Base.Type];
        string[] parts = { itemID.ToString(), "0", ItemID.IsRarityLocked(itemID).ToString(), "0" };
        list.Add(new WeaponItem(instanceID, parts, itemData as WeaponItemData));
    }

    public void RemoveItem(Item item, int count = 1)
    {
        var items = m_OwnedItemsDic[item.Info.Base.Type];
        if (item is EquippableItem)
        {
            items.Remove(item);
        }
        else
        {
            var targetItem = items.Find(i => i.Info.Base.ItemID == item.Info.Base.ItemID);
            if (targetItem == null)
            {
                HDebug.LogError($"[Inventory] RemoveItem failed. ItemID not found: {item.Info.Base.ItemID}");
                return;
            }
            if (targetItem.CurCount >= count)
            {
                targetItem.CurCount -= count;
                if (targetItem.CurCount == 0)
                    items.Remove(targetItem);
            }
        }
    }

    public void RemoveStackableItem(Dictionary<int, int> items)
    {
        foreach (var p in items)
        {
            if (ItemID.IsCurrency(p.Key))
            {
                UserManager.Instance.SpendBalance(p.Key, p.Value);
            }
            else
            {
                var itemData = BDatabase.ItemTable.ItemDataDic[p.Key];
                var list = m_OwnedItemsDic[itemData.Base.Type];
                var ownedItem = list.Find(i => i.Info.Base.ItemID == p.Key);
                if (ownedItem == null)
                    continue;
                ownedItem.CurCount -= p.Value;
                if (ownedItem.CurCount <= 0)
                    list.Remove(ownedItem);
            }
        }
    }

    public void ConsumeItem(List<TransactionItemSpec> items)
    {
        if (items == null || items.Count == 0)
            return;

        var consumeDic = items.BuildStackableDic();
        RemoveStackableItem(consumeDic);
        Send(new InventoryContentsParam(InventoryContentsParam.EType.Refresh));

        SendConsumeStackableRequest(items, consumeDic).Forget();
    }

    public void ConsumeItem(List<long> equipInstanceIDs)
    {
        if (equipInstanceIDs == null || equipInstanceIDs.Count == 0)
            return;

        var removedItems = new List<EquippableItem>();
        foreach (var instanceID in equipInstanceIDs)
        {
            var item = FindEquippableItem(instanceID);
            if (item == null)
            {
                HDebug.LogError($"[Inventory] ConsumeItem failed. InstanceID not found: {instanceID}");
                continue;
            }
            RemoveItem(item);
            removedItems.Add(item);
        }
        if (removedItems.Count == 0)
            return;

        Send(new InventoryContentsParam(InventoryContentsParam.EType.Refresh));

        SendConsumeEquipRequest(equipInstanceIDs, removedItems).Forget();
    }

    private async UniTask SendConsumeStackableRequest(List<TransactionItemSpec> items, Dictionary<int, int> consumeDic)
    {
        BackendReturnObject bro = null;
        var param = new Param();
        param.Add("items", JsonConvert.SerializeObject(items));

        SendQueue.Enqueue(Backend.BFunc.InvokeFunction, "ConsumeItem", param, callback =>
        {
            bro = callback;
        });
        await UniTask.WaitUntil(() => bro != null);

        var result = BFuncResponseHandler.Parse(bro, "ConsumeItem/Stackable");
        if (result == null)
        {
            AddStackableItem(consumeDic);
            Send(new InventoryContentsParam(InventoryContentsParam.EType.Refresh));
        }
    }

    private async UniTask SendConsumeEquipRequest(List<long> equipInstanceIDs, List<EquippableItem> removedItems)
    {
        BackendReturnObject bro = null;
        var param = new Param();
        param.Add("equipInstanceIDs", JsonConvert.SerializeObject(equipInstanceIDs));

        SendQueue.Enqueue(Backend.BFunc.InvokeFunction, "ConsumeItem", param, callback =>
        {
            bro = callback;
        });
        await UniTask.WaitUntil(() => bro != null);

        var result = BFuncResponseHandler.Parse(bro, "ConsumeItem/Equip");
        if (result == null)
        {
            RestoreEquipItems(removedItems);
            Send(new InventoryContentsParam(InventoryContentsParam.EType.Refresh));
        }
    }

    private void RestoreEquipItems(List<EquippableItem> items)
    {
        foreach (var item in items)
        {
            m_OwnedItemsDic[item.Info.Base.Type].Add(item);
        }
    }

    public PooledList<Item> GetCategoryItems(EItemType category)
    {
        var pooledList = new PooledList<Item>();
        var resultItems = pooledList.List;

        foreach (var itemType in m_OwnedItemsDic.Keys)
        {
            if ((category & itemType) != 0)
            {
                var items = m_OwnedItemsDic[itemType];
                resultItems.AddRange(items);
            }
        }
        return pooledList;
    }

    public PooledList<Item> GetTypeItems(EItemType type)
    {
        var pooledList = new PooledList<Item>();
        var resultItems = pooledList.List;
        var items = m_OwnedItemsDic[type];
        resultItems.AddRange(items);

        return pooledList;
    }

    public int GetMaxCategorySlotCount()
    {
        int maxCount = 0;
        EItemType[] categories = { EItemType.Weapon, EItemType.Wearable, EItemType.Consumable,
            EItemType.Material, EItemType.Valuable };
        foreach (var category in categories)
        {
            int total = 0;
            foreach (var itemType in m_OwnedItemsDic.Keys)
            {
                if ((category & itemType) != 0)
                    total += m_OwnedItemsDic[itemType].Count;
            }
            if (total > maxCount)
                maxCount = total;
        }
        return maxCount;
    }

    public int GetMaxSlotItemCount(EItemType type)
    {
        int maxCount = 0;
        foreach (var itemType in m_OwnedItemsDic.Keys)
        {
            int total = 0;
            if ((type & itemType) != 0)
            {
                total += m_OwnedItemsDic[itemType].Count;
            }
            if (total > maxCount)
                maxCount = total;
        }

        return maxCount;
    }

    public Dictionary<int, int> GetStackableItemDic()
    {
        var result = new Dictionary<int, int>();
        foreach (var pair in m_OwnedItemsDic)
        {
            if ((pair.Key & EItemType.Equip) != 0)
                continue;
            foreach (var item in pair.Value)
                result[item.Info.Base.ItemID] = item.CurCount;
        }
        return result;
    }

    public Item FindItem(EItemType type, int itemID)
    {
        var list = m_OwnedItemsDic[type];
        foreach (var item in list)
        {
            if (item.Info.Base.ItemID == itemID)
                return item;
        }

        return null;
    }

    private EquippableItem FindEquippableItem(long instanceID)
    {
        foreach (var pair in m_OwnedItemsDic)
        {
            if ((pair.Key & EItemType.Equip) == 0)
                continue;

            foreach (var item in pair.Value)
            {
                if (item is EquippableItem eItem && eItem.InstanceID == instanceID)
                    return eItem;
            }
        }

        return null;
    }

    public EquippableItem FindEquippableItem(EItemType type, long instanceID)
    {
        var list = m_OwnedItemsDic[type];
        foreach (var item in list)
        {
            var eItem = item as EquippableItem;
            if (eItem.InstanceID == instanceID)
                return eItem;
        }

        return null;
    }

}
