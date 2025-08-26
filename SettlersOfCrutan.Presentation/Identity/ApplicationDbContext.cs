using Azure.Data.Tables;
using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace SettlersOfCrutan.Presentation.Identity;

public class ApplicationDbContext : IdentityCloudContext
{
    public ApplicationDbContext(IdentityConfiguration config, TableServiceClient tableServiceClient) : base(config, tableServiceClient)
    {
    }
}
