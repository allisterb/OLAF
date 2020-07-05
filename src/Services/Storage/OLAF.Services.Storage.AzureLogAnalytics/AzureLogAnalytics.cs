using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OLAF.Services.Storage
{
    public class AzureLogAnalytics : Service<Artifact, Artifact>
    {
        #region Constructors
        public AzureLogAnalytics(Profile profile, params Type[] clients) : base(profile, clients) 
        {
            
        }
        #endregion

        #region Overriden members
        public override ApiResult Init()
        {
            var i = Global.GetAppSetting("cred.config", "AzureLogAnalytics").Trim().Split(":".ToCharArray());
            if (i.Length != 2 || string.IsNullOrEmpty(i[0]) || string.IsNullOrEmpty(i[1]))
            {
                Error("Could not read Azure Log Analytics key or workspace id from file {0}.", "cred.config");
                return SetErrorStatusAndReturnFailure();
            }
            else
            {
                I = i[0];
                K = i[1];
            }
            WorkspaceId = I;
            SharedKey = K;
            RequestBaseUrl = $"https://{WorkspaceId}.ods.opinsights.azure.com/api/logs?api-version={ApiVersion}";
            return SetInitializedStatusAndReturnSucces();
        }

        protected override ApiResult ProcessClientQueueMessage(Artifact message)
        {
            if (message is TextArtifact ta)
            {
                var e = new DocumentLogEvent(ta);
                return SendLogEntry(e, "OLAF").Result;

            }
            else if (message is ImageArtifact ia)
            {
                var e = new DocumentLogEvent(ia);
                return SendLogEntry(e, "OLAF").Result;
            }
            else return ApiResult.NoOp;
        }
        #endregion

        #region Methods
        public async Task<ApiResult> SendLogEntries<T>(List<T> entities, string logType)
        {
            #region Argument validation

            if (entities == null)
                throw new NullReferenceException("parameter 'entities' cannot be null");

            if (logType.Length > 100)
                throw new ArgumentOutOfRangeException(nameof(logType), logType.Length, "The size limit for this parameter is 100 characters.");

            if (!IsAlphaOnly(logType))
                throw new ArgumentOutOfRangeException(nameof(logType), logType, "Log-Type can only contain alpha characters. It does not support numerics or special characters.");

            foreach (var entity in entities)
                ValidatePropertyTypes(entity);

            #endregion

            var dateTimeNow = DateTime.UtcNow.ToString("r");

            var entityAsJson = JsonConvert.SerializeObject(entities);
            var authSignature = GetAuthSignature(entityAsJson, dateTimeNow);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", authSignature);
            httpClient.DefaultRequestHeaders.Add("Log-Type", logType);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("x-ms-date", dateTimeNow);
            httpClient.DefaultRequestHeaders.Add("time-generated-field", ""); // if we want to extend this in the future to support custom date fields from the entity etc.

            HttpContent httpContent = new StringContent(entityAsJson, Encoding.UTF8);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await httpClient.PostAsync(new Uri(RequestBaseUrl), httpContent);
            if (response.IsSuccessStatusCode)
            {

                Debug("Wrote {0} log entries to Azure Log Analytics.", entities.Count);
                return ApiResult.Success;
            }
            else
            {
                Error("Could not send {0} log entries to Azure Log Analytics: HTTP response: {1} {2}.", entities.Count, response.StatusCode, response.ReasonPhrase);
                return ApiResult.Failure;
            }
        }

        public async Task<ApiResult> SendLogEntry<T>(T entity, string logType)
        {
            #region Argument validation

            if (entity == null)
                throw new NullReferenceException("parameter 'entity' cannot be null");

            if (logType.Length > 100)
                throw new ArgumentOutOfRangeException(nameof(logType), logType.Length, "The size limit for this parameter is 100 characters.");

            if (!IsAlphaOnly(logType))
                throw new ArgumentOutOfRangeException(nameof(logType), logType, "Log-Type can only contain alpha characters. It does not support numerics or special characters.");

            ValidatePropertyTypes(entity);

            #endregion

            List<T> list = new List<T> { entity };
            return await SendLogEntries(list, logType).ConfigureAwait(false);
        }


        #region Helpers

        private string GetAuthSignature(string serializedJsonObject, string dateString)
        {
            string stringToSign = $"POST\n{serializedJsonObject.Length}\napplication/json\nx-ms-date:{dateString}\n/api/logs";
            string signedString;

            var encoding = new ASCIIEncoding();
            var sharedKeyBytes = Convert.FromBase64String(SharedKey);
            var stringToSignBytes = encoding.GetBytes(stringToSign);
            using (var hmacsha256Encryption = new HMACSHA256(sharedKeyBytes))
            {
                var hashBytes = hmacsha256Encryption.ComputeHash(stringToSignBytes);
                signedString = Convert.ToBase64String(hashBytes);
            }

            return $"SharedKey {WorkspaceId}:{signedString}";
        }
        private bool IsAlphaOnly(string str)
        {
            return Regex.IsMatch(str, @"^[a-zA-Z]+$");
        }
        private void ValidatePropertyTypes<T>(T entity)
        {
            // as of 2018-10-30, the allowed property types for log analytics, as defined here (https://docs.microsoft.com/en-us/azure/log-analytics/log-analytics-data-collector-api#record-type-and-properties) are: string, bool, double, datetime, guid.
            // anything else will be throwing an exception here.
            foreach (PropertyInfo propertyInfo in entity.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType != typeof(string) &&
                    propertyInfo.PropertyType != typeof(bool) &&
                    propertyInfo.PropertyType != typeof(double) &&
                    propertyInfo.PropertyType != typeof(DateTime) &&
                    propertyInfo.PropertyType != typeof(Guid))
                {
                    throw new ArgumentOutOfRangeException($"Property '{propertyInfo.Name}' of entity with type '{entity.GetType()}' is not one of the valid properties. Valid properties are String, Boolean, Double, DateTime, Guid.");
                }
            }
        }

        #endregion

        #endregion

        #region Fields
        private const string ApiVersion = "2016-04-01";
        public string I { get; protected set; }
        public string K { get; protected set; }
        private string WorkspaceId { get; set; }
        private string SharedKey { get; set; }
        private string RequestBaseUrl { get; set; }

        private static readonly HttpClient httpClient = new HttpClient();
        #endregion
    }
}
