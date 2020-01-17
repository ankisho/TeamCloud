/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectCreateActivity
    {
        private readonly IProjectsRepository projectsRepository;

        public ProjectCreateActivity(IProjectsRepository projectsRepository)
        {
            this.projectsRepository = projectsRepository ?? throw new System.ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectCreateActivity))]
        public Task<Project> RunActivity(
            [ActivityTrigger] Project project)
            => projectsRepository.AddAsync(project);
    }
}
