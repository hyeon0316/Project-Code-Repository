using BackEnd;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public interface IRealPurchaseHandler
{
    public void OnPurchaseSuccess(string productID);
    public void OnPurchaseFailed(string productID, string error);
}

public interface IIAPGrantHandler
{
    public void OnIAPGranted(string productID, TransactionResult result);
}

public class IAPManager : Singleton<IAPManager>
{
    public static readonly string MONTHLY_PASS = "monthly_pass";
    public static readonly string BATTLE_PASS = "battle_pass";
    private const int RETRY_DELAY_MS = 5000;
    private const int RETRY_MAX = 3;

    private Dictionary<string, string> m_LocalizePrices = new();
    private Dictionary<string, ShopEntry> m_IAPEntries = new();
    private List<PendingOrder> m_QueuedOrders = new();
    private Dictionary<string, int> m_ProcessingTransactions = new();
    private StoreController m_StoreController;
    private IRealPurchaseHandler m_PurchaseHandler;
    private IIAPGrantHandler m_GrantHandler;
    private bool m_Initialized;

    public async UniTask Init()
    {
        if (m_Initialized)
            return;

        m_Initialized = true;
        m_StoreController = UnityIAPServices.StoreController();

        m_StoreController.OnProductsFetched += HandleProductsFetched;
        m_StoreController.OnProductsFetchFailed += HandleProductsFetchFailed;
        m_StoreController.OnPurchasesFetched += HandlePurchasesFetched;
        m_StoreController.OnPurchasePending += HandlePurchasePending;
        m_StoreController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
        m_StoreController.OnPurchaseFailed += HandlePurchaseFailed;

        await m_StoreController.Connect();

        var products = new List<ProductDefinition>();
        foreach (var pair in BDatabase.ShopEntryDic)
        {
            foreach (var p in pair.Value)
            {
                if (!p.IsIAP)
                    continue;

                m_IAPEntries[p.ProductID] = p;
                products.Add(new ProductDefinition(p.ProductID, ProductType.Consumable));
            }
        }
        m_StoreController.FetchProducts(products);
    }

    public void SetGrantHandler(IIAPGrantHandler handler)
    {
        m_GrantHandler = handler;
        if (m_GrantHandler == null)
            return;

        var queued = new List<PendingOrder>(m_QueuedOrders);
        m_QueuedOrders.Clear();
        foreach (var order in queued)
        {
            HandlePurchasePending(order);
        }
    }

    private void HandlePurchaseFailed(FailedOrder order)
    {
        string productID = GetProductID(order);
        HDebug.LogError($"[IAP] Failed purchase. {productID}, reason={order.FailureReason}");
        NotifyFailed(productID, order.FailureReason.ToString());
    }

    private void HandlePurchasePending(PendingOrder order)
    {
        if (m_GrantHandler == null)
        {
            m_QueuedOrders.Add(order);
            return;
        }

        string transactionID = order.Info.TransactionID;
        if (!m_ProcessingTransactions.TryAdd(transactionID, 0))
            return;

        string productID = GetProductID(order);

#if UNITY_EDITOR
        CompleteGrant(order, productID, CreateEditorResult(productID));
#elif UNITY_ANDROID
        SendValidateRequest(order, productID);
#endif
    }

    private void SendValidateRequest(PendingOrder order, string productID)
    {
        string transactionID = order.Info.TransactionID;
        var param = new Param();
        Backend.Receipt.FromJson(order.Info.Receipt, out _, out string token);
        param.Add("productID", productID);
        param.Add("token", token);
        param.Add("inventoryConfigTableID", BDatabase.TableIDDic["InventoryConfigTable"]);
        param.Add("shopTableID", BDatabase.TableIDDic["ShopTable"]);

        UIManager.Instance.SetLoadingUI(ELoadingType.Center, true);

        SendQueue.Enqueue(Backend.BFunc.InvokeFunction, "Shop", param, callback =>
        {
            if (TryConsumeRetry(transactionID, callback))
            {
                RetryLater(order, productID).Forget();
                return;
            }

            UIManager.Instance.SetLoadingUI(ELoadingType.Center, false);

            var result = BFuncResponseHandler.Parse(callback, "Shop/ValidateReceipt");
            if (result == null)
            {
                m_ProcessingTransactions.Remove(transactionID);
                NotifyFailed(productID, Localize.Get("IAP_PAYMENT_ERROR"));
                return;
            }

            // 서버가 payload를 한 번 직렬화해서 보내므로 success는 JSON 문자열이다. 필드 접근 전에 다시 파싱한다.
            var payload = result.IsString ? JsonMapper.ToObject(result.ToString()) : result;

            // 서버가 이미 지급한 구매. 이번 세션에 응답이 유실된 주문이 아니면
            // 로그인 시 로드한 데이터에 반영돼 있으므로 Confirm만 한다.
            if (payload.ContainsKey("alreadyGranted") && m_ProcessingTransactions[transactionID] == 0)
            {
                m_StoreController.ConfirmPurchase(order);
                m_ProcessingTransactions.Remove(transactionID);
                return;
            }

            CompleteGrant(order, productID, JsonConvert.DeserializeObject<TransactionResult>(result.ToString()));
        });
    }

