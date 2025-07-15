using Johan_Reinoso_AP1_P2.Components.DAL;
using Johan_Reinoso_AP1_P2.Components.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Johan_Reinoso_AP1_P2.Components.Service;

public class ProductoService(IDbContextFactory<Contexto> DbFactory)
{
    public async Task<List<Productos>> Listar(Expression<Func<Productos, bool>> criterio)
    {
        await using var context = await DbFactory.CreateDbContextAsync();
        return await context.productos.Where(criterio).AsNoTracking().ToListAsync();
    }
}