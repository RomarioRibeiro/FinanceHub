using Microsoft.EntityFrameworkCore.Storage;

namespace FinanceHub.Repositories
{
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
