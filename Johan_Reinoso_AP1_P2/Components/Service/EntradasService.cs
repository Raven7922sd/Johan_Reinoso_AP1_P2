using Johan_Reinoso_AP1_P2.Components.DAL;
using Johan_Reinoso_AP1_P2.Components.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Johan_Reinoso_AP1_P2.Components.Service
{
    public class EntradasService
    {
        private readonly IDbContextFactory<Contexto> _dbFactory;

        public EntradasService(IDbContextFactory<Contexto> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> Guardar(Entradas entrada)
        {
            if (!await ExisteId(entrada.EntradasId))
            {
                return (await Insertar(entrada));
            }
            else
            {
                return await Modificar(entrada);
            }
        }

        public async Task<bool> ExisteId(int id)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            return await context.entradas.AnyAsync(e => e.EntradasId == id);
        }

        public async Task<bool> Insertar(Entradas entrada)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            context.entradas.Add(entrada);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Modificar(Entradas entrada)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            context.entradas.Update(entrada);

            try
            {
                return await context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al modificar la Entrada: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Eliminar(int entradaId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            try
            {
                var entradaAEliminar = await context.entradas
                                                    .Include(e => e.EntradasDetalles)
                                                    .FirstOrDefaultAsync(e => e.EntradasId == entradaId);

                if (entradaAEliminar == null)
                {
                    return false;
                }

                context.entradas.Remove(entradaAEliminar);

                return await context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar la Entrada: {ex.Message}");
                return false;
            }
        }

        public async Task<Entradas?> Buscar(int entradaId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            return await context.entradas
                                .Include(e => e.EntradasDetalles)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => e.EntradasId == entradaId);
        }

        public async Task<List<Entradas>> Listar(Expression<Func<Entradas, bool>> criterio)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            return await context.entradas
                                .Include(e => e.EntradasDetalles)
                                .Where(criterio)
                                .AsNoTracking()
                                .ToListAsync();
        }

        public async Task<List<Entradas>> BuscarFiltradosAsync(
            string filtroCampo,
            string valorFiltro,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            IQueryable<Entradas> query = context.entradas
                                                .Include(e => e.EntradasDetalles)
                                                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(valorFiltro))
            {
                var valor = valorFiltro.ToLower();

                if (filtroCampo == "EntradaId" && int.TryParse(valorFiltro, out var id))
                    query = query.Where(x => x.EntradasId == id);
                else if (filtroCampo == "Concepto")
                    query = query.Where(x => x.Concepto.ToLower().Contains(valor));
                else if (filtroCampo == "IdProducido" && int.TryParse(valorFiltro, out var idProducido))
                    query = query.Where(x => x.IdProducido == idProducido);
                else if (filtroCampo == "CantidadProducida" && int.TryParse(valorFiltro, out var cantidad))
                    query = query.Where(x => x.CantidadProducida == cantidad);
                else if (filtroCampo == "PesoTotal" && decimal.TryParse(valorFiltro, out var peso))
                    query = query.Where(x => x.PesoTotal == peso);
            }

            if (fechaDesde.HasValue)
                query = query.Where(x => x.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(x => x.Fecha <= fechaHasta.Value);

            return await query.OrderBy(x => x.Fecha).ToListAsync();
        }

        public async Task<bool> AgregarProductoDetalle(Entradas entrada, int selectedProductoId, int cantidadDetalle)
        {
            if (selectedProductoId == 0 || cantidadDetalle <= 0)
            {
                return false;
            }

            await using var context = await _dbFactory.CreateDbContextAsync();

            var productoSeleccionado = await context.productos.AsNoTracking().FirstOrDefaultAsync(p => p.ProductoId == selectedProductoId);
            if (productoSeleccionado == null)
            {
                return false;
            }

            var detalleExistente = entrada.EntradasDetalles.FirstOrDefault(d => d.ProductoId == selectedProductoId);
            if (detalleExistente != null)
            {
                detalleExistente.Cantidad += cantidadDetalle;
            }
            else
            {
                var nuevoDetalle = new EntradasDetalles
                {
                    ProductoId = selectedProductoId,
                    Producto = productoSeleccionado,
                    Cantidad = cantidadDetalle,
                };
                entrada.EntradasDetalles.Add(nuevoDetalle);
            }
            return true;
        }
    }
}