    private bool TryConsumeRetry(string transactionID, BackendReturnObject bro)
    {
        if (!BFuncResponseHandler.IsRetryable(bro))
            return false;

        // 재시도 실패 시 다음 앱 시작의 Pending 재처리에 맡긴다.
        int count = m_ProcessingTransactions[transactionID];
        if (count >= RETRY_MAX)
            return false;

        m_ProcessingTransactions[transactionID] = count + 1;
        return true;
    }

    private async UniTask RetryLater(PendingOrder order, string productID)
    {
        await UniTask.Delay(RETRY_DELAY_MS, ignoreTimeScale: true);
        SendValidateRequest(order, productID);
    }

    private void CompleteGrant(PendingOrder order, string productID, TransactionResult result)
    {
        m_GrantHandler.OnIAPGranted(productID, result);
        m_StoreController.ConfirmPurchase(order);
        m_ProcessingTransactions.Remove(order.Info.TransactionID);

        m_PurchaseHandler?.OnPurchaseSuccess(productID);
        m_PurchaseHandler = null;
    }

    private void NotifyFailed(string productID, string error)
    {
        m_PurchaseHandler?.OnPurchaseFailed(productID, error);
        m_PurchaseHandler = null;
    }

#if UNITY_EDITOR
    private TransactionResult CreateEditorResult(string productID)
    {
        var result = new TransactionResult();
        if (productID == BATTLE_PASS || !m_IAPEntries.TryGetValue(productID, out var entry))
            return result;

        foreach (var spec in entry.RewardSpecs)
        {
            // 장비는 서버가 인스턴스 ID를 발급해야 해서 에디터에서 만들 수 없다.
            if (ItemID.IsWearable(spec.ID) || ItemID.IsWeapon(spec.ID))
                continue;

            result.StackableDic.TryGetValue(spec.ID, out int cur);
            result.StackableDic[spec.ID] = cur + spec.Amount;
        }
        return result;
    }
#endif

    private string GetProductID(Order order)
    {
        var purchasedProducts = order.Info?.PurchasedProductInfo;
        if (purchasedProducts == null || purchasedProducts.Count == 0)
            return "";

        return purchasedProducts[0].productId;
    }

    private void HandlePurchaseConfirmed(Order order)
    {
        // 지급은 Confirm 전에 끝난다. 실패한 Confirm은 다음 실행 시 Pending으로 다시 와서 alreadyGranted로 처리된다.
        if (order is FailedOrder failedOrder)
        {
            HDebug.LogError($"[IAP] Confirm failed. {GetProductID(order)}, reason={failedOrder.FailureReason}");
        }
    }

    private void HandleProductsFetched(List<Product> list)
    {
        m_LocalizePrices.Clear();

        foreach (var p in list)
        {
            if (p.metadata != null)
            {
                m_LocalizePrices[p.definition.id] =
                    p.metadata.localizedPriceString + p.metadata.isoCurrencyCode;
            }
        }

        m_StoreController.FetchPurchases();
    }

    private void HandleProductsFetchFailed(ProductFetchFailed failed)
    {
        var ids = string.Join(",", failed.FailedFetchProducts.Select(p => p.id));
        HDebug.LogError($"[IAP] Products fetch failed. reason={failed.FailureReason}, ids={ids}");
    }

    private void HandlePurchasesFetched(Orders orders)
    {
        //구매가 아직 완료되지 않은 내역
        HDebug.Log($"[IAP] Purchases fetched. pending={orders.PendingOrders.Count}");
        foreach (var pendingOrder in orders.PendingOrders)
        {
            HDebug.Log($"[IAP] Pending order. {GetProductID(pendingOrder)}, transactionID={pendingOrder.Info.TransactionID}");
            HandlePurchasePending(pendingOrder);
        }
    }

    public void Purchase(string productID, IRealPurchaseHandler handler)
    {
        if (!m_IAPEntries.ContainsKey(productID))
        {
            SystemPopupTemplate.ShowOKPopup(Localize.Get("COMMON_TITLE_ERROR"), Localize.Get("IAP_ITEM_NOT_FOUND"));
            return;
        }

        m_PurchaseHandler = handler;
        m_StoreController.PurchaseProduct(productID);
    }

    public ShopEntry GetIAPEntry(string productID)
    {
        if (m_IAPEntries.TryGetValue(productID, out var entry))
            return entry;

        HDebug.LogError($"Failed to find IAP entry. {productID}");
        return null;
    }

    public string GetCostString(string productID)
    {
        if (m_LocalizePrices.TryGetValue(productID, out var price))
            return price;

        HDebug.LogError($"[IAP] Price not fetched. {productID}");
        return "-";
    }
}
