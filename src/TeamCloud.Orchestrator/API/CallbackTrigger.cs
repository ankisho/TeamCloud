﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Http;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator
{
    public static class CallbackTrigger
    {
        private static async Task<string> GetCallbackToken(string instanceId)
        {
            var masterKey = await FunctionEnvironment
                .GetMasterKeyAsync()
                .ConfigureAwait(false);

            var json = await FunctionEnvironment.HostUrl
                .AppendPathSegment("admin/functions")
                .AppendPathSegment(nameof(CallbackTrigger))
                .AppendPathSegment("keys")
                .SetQueryParam("code", masterKey)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            return json
                .SelectToken($"$.keys[?(@.name == '{instanceId}')].value")?
                .ToString();
        }

        internal static async Task<string> AcquireCallbackUrlAsync(string instanceId, ICommand command)
        {
            var functionKey = await GetCallbackToken(instanceId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(functionKey))
            {
                var masterKey = await FunctionEnvironment
                    .GetMasterKeyAsync()
                    .ConfigureAwait(false);

                var response = await FunctionEnvironment.HostUrl
                    .AppendPathSegment("admin/functions")
                    .AppendPathSegment(nameof(CallbackTrigger))
                    .AppendPathSegment("keys")
                    .AppendPathSegment(instanceId, true)
                    .SetQueryParam("code", masterKey)
                    .PostJsonAsync(null)
                    .ConfigureAwait(false);

                var functionKeysJson = await response.Content
                    .ReadAsJsonAsync()
                    .ConfigureAwait(false);

                functionKey = functionKeysJson
                    .SelectToken($"$.value")?
                    .ToString();
            }

            return FunctionEnvironment.HostUrl
                .AppendPathSegment("api/callback")
                .AppendPathSegment(instanceId, true)
                .AppendPathSegment(command.CommandId)
                .SetQueryParam("code", functionKey)
                .ToString();
        }

        internal static async Task InvalidateCallbackUrlAsync(string instanceId)
        {
            var masterKey = await FunctionEnvironment
                .GetMasterKeyAsync()
                .ConfigureAwait(false);

            try
            {
                _ = await FunctionEnvironment.HostUrl
                    .AppendPathSegment("admin/functions")
                    .AppendPathSegment(nameof(CallbackTrigger))
                    .AppendPathSegment("keys")
                    .AppendPathSegment(instanceId, true)
                    .SetQueryParam("code", masterKey)
                    .AllowAnyHttpStatus()
                    .DeleteAsync()
                    .ConfigureAwait(false);
            }
            catch
            {
                var functionKey = await GetCallbackToken(instanceId)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(functionKey))
                    throw;
            }
        }

        [FunctionName(nameof(CallbackTrigger))]
        public static async Task<IActionResult> RunTrigger(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "callback/{instanceId}/{eventName}")] HttpRequest httpRequest,
            [DurableClient] IDurableClient durableClient,
            string instanceId,
            string eventName,
            ILogger log)
        {
            if (httpRequest is null)
                throw new ArgumentNullException(nameof(httpRequest));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            if (instanceId is null)
                throw new ArgumentNullException(nameof(instanceId));

            if (eventName is null)
                throw new ArgumentNullException(nameof(eventName));

            ICommandResult commandResult;

            try
            {
                using var reader = new StreamReader(httpRequest.Body);

                var requestBody = await reader
                    .ReadToEndAsync()
                    .ConfigureAwait(false);

                commandResult = JsonConvert.DeserializeObject<ICommandResult>(requestBody);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Callback '{instanceId}' ({eventName}) - Failed to deserialize callback payload");

                return new BadRequestResult();
            }

            try
            {
                var status = await durableClient
                    .GetStatusAsync(instanceId)
                    .ConfigureAwait(false);

                if (status?.RuntimeStatus.IsFinal() ?? true)
                {
                    // the orchestration does not exist or reached
                    // a final state - raising an external event 
                    // doesn't make sense, but we need to response
                    // with an status code that indicates that
                    // another retry doesn't make sense.
                    // status code 410 (Gone) looks like a perfect fit

                    return new StatusCodeResult((int)HttpStatusCode.Gone);
                }
                else
                {
                    await durableClient
                        .RaiseEventAsync(instanceId, eventName, commandResult)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Callback '{instanceId}' ({eventName}) - Failed to raise event with payload {JsonConvert.SerializeObject(commandResult)}.");

                throw; // re-throw exception to return an internal server error
            }

            return new OkResult();
        }
    }
}
