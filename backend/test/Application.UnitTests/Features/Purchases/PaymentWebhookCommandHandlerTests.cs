using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Repositories;
using Application.Features.Purchases.Command.PaymentWebhook;
using Domain.Carts;
using Domain.Carts.CartItems;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Address;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Products.Variants;
using Domain.Purchases;
using Domain.Purchases.Enum;
using Domain.Purchases.Payments;
using Domain.Purchases.Payments.Enum;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Features.Purchases;

public class PaymentWebhookCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly PaymentWebhookCommandHandler _handler;
    private readonly ILogger<PaymentWebhookCommandHandler> _loggerMock;
    private readonly Mock<IEmailJob> _emailJobMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;

    public PaymentWebhookCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _emailJobMock = new Mock<IEmailJob>();
        _loggerMock = NullLogger<PaymentWebhookCommandHandler>.Instance;
        _cartRepositoryMock = new Mock<ICartRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();

        _unitOfWorkMock.SetupGet(u => u.Payments).Returns(_paymentRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Carts).Returns(_cartRepositoryMock.Object);
        _unitOfWorkMock.SetupGet(u => u.CartItems).Returns(_cartItemRepositoryMock.Object);

        _handler = new PaymentWebhookCommandHandler(
            _unitOfWorkMock.Object,
            _paymentRepositoryMock.Object,
            _loggerMock,
            _emailJobMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnPaymentNotFound_When_PaymentDoesNotExist()
    {
        // Arrange
        var command = new PaymentWebhookCommand("nonexistent-checkout-id", "payment_intent.succeeded", "paid");

        _paymentRepositoryMock.Setup(repo => 
            repo.GetByTransactionIdWithPurchaseAndItemsAndVariantAsync(command.CheckoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(ApplicationErrors.PaymentNotFound);
    }

    [Theory]
    [InlineData(PaymentStatus.Paid)]
    [InlineData(PaymentStatus.Failed)]
    public async Task Handle_Should_ReturnUpdated_When_PaymentAlreadyProcessedOrFailed(PaymentStatus status)
    {
        // Arrange
        var payment = Payment.Create(Guid.NewGuid(), 100, status);
        var command = new PaymentWebhookCommand("checkout-id", "payment_intent.succeeded", "paid");

        _paymentRepositoryMock.Setup(repo =>
            repo.GetByTransactionIdWithPurchaseAndItemsAndVariantAsync(command.CheckoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment.Value);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Result.Updated);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("processing")]
    public async Task Handle_Should_ReturnUpdated_When_ChargilyStatusIsNonTerminal(string chargilyStatus)
    {
        // Arrange
        var payment = Payment.Create(Guid.NewGuid(), 100, PaymentStatus.Pending);
        var command = new PaymentWebhookCommand("checkout-id", "payment_intent.succeeded", chargilyStatus);

        _paymentRepositoryMock.Setup(repo =>
            repo.GetByTransactionIdWithPurchaseAndItemsAndVariantAsync(command.CheckoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment.Value);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Result.Updated);
        payment.Value.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task Handle_Should_ReturnUpdated_When_DeleteCartItemsAfterSuccessfulPayment()
    {
        // Arrange     
        var userId = Guid.NewGuid();
        var phoneNumber = PhoneNumber.Create("0660949924");
        var address = Address.Create("123 Main St", "City", "State");
        var purchase = Purchase.Create(userId, phoneNumber.Value, address.Value, PurchaseOrigin.Cart);
        var payment = Payment.Create(purchase.Value.Id, 100, PaymentStatus.Pending);
        payment.Value.AddPurchase(purchase.Value);
        var command = new PaymentWebhookCommand("checkout-id", "payment_intent.succeeded", "paid");

        _paymentRepositoryMock.Setup(repo =>
            repo.GetByTransactionIdWithPurchaseAndItemsAndVariantAsync(command.CheckoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment.Value);

        var cart = Cart.Create(userId);
        for(var i = 0; i < 3; i++)
            cart.Value.AddItem(Guid.NewGuid(), 1);

        _cartRepositoryMock.Setup(c => 
            c.GetByUserIdWithItemsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Result.Updated);
        payment.Value.Status.Should().Be(PaymentStatus.Paid);
        _cartItemRepositoryMock.Verify(r => r.Delete(It.IsAny<CartItem>()), Times.Exactly(3));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailJobMock.Verify(e => e.ScheduleSendPurchaseNotificationAsync(purchase.Value.Id, userId,
            It.IsAny<List<PurchaseNotificationItemPayload>>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnUpdated_When_PaymentFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var phoneNumber = PhoneNumber.Create("0660949924");
        var address = Address.Create("123 Main St", "City", "State");
        var purchase = Purchase.Create(userId, phoneNumber.Value, address.Value, PurchaseOrigin.Cart);
        var payment = Payment.Create(purchase.Value.Id, 100, PaymentStatus.Pending);
        payment.Value.AddPurchase(purchase.Value);
        var variant1 = Variant.Create(Guid.NewGuid(), "M", "Red", 10);
        var variant2 = Variant.Create(Guid.NewGuid(), "L", "Blue", 5);
        var purchaseitem = payment.Value.Purchase.AddItem(variant1.Value.Id, 2, 3000);
        var purchaseitem2 = payment.Value.Purchase.AddItem(variant2.Value.Id, 3, 3000);        
        purchaseitem.Value.AttachVariant(variant1.Value);
        purchaseitem2.Value.AttachVariant(variant2.Value);
        var command = new PaymentWebhookCommand("checkout-id", "payment_intent.payment_failed", "failed");

        _paymentRepositoryMock.Setup(repo =>
            repo.GetByTransactionIdWithPurchaseAndItemsAndVariantAsync(command.CheckoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Result.Updated);
        payment.Value.Status.Should().Be(PaymentStatus.Failed);
        variant1.Value.StockQuantity.Should().Be(12);
        variant2.Value.StockQuantity.Should().Be(8);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
