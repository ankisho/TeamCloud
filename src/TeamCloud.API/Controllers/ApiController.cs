﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    public abstract class ApiController : ControllerBase
    {
        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository)
        {
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            Orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            OrganizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectRepository projectRepository)
            : this(userService, orchestrator, organizationRepository)
        {
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IUserRepository userRepository)
            : this(userService, orchestrator, organizationRepository)
        {
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectTemplateRepository projectTemplateRepository)
            : this(userService, orchestrator, organizationRepository)
        {
            ProjectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IProjectTemplateRepository projectTemplateRepository)
            : this(userService, orchestrator, organizationRepository, projectRepository)
        {
            ProjectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IUserRepository userRepository)
            : this(userService, orchestrator, organizationRepository)
        {
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        private string Org => RouteData.ValueOrDefault(nameof(Org));

        private string ProjectTemplateId => RouteData.ValueOrDefault(nameof(ProjectTemplateId));

        public string UserId => RouteData.ValueOrDefault(nameof(UserId));

        public string ProjectId => RouteData.ValueOrDefault(nameof(ProjectId));

        public string OrgId { get; private set; }


        public UserService UserService { get; }

        public Orchestrator Orchestrator { get; }

        public IUserRepository UserRepository { get; }

        public IProjectRepository ProjectRepository { get; }

        public IOrganizationRepository OrganizationRepository { get; }

        public IProjectTemplateRepository ProjectTemplateRepository { get; }


        [NonAction]
        public Task<IActionResult> ResolveOrganizationIdAsync(Func<string, Task<IActionResult>> callback)
            => ResolveOrganizationIdInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> ResolveOrganizationIdAsync(Func<string, IActionResult> callback)
            => ResolveOrganizationIdInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> ResolveOrganizationIdInternalAsync(Func<string, Task<IActionResult>> asyncCallback = null, Func<string, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrgId = await OrganizationRepository
                    .ResolveIdAsync(Org)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrgId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(OrgId);

                if (!(asyncCallback is null))
                    return await asyncCallback(OrgId)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }


        [NonAction]
        public Task<IActionResult> EnsureOrganizationAsync(Func<Organization, Task<IActionResult>> callback)
            => EnsureOrganizationInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureOrganizationAsync(Func<Organization, IActionResult> callback)
            => EnsureOrganizationInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureOrganizationInternalAsync(Func<Organization, Task<IActionResult>> asyncCallback = null, Func<Organization, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var organization = await OrganizationRepository
                    .GetAsync(Org)
                    .ConfigureAwait(false);

                if (organization is null)
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(organization);

                if (!(asyncCallback is null))
                    return await asyncCallback(organization)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }


        [NonAction]
        public Task<IActionResult> EnsureProjectAsync(Func<Project, Task<IActionResult>> callback)
            => EnsureProjectInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAsync(Func<Project, IActionResult> callback)
            => EnsureProjectInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectInternalAsync(Func<Project, Task<IActionResult>> asyncCallback = null, Func<Project, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrgId = await OrganizationRepository
                    .ResolveIdAsync(Org)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrgId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProjectId))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(OrgId, ProjectId)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectId}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(project);

                if (!(asyncCallback is null))
                    return await asyncCallback(project)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }


        [NonAction]
        public Task<IActionResult> EnsureProjectTemplateAsync(Func<ProjectTemplate, Task<IActionResult>> callback)
            => EnsureProjectTemplateInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectTemplateAsync(Func<ProjectTemplate, IActionResult> callback)
            => EnsureProjectTemplateInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectTemplateInternalAsync(Func<ProjectTemplate, Task<IActionResult>> asyncCallback = null, Func<ProjectTemplate, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrgId = await OrganizationRepository
                    .ResolveIdAsync(Org)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrgId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProjectTemplateId))
                    return ErrorResult
                        .BadRequest($"Project Template name or id provided in the url path is invalid.  Must be a valid project template name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var template = await ProjectTemplateRepository
                    .GetAsync(OrgId, ProjectTemplateId)
                    .ConfigureAwait(false);

                if (template is null)
                    return ErrorResult
                        .NotFound($"A Project Template with the name or id '{ProjectTemplateId}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(template);

                if (!(asyncCallback is null))
                    return await asyncCallback(template)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }


        [NonAction]
        public Task<IActionResult> EnsureProjectAndProjectTemplateAsync(Func<Project, ProjectTemplate, Task<IActionResult>> callback)
            => EnsureProjectAndProjectTemplateInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAndProjectTemplateAsync(Func<Project, ProjectTemplate, IActionResult> callback)
            => EnsureProjectAndProjectTemplateInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectAndProjectTemplateInternalAsync(Func<Project, ProjectTemplate, Task<IActionResult>> asyncCallback = null, Func<Project, ProjectTemplate, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrgId = await OrganizationRepository
                    .ResolveIdAsync(Org)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrgId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProjectId))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(OrgId, ProjectId)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectId}' was not found.")
                        .ToActionResult();

                var templateId = ProjectTemplateId ?? project.Template;

                if (string.IsNullOrEmpty(templateId))
                    return ErrorResult
                        .BadRequest($"Project Template name or id provided in the url path is invalid.  Must be a valid project template name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var projectTemplate = await ProjectTemplateRepository
                    .GetAsync(OrgId, templateId)
                    .ConfigureAwait(false);

                if (projectTemplate is null)
                    return ErrorResult
                        .NotFound($"A Project Template with the name or id '{templateId}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(project, projectTemplate);

                if (!(asyncCallback is null))
                    return await asyncCallback(project, projectTemplate)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }


        [NonAction]
        public Task<IActionResult> EnsureUserAsync(Func<User, Task<IActionResult>> callback)
            => EnsureUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureUserAsync(Func<User, IActionResult> callback)
            => EnsureUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureUserInternalAsync(Func<User, Task<IActionResult>> asyncCallback = null, Func<User, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrgId = await OrganizationRepository
                    .ResolveIdAsync(Org)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrgId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(UserId))
                    return ErrorResult
                        .BadRequest($"User name or id provided in the url path is invalid.  Must be a valid email address, service pricipal name, or GUID.", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var userId = await UserService
                    .GetUserIdAsync(UserId)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(userId))
                    return ErrorResult
                        .NotFound($"A User with the name or id '{UserId}' was not found.")
                        .ToActionResult();

                var user = await UserRepository
                    .GetAsync(OrgId, userId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User with the Id '{UserId}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(user);

                if (!(asyncCallback is null))
                    return await asyncCallback(user)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public Task<IActionResult> EnsureCurrentUserAsync(Func<User, Task<IActionResult>> callback)
            => EnsureCurrentUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureCurrentUserAsync(Func<User, IActionResult> callback)
            => EnsureCurrentUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureCurrentUserInternalAsync(Func<User, Task<IActionResult>> asyncCallback = null, Func<User, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrgId = await OrganizationRepository
                    .ResolveIdAsync(Org)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrgId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                var user = await UserService
                    .CurrentUserAsync(OrgId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User matching the current authenticated user was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(user);

                if (!(asyncCallback is null))
                    return await asyncCallback(user)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public Task<IActionResult> EnsureProjectAndUserAsync(Func<Project, User, Task<IActionResult>> callback)
            => EnsureProjectAndUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAndUserAsync(Func<Project, User, IActionResult> callback)
            => EnsureProjectAndUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectAndUserInternalAsync(Func<Project, User, Task<IActionResult>> asyncCallback = null, Func<Project, User, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrgId = await OrganizationRepository
                    .ResolveIdAsync(Org)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrgId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProjectId))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                if (string.IsNullOrEmpty(UserId))
                    return ErrorResult
                        .BadRequest($"User name or id provided in the url path is invalid.  Must be a valid email address, service pricipal name, or GUID.", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var userId = await UserService
                    .GetUserIdAsync(UserId)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(userId))
                    return ErrorResult
                        .NotFound($"A User with the name or id '{UserId}' was not found.")
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(OrgId, ProjectId)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectId}' was not found.")
                        .ToActionResult();

                var user = await UserRepository
                    .GetAsync(OrgId, userId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User with the Id '{UserId}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(project, user);

                if (!(asyncCallback is null))
                    return await asyncCallback(project, user)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public Task<IActionResult> EnsureProjectAndCurrentUserAsync(Func<Project, User, Task<IActionResult>> callback)
            => EnsureProjectAndCurrentUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAndCurrentUserAsync(Func<Project, User, IActionResult> callback)
            => EnsureProjectAndCurrentUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectAndCurrentUserInternalAsync(Func<Project, User, Task<IActionResult>> asyncCallback = null, Func<Project, User, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Org))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrgId = await OrganizationRepository
                    .ResolveIdAsync(Org)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrgId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Org}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProjectId))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(OrgId, ProjectId)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectId}' was not found.")
                        .ToActionResult();

                var user = await UserService
                    .CurrentUserAsync(OrgId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User matching the current authenticated user was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(project, user);

                if (!(asyncCallback is null))
                    return await asyncCallback(project, user)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }
    }
}
