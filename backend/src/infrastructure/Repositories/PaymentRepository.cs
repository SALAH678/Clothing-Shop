using Application.Common.Interfaces.Repositories;
using Domain.Purchases.Payments;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class PaymentRepository(AppDbContext context) : Repository<Payment>(context), IPaymentRepository
{
}
