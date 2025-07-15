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
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                if (entrada.EntradasDetalles == null || !entrada.EntradasDetalles.Any())
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Error: La entrada debe contener al menos un detalle de producto.");
                    return false;
                }

                foreach (var detalle in entrada.EntradasDetalles)
                {
                    if (detalle.ProductoId <= 0)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error: Detalle con ID {detalle.EntradasDetallesId} tiene un ProductoId inválido.");
                        return false;
                    }

                    if (detalle.Cantidad <= 0)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error: Detalle con ID {detalle.EntradasDetallesId} tiene una Cantidad inválida (debe ser mayor que 0).");
                        return false;
                    }

                    if (detalle.Producto == null)
                    {
                        var productoEnContexto = context.productos.Local.FirstOrDefault(p => p.ProductoId == detalle.ProductoId);
                        if (productoEnContexto != null)
                        {
                            detalle.Producto = productoEnContexto;
                        }
                        else
                        {
                            var productoDesdeDb = await context.productos.FindAsync(detalle.ProductoId);
                            if (productoDesdeDb != null)
                            {
                                detalle.Producto = productoDesdeDb;
                                context.Entry(detalle.Producto).State = EntityState.Unchanged;
                            }
                            else
                            {
                                await transaction.RollbackAsync();
                                Console.WriteLine($"Error: Producto con ID {detalle.ProductoId} no encontrado para un detalle.");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        context.Entry(detalle.Producto).State = EntityState.Unchanged;
                    }

                    var productoToUpdate = await context.productos.FindAsync(detalle.ProductoId);
                    if (productoToUpdate == null || productoToUpdate.Existencia < detalle.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error: Existencia insuficiente para ProductoId {detalle.ProductoId}. Requerido: {detalle.Cantidad}, Disponible: {productoToUpdate?.Existencia ?? 0}");
                        return false;
                    }
                    productoToUpdate.Existencia -= detalle.Cantidad;
                    context.productos.Update(productoToUpdate);
                }

                entrada.PesoTotal = entrada.EntradasDetalles.Sum(d => d.Cantidad * (d.Producto?.Peso ?? 0));

                context.entradas.Add(entrada);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error al insertar la Entrada y actualizar existencias: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Modificar(Entradas entrada)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var entradaExistente = await context.entradas
                                                    .Include(e => e.EntradasDetalles)
                                                        .ThenInclude(ed => ed.Producto)
                                                    .FirstOrDefaultAsync(e => e.EntradasId == entrada.EntradasId);

                if (entradaExistente == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                foreach (var detalleExistente in entradaExistente.EntradasDetalles.ToList())
                {
                    var newDetalle = entrada.EntradasDetalles.FirstOrDefault(d => d.EntradasDetallesId == detalleExistente.EntradasDetallesId);

                    if (newDetalle == null)
                    {
                        var producto = await context.productos.FindAsync(detalleExistente.ProductoId);
                        if (producto != null)
                        {
                            producto.Existencia += detalleExistente.Cantidad;
                            context.productos.Update(producto);
                        }
                        context.entradasDetalles.Remove(detalleExistente);
                    }
                    else if (newDetalle.Cantidad != detalleExistente.Cantidad)
                    {
                        var producto = await context.productos.FindAsync(detalleExistente.ProductoId);
                        if (producto != null)
                        {
                            int quantityDifference = detalleExistente.Cantidad - newDetalle.Cantidad;
                            producto.Existencia += quantityDifference;
                            context.productos.Update(producto);
                        }
                    }
                }

                foreach (var detalleNuevoOModificado in entrada.EntradasDetalles)
                {
                    var detalleDb = entradaExistente.EntradasDetalles.FirstOrDefault(d => d.EntradasDetallesId == detalleNuevoOModificado.EntradasDetallesId);

                    if (detalleDb == null)
                    {
                        detalleNuevoOModificado.EntradasId = entrada.EntradasId;
                        context.entradasDetalles.Add(detalleNuevoOModificado);

                        if (detalleNuevoOModificado.Producto == null && detalleNuevoOModificado.ProductoId > 0)
                        {
                            var producto = await context.productos.FirstOrDefaultAsync(p => p.ProductoId == detalleNuevoOModificado.ProductoId);
                            if (producto != null)
                            {
                                detalleNuevoOModificado.Producto = producto;
                            }
                            else
                            {
                                await transaction.RollbackAsync();
                                return false;
                            }
                        }

                        var productoToUpdate = await context.productos.FindAsync(detalleNuevoOModificado.ProductoId);
                        if (productoToUpdate == null || productoToUpdate.Existencia < detalleNuevoOModificado.Cantidad)
                        {
                            await transaction.RollbackAsync();
                            return false;
                        }
                        productoToUpdate.Existencia -= detalleNuevoOModificado.Cantidad;
                        context.productos.Update(productoToUpdate);

                    }
                    else
                    {
                        detalleDb.Cantidad = detalleNuevoOModificado.Cantidad;

                        if (detalleDb.Producto == null && detalleDb.ProductoId > 0)
                        {
                            var producto = await context.productos.FirstOrDefaultAsync(p => p.ProductoId == detalleDb.ProductoId);
                            if (producto != null)
                            {
                                detalleDb.Producto = producto;
                            }
                        }
                    }
                }

                context.Entry(entradaExistente).CurrentValues.SetValues(entrada);
                entradaExistente.PesoTotal = entradaExistente.EntradasDetalles.Sum(d => d.Cantidad * (d.Producto?.Peso ?? 0));

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error al modificar la Entrada y actualizar existencias: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Eliminar(int entradaId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var entradaAEliminar = await context.entradas
                                                     .Include(e => e.EntradasDetalles)
                                                         .ThenInclude(ed => ed.Producto)
                                                     .FirstOrDefaultAsync(e => e.EntradasId == entradaId);

                if (entradaAEliminar == null)
                {
                    return false;
                }

                foreach (var detalle in entradaAEliminar.EntradasDetalles)
                {
                    var producto = await context.productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Existencia += detalle.Cantidad;
                        context.productos.Update(producto);
                    }
                }

                context.entradas.Remove(entradaAEliminar);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error al eliminar la entrada: {ex.Message}");
                return false;
            }
        }

        public async Task<Entradas?> Buscar(int entradaId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            return await context.entradas
                                .Include(e => e.EntradasDetalles)
                                    .ThenInclude(ed => ed.Producto)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => e.EntradasId == entradaId);
        }

        public async Task<List<Entradas>> Listar(Expression<Func<Entradas, bool>> criterio)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            return await context.entradas
                                .Include(e => e.EntradasDetalles)
                                    .ThenInclude(ed => ed.Producto)
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
                                                    .ThenInclude(ed => ed.Producto)
                                                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(valorFiltro))
            {
                var valor = valorFiltro.ToLower();

                if (filtroCampo == "EntradaId" && int.TryParse(valorFiltro, out var id))
                    query = query.Where(x => x.EntradasId == id);
                else if (filtroCampo == "Concepto")
                    query = query.Where(x => x.Concepto.ToLower().Contains(valor));
                else if (filtroCampo == "Cantidad" && int.TryParse(valorFiltro, out var cantidad))
                    query = query.Where(x => x.EntradasDetalles.FirstOrDefault().Cantidad == cantidad);
                else if (filtroCampo == "PesoTotal" && decimal.TryParse(valorFiltro, out var peso))
                    query = query.Where(x => x.PesoTotal == peso);
            }

            if (fechaDesde.HasValue)
                query = query.Where(x => x.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(x => x.Fecha < fechaHasta.Value.AddDays(1));

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

            var currentQuantityInDetails = entrada.EntradasDetalles
                                                .Where(d => d.ProductoId == selectedProductoId)
                                                .Sum(d => d.Cantidad);

            var projectedTotalQuantity = currentQuantityInDetails + cantidadDetalle;

            if (productoSeleccionado.Existencia < projectedTotalQuantity)
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