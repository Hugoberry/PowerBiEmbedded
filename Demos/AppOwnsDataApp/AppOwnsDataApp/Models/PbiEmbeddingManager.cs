﻿using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;

namespace AppOwnsDataApp.Models {

  public class PbiEmbeddingManager {

    private static readonly string clientId = ConfigurationManager.AppSettings["client-id"];
    private static readonly string clientSecret = ConfigurationManager.AppSettings["client-secret"];
    private static readonly string tenantName = ConfigurationManager.AppSettings["tenant-name"];

    private static readonly string workspaceId = ConfigurationManager.AppSettings["app-workspace-id"];
    private static readonly string datasetId = ConfigurationManager.AppSettings["dataset-id"];
    private static readonly string reportId = ConfigurationManager.AppSettings["report-id"];
    private static readonly string dashboardId = ConfigurationManager.AppSettings["dashboard-id"];

    // endpoint for tenant-specific authority 
    private static readonly string tenantAuthority = "https://login.microsoftonline.com/" + tenantName;

    // Power BI Service API Root URL
    const string urlPowerBiRestApiRoot = "https://api.powerbi.com/";

    static string GetAppOnlyAccessToken() {

      var appConfidential = ConfidentialClientApplicationBuilder.Create(clientId)
                              .WithClientSecret(clientSecret)
                              .WithAuthority(tenantAuthority)
                              .Build();

      string[] scopesDefault = new string[] { "https://analysis.windows.net/powerbi/api/.default" };
      var authResult = appConfidential.AcquireTokenForClient(scopesDefault).ExecuteAsync().Result;
      return authResult.AccessToken;
    }

    private static PowerBIClient GetPowerBiClient() {
      var tokenCredentials = new TokenCredentials(GetAppOnlyAccessToken(), "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiRestApiRoot), tokenCredentials);
    }

    public static async Task<ReportEmbeddingData> GetReportEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);
      var embedUrl = report.EmbedUrl;
      var reportName = report.Name;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "edit");
      string embedToken =
            (await pbiClient.Reports.GenerateTokenInGroupAsync(workspaceId,
                                                               report.Id,
                                                               generateTokenRequestParameters)).Token;

      return new ReportEmbeddingData {
        reportId = reportId,
        reportName = reportName,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static async Task<DashboardEmbeddingData> GetDashboardEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var dashboard = await pbiClient.Dashboards.GetDashboardInGroupAsync(workspaceId, dashboardId);
      var embedUrl = dashboard.EmbedUrl;
      var dashboardDisplayName = dashboard.DisplayName;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");

      string embedToken =
         (await pbiClient.Dashboards.GenerateTokenInGroupAsync(workspaceId,
                                                               dashboardId,
                                                               generateTokenRequestParameters)).Token;

      return new DashboardEmbeddingData {
        dashboardId = dashboardId,
        dashboardName = dashboardDisplayName,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public async static Task<QnaEmbeddingData> GetQnaEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var dataset = await pbiClient.Datasets.GetDatasetByIdInGroupAsync(workspaceId, datasetId);

      string embedUrl = "https://app.powerbi.com/qnaEmbed?groupId=" + workspaceId;
      string datasetID = dataset.Id;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
      string embedToken =
             (await pbiClient.Datasets.GenerateTokenInGroupAsync(workspaceId,
                                                                 dataset.Id,
                                                                 generateTokenRequestParameters)).Token;

      return new QnaEmbeddingData {
        datasetId = datasetId,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static async Task<NewReportEmbeddingData> GetNewReportEmbeddingData() {

      string embedUrl = "https://app.powerbi.com/reportEmbed?groupId=" + workspaceId;

      PowerBIClient pbiClient = GetPowerBiClient();

      GenerateTokenRequest generateTokenRequestParameters =
                           new GenerateTokenRequest(accessLevel: "create", datasetId: datasetId);
      string embedToken =
        (await pbiClient.Reports.GenerateTokenForCreateInGroupAsync(workspaceId,
                                                                    generateTokenRequestParameters)).Token;

      return new NewReportEmbeddingData {
        workspaceId = workspaceId,
        datasetId = datasetId,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static async Task<ReportEmbeddingData> GetEmbeddingDataForReport(string currentReportId) {
      PowerBIClient pbiClient = GetPowerBiClient();
      var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, currentReportId);
      var embedUrl = report.EmbedUrl;
      var reportName = report.Name;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "edit");
      string embedToken =
            (await pbiClient.Reports.GenerateTokenInGroupAsync(workspaceId,
                                                                currentReportId,
                                                                generateTokenRequestParameters)).Token;

      return new ReportEmbeddingData {
        reportId = currentReportId,
        reportName = reportName,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

  }

}
