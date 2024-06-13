using IceSync.Domain.Contracts;
using IceSync.Domain.Entities;
using IceSync.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IceSync.Persistence.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WorkflowRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IEnumerable<Workflow>> GetWorkflowsAsync()
        {
            using(var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IceSyncDbContext>();
                return await dbContext.Workflows.ToListAsync();
            }
        }

        public async Task AddWorkflowsAsync(IEnumerable<Workflow> workflows)
        {
            using(var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IceSyncDbContext>();
                dbContext.Workflows.AddRange(workflows);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateWorkflowsAsync(IEnumerable<Workflow> workflows)
        {
            using(var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IceSyncDbContext>();
                dbContext.Workflows.UpdateRange(workflows);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteWorkflowsAsync(IEnumerable<int> workflowIds)
        {
            using(var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IceSyncDbContext>();

                var entities = await dbContext.Workflows
                    .Where(w => workflowIds.Contains(w.WorkflowID))
                    .ToListAsync();

                dbContext.Workflows.RemoveRange(entities);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
