using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceHub.Models;

namespace FinanceHub.Data
{
    public class FinanceHubContext : DbContext
    {
        public FinanceHubContext (DbContextOptions<FinanceHubContext> options)
            : base(options)
        {
        }

        public DbSet<FinanceHub.Models.Categoria> Categoria { get; set; } = default!;
    }
}
