using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Unity.Services.Economy.Model;

namespace PlayerModule;

public class ShopController
{
    private readonly ILogger<ShopController> _logger;
    
    public ShopController(ILogger<ShopController> logger)
    {
        _logger = logger;
    }
    
    [CloudCodeFunction("GetItemList")]
    public async Task<string> GetItemList(IExecutionContext ctx, IGameApiClient apiClient)
    {
        List<Item> result = new List<Item>();

        try
        {
            /*result.Add(new InitialisationResult() {
                Stage = initStage,
                Value = await apiClient.CloudSaveData
                .GetItemsAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                    new List<string> { "initialisation" })
                }
            );*/
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            return JsonConvert.SerializeObject(result, jsonSerializerSettings);
        }
        catch (Exception e)
        {
            return JsonConvert.SerializeObject(e);
        }
    }

    [CloudCodeFunction("BuyItem")]
    public async Task<string> BuyItem(IExecutionContext ctx, IGameApiClient apiClient, string transactionId)
    {
        try
        {
            PlayerPurchaseVirtualRequest playerPurchase = new PlayerPurchaseVirtualRequest(transactionId);
            
            //TODO
            ApiResponse<PlayerPurchaseVirtualResponse> purchaseRequest = await apiClient.EconomyPurchases.MakeVirtualPurchaseAsync(
                ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, playerPurchase);

            List<string> inventoryItemIds = new List<string>();
            string log = "";
            
            foreach (InventoryExchangeItem inventoryExchangeItem in purchaseRequest.Data.Rewards.Inventory)
            {
                foreach (string inventoryItemId in inventoryExchangeItem.PlayersInventoryItemIds)
                {
                    inventoryItemIds.Add(inventoryItemId);
                }

                log += JsonConvert.SerializeObject(inventoryExchangeItem);
            }
            
            ApiResponse<PlayerInventoryResponse> items = await apiClient.EconomyInventory.GetPlayerInventoryAsync(
                ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, null, null, null, null, null, inventoryItemIds);
            _logger.LogInformation("buy item : " + log);

            return JsonConvert.SerializeObject(purchaseRequest);
        }
        catch (Exception e)
        {
            return JsonConvert.SerializeObject(e);
        }
    }
}