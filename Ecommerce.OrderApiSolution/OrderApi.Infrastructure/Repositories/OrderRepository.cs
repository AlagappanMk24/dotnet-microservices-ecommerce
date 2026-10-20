using Ecommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using OrderApi.Application.Common.Interfaces;
using OrderApi.Domain.Entities;
using OrderApi.Infrastructure.Persistence.Context;

namespace OrderApi.Infrastructure.Repositories;

public sealed class OrderRepository(OrderDbContext db) : IOrderRepository
{
    public async Task<CustomerOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
         await db.Orders
             .Include(x => x.Items)
             .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<CustomerOrder?> GetForCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken) =>
        await db.Orders
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == orderId && x.CustomerId == customerId, cancellationToken);

    public async Task<PagedResult<CustomerOrder>> GetAllForCustomerAsync(Guid customerId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.Orders
            .Include(x => x.Items)
            .Where(x => x.CustomerId == customerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerOrder>(items, totalCount, pageNumber, pageSize);
    }

    public async Task AddAsync(CustomerOrder entity, CancellationToken cancellationToken = default) =>
        await db.Orders.AddAsync(entity, cancellationToken);

    public void Update(CustomerOrder entity) =>
           db.Orders.Update(entity);

    public void Remove(CustomerOrder entity)
    {
        entity.MarkDeleted();
        db.Orders.Update(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await db.SaveChangesAsync(cancellationToken);
}