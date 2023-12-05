using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Unity.Services.Economy.Model;

namespace PlayerModule;

public class MainMenuController
{
    private readonly ILogger<MainMenuController> _logger;
    
    public MainMenuController(ILogger<MainMenuController> logger)
    {
        _logger = logger;
    }
    
    [CloudCodeFunction("GetInitialisation")]
    public async Task<string> GetInitialisation(IExecutionContext ctx, IGameApiClient apiClient)
    {
        List<InitialisationResult> result = new List<InitialisationResult>();

        try
        {
            foreach (InitialisationStage initStage in (InitialisationStage[])
                     Enum.GetValues(typeof(InitialisationStage)))
            {
                switch (initStage)
                {
                    case InitialisationStage.Body:
                        ApiResponse<GetItemsResponse> body = await apiClient.CloudSaveData.GetItemsAsync(
                            ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                            new List<string> { "body" });

                        result.Add(new InitialisationResult()
                        {
                            Stage = initStage,
                            Value = body.Data.Results.First().Value.ToString()
                        });
                        break;
                    case InitialisationStage.Progression:
                        ApiResponse<GetItemsResponse> progression = await apiClient.CloudSaveData.GetItemsAsync(
                            ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                            new List<string> { "progression" });

                        result.Add(new InitialisationResult()
                        {
                            Stage = initStage,
                            Value = progression.Data.Results.First().Value.ToString()
                        });
                        break;
                    case InitialisationStage.Wallet:
                        ApiResponse<PlayerCurrencyBalanceResponse> currenciesResult = await apiClient.EconomyCurrencies.GetPlayerCurrenciesAsync(
                            ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId);

                        Wallet wallet = new Wallet();
                        //string log = "";
                        foreach (CurrencyBalanceResponse response in currenciesResult.Data.Results)
                        {
                            CurrencyType currencyType = Enum.Parse<CurrencyType>(response.CurrencyId, true);
                            wallet.Currencies[(int)currencyType] = new Currency()
                            {
                                currencyType = currencyType,
                                amount = (int)response.Balance
                            };
                        }
                        
                        result.Add(new InitialisationResult()
                        {
                            Stage = initStage,
                            Value = JsonConvert.SerializeObject(wallet)
                        });
                        break;
                    case InitialisationStage.Equipment:
                        ApiResponse<GetItemsResponse> equipments = await apiClient.CloudSaveData.GetItemsAsync(
                            ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                            new List<string> { "gear" }
                        );
                        
                        Gear gear = JsonConvert.DeserializeObject<Gear>(
                            equipments.Data.Results.First().Value.ToString()
                        );

                        result.Add(new InitialisationResult()
                        {
                            Stage = initStage,
                            Value = JsonConvert.SerializeObject(gear)
                        });
                        break;
                    case InitialisationStage.Inventory:
                        ApiResponse<PlayerInventoryResponse> inventoryResult = await apiClient.EconomyInventory.GetPlayerInventoryAsync(
                            ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId);
                        
                        //string log = "";

                        Inventory inventory = new Inventory();
                        foreach (InventoryResponse dataResult in inventoryResult.Data.Results)
                        {
                            inventory.AddItem(new CharacterEquipment()
                            {
                                id = dataResult.PlayersInventoryItemId,
                                item = new Equipment()
                                {
                                    ID = dataResult.InventoryItemId,
                                    itemType = ItemType.Equipment
                                }
                            });
                            //log += dataResult + "\n";
                        }
                        
                        //_logger.LogInformation("data results : {result}", log);

                        /*result.Add(new InitialisationResult()
                        {
                            Stage = initStage,
                            Value = JsonConvert.SerializeObject(inventory)
                        });*/
                        break;
                    case InitialisationStage.CharacterStats:
                        ApiResponse<GetItemsResponse> stats = await apiClient.CloudSaveData.GetItemsAsync(
                            ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                            new List<string> { "characterStats" });

                        result.Add(new InitialisationResult()
                        {
                            Stage = initStage,
                            Value = stats.Data.Results.First().Value.ToString()
                        });
                        break;
                }

                /*result.Add(new InitialisationResult() {
                    Stage = initStage,
                    Value = await apiClient.CloudSaveData
                    .GetItemsAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                        new List<string> { "initialisation" })
                    }
                );*/
            }

            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            return JsonConvert.SerializeObject(result, jsonSerializerSettings);
        }
        catch (Exception e)
        {
            return JsonConvert.SerializeObject(e);
        }
    }

    [CloudCodeFunction("TutorialFinished")]
    public async Task<string> TutorialFinished(IExecutionContext ctx, IGameApiClient apiClient)
    {
        try
        {
            ApiResponse<GetItemsResponse> playerConfResult = await apiClient.CloudSaveData.GetItemsAsync(
                ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                new List<string> { "playerConf" }
            );
                
            PlayerConf playerConf = JsonConvert.DeserializeObject<PlayerConf>(playerConfResult.Data.Results.First().Value.ToString());
            playerConf.TutorialDone = true;
                
            var result = await apiClient.CloudSaveData
                .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody("playerConf", playerConf));

            return result.Data.ToString();
        }
        catch (Exception e)
        {
            return JsonConvert.SerializeObject(e);
        }
    }
}