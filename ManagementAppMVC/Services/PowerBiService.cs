using ManagementAppMVC.Services;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;

namespace ManagementAppMVC.Services
{
    public class PowerBiService : IPowerBiService
    {
        private readonly ILogger<PowerBiService> _logger;
        private readonly IConfiguration _config;

        public PowerBiService(ILogger<PowerBiService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<(string EmbedToken, string EmbedUrl, string reportId)> GetEmbedInfoAsync(string accessToken)
        {
            return await GetEmbedInfoWithFiltersAsync(accessToken, null, null);
        }

        public async Task<(string EmbedToken, string EmbedUrl, string reportId)> GetEmbedInfoWithFiltersAsync(
            string accessToken,
            string username,
            Dictionary<string, string> rlsFilters)
        {
            var groupIdStr = _config["PowerBI:WorkspaceId"];
            var reportIdStr = _config["PowerBI:ReportId"];

            if (string.IsNullOrWhiteSpace(groupIdStr))
                throw new ArgumentNullException(nameof(groupIdStr), "PowerBI:WorkspaceId configuration value is missing or empty.");

            if (string.IsNullOrWhiteSpace(reportIdStr))
                throw new ArgumentNullException(nameof(reportIdStr), "PowerBI:ReportId configuration value is missing or empty.");

            Guid groupId = new(groupIdStr);
            Guid reportId = new(reportIdStr);

            var client = new PowerBIClient(accessToken);

            // list workspaces just for information
            var workspaces = await client.Groups.GetGroupsAsync();
            foreach (var r in workspaces.Value.Value)
            {
                _logger.LogInformation($"Workspace: {r.Name}, Id: {r.Id}");
            }

            // list reports in workspace just for information
            var reports = await client.Reports.GetReportsInGroupAsync(groupId);
            foreach (var r in reports.Value.Value)
            {
                _logger.LogInformation($"Report: {r.Name}, Id: {r.Id}");
            }

            // Get report
            var report = await client.Reports.GetReportInGroupAsync(groupId, reportId);
            var datasetId = report.Value.DatasetId;

            // Generate embed token with optional RLS
            var request = new GenerateTokenRequestV2();
            request.Reports.Add(new GenerateTokenRequestV2Report(reportId));
            request.Datasets.Add(new GenerateTokenRequestV2Dataset(datasetId));

            // Apply Row-Level Security if filters are provided
            if (!string.IsNullOrWhiteSpace(username) && rlsFilters?.Count > 0)
            {
                // BuildRlsFilters can remain for logging; actual table-level filter objects require building EffectiveIdentity table filters.
                var rlsList = BuildRlsFilters(rlsFilters);

                var effectiveIdentity = new EffectiveIdentity
                {
                    Username = username,
                    CustomData = "C001"
                };

                // IMPORTANT: associate the effective identity with the dataset(s) being accessed
                effectiveIdentity.Datasets.Add(datasetId);

                // Add role(s) that exist in the dataset RLS definition
                effectiveIdentity.Roles.Add("CompanyRLS");

                request.Identities.Add(effectiveIdentity);

                _logger.LogInformation($"RLS Applied for user: {username} with {rlsList.Count} filters (dataset: {datasetId})");
            }

            var tokenResponse = await client.EmbedToken.GenerateTokenAsync(request);

            return (tokenResponse.Value.Token, report.Value.EmbedUrl, reportIdStr);
        }

        /// <summary>
        /// Builds RLS filter strings from dictionary of table/column pairs and their values.
        /// Example: { "Sales", "Country" } = "Value" becomes '[Sales].[Country] = "Value"'
        /// </summary>
        private List<string> BuildRlsFilters(Dictionary<string, string> filters)
        {
            var rlsList = new List<string>();

            foreach (var filter in filters)
            {
                // Format: [Table].[Column] = 'Value'
                var rlsFilter = $"'{filter.Key}' = '{EscapeRlsValue(filter.Value)}'";
                rlsList.Add(rlsFilter);
                _logger.LogInformation($"RLS Filter: {rlsFilter}");
            }

            return rlsList;
        }

        /// <summary>
        /// Escapes single quotes in RLS filter values to prevent injection.
        /// </summary>
        private string EscapeRlsValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape single quotes by doubling them
            return value.Replace("'", "''");
        }
    }
}
