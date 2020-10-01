﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class StatusController : ApiController
    {
        public StatusController(UserService userService, Orchestrator orchestrator) : base(userService, orchestrator)
        { }

        [Authorize(Policy = AuthPolicies.Admin)]
        [HttpGet("api/status/{trackingId:guid}")]
        [SwaggerOperation(OperationId = "GetStatus", Summary = "Gets the status of a long-running operation.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The long-running operation completed.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The long-running operation is running. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status302Found, "The long-running operation completed.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The long-running operation with the trackingId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get(Guid trackingId)
        {
            var result = await Orchestrator
                .QueryAsync(trackingId, null)
                .ConfigureAwait(false);

            return GetStatusResult(result);
        }

        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [HttpGet("api/projects/{projectId:guid}/status/{trackingId:guid}")]
        [SwaggerOperation(OperationId = "GetProjectStatus", Summary = "Gets the status of a long-running operation.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The long-running operation completed.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The long-running operation is running. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status302Found, "The long-running operation completed.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The long-running operation with the trackingId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get(string projectId, Guid trackingId)
        {
            var result = await Orchestrator
                .QueryAsync(trackingId, projectId)
                .ConfigureAwait(false);

            return GetStatusResult(result);
        }

        private static IActionResult GetStatusResult(ICommandResult result)
        {
            if (result is null)
                return ErrorResult
                    .NotFound($"A status for the provided Tracking Id was not found.")
                    .ToActionResult();

            result.Links.TryGetValue("status", out var status);

            switch (result.RuntimeStatus)
            {
                case CommandRuntimeStatus.Completed:

                    if (result.Links.TryGetValue("location", out var location))
                    {
                        return StatusResult
                            .Success(result.CommandId.ToString(), location, result.RuntimeStatus.ToString(), result.CustomStatus)
                            .ToActionResult();
                    }

                    // no resource location (i.e. DELETE command) return 200 (ok)
                    return StatusResult
                        .Success(result.CommandId.ToString(), result.RuntimeStatus.ToString(), result.CustomStatus)
                        .ToActionResult();

                case CommandRuntimeStatus.Running:
                case CommandRuntimeStatus.ContinuedAsNew:
                case CommandRuntimeStatus.Pending:

                    // command is in an active state, return 202 (accepted) so client can poll
                    return StatusResult
                        .Accepted(result.CommandId.ToString(), status, result.RuntimeStatus.ToString(), result.CustomStatus)
                        .ToActionResult();

                case CommandRuntimeStatus.Canceled:
                case CommandRuntimeStatus.Terminated:
                case CommandRuntimeStatus.Failed:

                    return StatusResult
                        .Failed(result.Errors, result.CommandId.ToString(), result.RuntimeStatus.ToString(), result.CustomStatus)
                        .ToActionResult();

                default: // TODO: this probably isn't right as a default

                    if (result.Errors?.Any() ?? false)
                        return StatusResult
                            .Failed(result.Errors, result.CommandId.ToString(), result.RuntimeStatus.ToString(), result.CustomStatus)
                            .ToActionResult();

                    return StatusResult
                        .Ok(result.CommandId.ToString(), result.RuntimeStatus.ToString(), result.CustomStatus)
                        .ToActionResult();
            }
        }
    }
}
