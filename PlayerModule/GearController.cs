using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Unity.Services.Economy.Model;

namespace PlayerModule;

public class GearController
{
    private readonly ILogger<GearController> _logger;
    
    public GearController(ILogger<GearController> logger)
    {
        _logger = logger;
    }
    
    [CloudCodeFunction("Equip")]
    public async Task<string> Equip(IExecutionContext ctx, IGameApiClient apiClient, CharacterEquipment characterEquipment)
    {
        try
        {
            ApiResponse<GetItemsResponse> result = await apiClient.CloudSaveData.GetItemsAsync(
                ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                new List<string> { "progression" });
            
            PlayerProgression progression = JsonConvert.DeserializeObject<PlayerProgression>(result.Data.Results.First().Value.ToString());
            if (progression.Level >= characterEquipment.item.requiredLevel)
            {
                result = await apiClient.CloudSaveData.GetItemsAsync(
                    ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                    new List<string> { "gear" });
            
                Gear gear = JsonConvert.DeserializeObject<Gear>(result.Data.Results.First().Value.ToString());
                if (gear.Equipments == null || gear.Equipments.Length == 0) {
                    gear.Equipments = new CharacterEquipment[Enum.GetNames(typeof(EquipmentSlot)).Length];
                }
                _logger.LogInformation("gear : " + gear + "\n equipments length : " + gear.Equipments.Length);
                gear.Equipments[(int)characterEquipment.item.equipmentSlot] = characterEquipment;
            
                await apiClient.CloudSaveData
                    .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody("gear", gear));

                await apiClient.EconomyInventory.DeleteInventoryItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                    characterEquipment.id);
            
                return JsonConvert.SerializeObject(characterEquipment);
            }
            else
            {
                return JsonConvert.SerializeObject(new Exception("Equipment required level is too high !"));
            }
        }
        catch (Exception e)
        {
            return JsonConvert.SerializeObject(e);
        }
    }
    
    [CloudCodeFunction("UnEquip")]
    public async Task<string> UnEquip(IExecutionContext ctx, IGameApiClient apiClient, CharacterEquipment characterEquipment)
    {
        try
        {
            ApiResponse<GetItemsResponse> gearResult = await apiClient.CloudSaveData.GetItemsAsync(
                ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                new List<string> { "gear" });
            
            Gear gear = JsonConvert.DeserializeObject<Gear>(gearResult.Data.Results.First().Value.ToString());
            gear.Equipments[(int)characterEquipment.item.equipmentSlot] = null;
            
            ApiResponse<SetItemResponse> result = await apiClient.CloudSaveData
                .SetItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId, new SetItemBody("gear", gear));

            ApiResponse<InventoryResponse> response = await apiClient.EconomyInventory.AddInventoryItemAsync(ctx, ctx.AccessToken, ctx.ProjectId, ctx.PlayerId,
                new AddInventoryRequest(characterEquipment.item.ID, characterEquipment.id));
            
            return JsonConvert.SerializeObject(result);
        }
        catch (Exception e)
        {
            return JsonConvert.SerializeObject(e);
        }
    }
}