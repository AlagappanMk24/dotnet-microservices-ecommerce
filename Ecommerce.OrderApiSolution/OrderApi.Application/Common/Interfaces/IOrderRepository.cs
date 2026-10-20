using Ecommerce.SharedLibrary.Repositories;
using Ecommerce.SharedLibrary.Responses;
using OrderApi.Domain.Entities;

namespace OrderApi.Application.Common.Interfaces;

public interface IOrderRepository : IRepository<CustomerOrder>
{
    Task<CustomerOrder?> GetForCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken);
    Task<PagedResult<CustomerOrder>> GetAllForCustomerAsync(Guid customerId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}