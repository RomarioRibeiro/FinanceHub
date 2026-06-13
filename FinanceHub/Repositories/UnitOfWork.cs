using FinanceHub.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinanceHub.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FinanceHubContext _context;

        public UnitOfWork(FinanceHubContext context)
        {
            _context = context;
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return _context.Database.BeginTransactionAsync();
        }
    }
}
