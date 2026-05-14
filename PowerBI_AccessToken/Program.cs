using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace PowerBI_AccessToken
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var authService = new AuthService();
            var accessToken = await authService.GetAccessTokenAsync();
            Console.WriteLine($"Access Token: {accessToken}");
            Console.WriteLine();

            var powerBiService = new PowerBiService();
            var groupId = "69fc403b-5ed9-46f4-991d-b36551c92492";
            var reportId = "4ade58c5-6995-4cad-9b01-c36c396e25f4";

            var embededInfo = await powerBiService.GetEmbedInfoAsync(accessToken,  new Guid(groupId), new Guid(reportId));
            Console.WriteLine($"Embeded Token: {embededInfo.EmbedToken}");
            Console.WriteLine($"Embeded Url: {embededInfo.EmbedUrl}");

            Console.ReadKey();
        }
    }

    public class AuthService
    {
        public async Task<string> GetAccessTokenAsync()
        {
            //sp-learnlight-fabric-integration
            var clientId = "0aee503d-91d6-43bf-98bd-71a167b399ec";
            var tenantId = "544f8ac3-ce4c-47d1-9b72-284ac54b8d1c";
            var clientSecret = "***";

            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var scopes = new[] { "https://analysis.windows.net/powerbi/api/.default" };

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(authority)
                .Build();

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }
    }

    public class PowerBiService
    {
        public async Task<(string EmbedToken, string EmbedUrl)> GetEmbedInfoAsync(
            string accessToken,
            Guid groupId,
            Guid reportId)
        {
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");

            var client = new PowerBIClient(accessToken);

            // list workspaces
            var workspaces = await client.Groups.GetGroupsAsync();
            foreach (var r in workspaces.Value.Value)
            {
                Console.WriteLine($"Workspace: {r.Name}, Id: {r.Id}");
            }
            Console.WriteLine();

            // list reports in workspace
            var reports = await client.Reports.GetReportsInGroupAsync(groupId);
            foreach (var r in reports.Value.Value)
            {
                Console.WriteLine($"Report: {r.Name}, Id: {r.Id}");
            }
            Console.WriteLine();

            // Get report
            var report = await client.Reports.GetReportInGroupAsync(groupId, reportId);
            var datasetId = report.Value.DatasetId;

            // Generate embed token
            var request = new GenerateTokenRequestV2();
            request.Reports.Add(new GenerateTokenRequestV2Report(reportId));
            request.Datasets.Add(new GenerateTokenRequestV2Dataset(datasetId));

            var tokenResponse = await client.EmbedToken.GenerateTokenAsync(request);


            return (tokenResponse.Value.Token, report.Value.EmbedUrl);
        }
    }
}
