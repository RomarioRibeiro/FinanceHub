using FinanceHub.Data;
using FinanceHub.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FinanceHub.Services
{
    public class CategoriaService
    {
        private readonly FinanceHubContext _context;

        public CategoriaService(FinanceHubContext context)
        {
            _context = context;
        }


        public async Task<List<Categoria>> FindAllAsync()
        {
            return await _context.Categoria.ToListAsync();
        }

        public async Task<Categoria> FindByIdAsync(int id)
        {
            return await _context.Categoria.FirstOrDefaultAsync(obj => obj.Id == id);
        }

        public async Task InsertAsync(Categoria categoria)
        {
            _context.Add(categoria);
            _context.SaveChanges();
        }

        public async Task Update(Categoria categoria)
        {
            bool isCategoria = await _context.Categoria.AnyAsync(obj => obj.Id == categoria.Id);
            if (!isCategoria)
            {
                throw new Exception();
            }
            try
            {
                _context.Update(categoria);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task RemoveAsync(int id)
        {
            try
            {
                var obj = await _context.Categoria.FindAsync(id);
                _context.Remove(obj);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

    }
}
