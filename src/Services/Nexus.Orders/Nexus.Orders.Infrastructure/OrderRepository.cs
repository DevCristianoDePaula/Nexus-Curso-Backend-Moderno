using Microsoft.EntityFrameworkCore;
using Nexus.Orders.Application;
using Nexus.Orders.Domain;

namespace Nexus.Orders.Infrastructure;

///
/// <summary>
/// Repositório de Pedidos implementado com Entity Framework Core (SQL Server).
/// 
/// Padrão **Repository**: abstrai o acesso a dados da tabela Orders.
/// Utiliza navegação explícita (Include) para carregar os itens do pedido,
/// e os métodos SaveChangesAsync são chamados internamente (unidade de trabalho via DbContext).
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly NexusOrderDbContext _context;

    public OrderRepository(NexusOrderDbContext context) => _context = context;

    ///
    /// <summary>
    /// Busca um pedido pelo ID com os itens carregados (Include).
    /// </summary>
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items) // Carregamento explícito da collection Items.
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    ///
    /// <summary>
    /// Retorna pedidos de um cliente com paginação, ordenados do mais recente ao mais antigo.
    /// </summary>
    public async Task<List<Order>> GetByCustomerAsync(string customerId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    ///
    /// <summary>
    /// Adiciona um novo pedido ao contexto e persiste no banco.
    /// </summary>
    public async Task CreateAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct); // Confirma a transação no SQL Server.
    }

    ///
    /// <summary>
    /// Atualiza um pedido existente no banco.
    /// </summary>
    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(ct);
    }
}
