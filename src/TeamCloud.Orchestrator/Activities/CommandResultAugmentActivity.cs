﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Activities
{
    public static class CommandResultAugmentActivity
    {
        [FunctionName(nameof(CommandResultAugmentActivity))]
        public static async Task<ICommandResult> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            [DurableClient] IDurableClient durableClient,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            if (durableClient is null)
                throw new ArgumentNullException(nameof(durableClient));

            var commandResult = functionContext.GetInput<ICommandResult>();

            try
            {
                var commandStatus = await durableClient
                    .GetStatusAsync(commandResult.CommandId.ToString())
                    .ConfigureAwait(false);

                if (commandStatus != null)
                    commandResult.ApplyStatus(commandStatus);
            }
            catch (Exception exc)
            {
                log?.LogWarning(exc, $"Failed to augment command result with orchestration status {commandResult.CommandId}: {exc.Message}");
            }

            return commandResult;
        }
    }
}
