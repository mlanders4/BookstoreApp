using System;
using System.Data;
using System.Data.SqlClient;
using Bookstore.Checkout.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Bookstore.Checkout.Accessors
{
    public class PaymentAccessor : IPaymentAccessor
    {
        private readonly string _connectionString;
        private readonly ILogger<PaymentAccessor> _logger;

        public PaymentAccessor(string connectionString, ILogger<PaymentAccessor> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> CreatePaymentAsync(PaymentEntity payment)
        {
            const string sql = @"
                INSERT INTO Checkout (
                    order_id, 
                    credit_card_number, 
                    expiry_date, 
                    amount, 
                    status, 
                    payment_date
                )
                OUTPUT INSERTED.checkout_id
                VALUES (
                    @OrderId, 
                    @CardNumber, 
                    @ExpiryDate, 
                    @Amount, 
                    @Status, 
                    @PaymentDate
                )";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            try
            {
                command.Parameters.AddRange(new[]
                {
                    new SqlParameter("@OrderId", payment.OrderId),
                    new SqlParameter("@CardNumber", payment.MaskedCardNumber),
                    new SqlParameter("@ExpiryDate", payment.ExpiryDate),
                    new SqlParameter("@Amount", payment.Amount),
                    new SqlParameter("@Status", payment.PaymentStatus ?? "Pending"),
                    new SqlParameter("@PaymentDate", payment.PaymentDate)
                });

                await connection.OpenAsync();
                var checkoutId = (int)await command.ExecuteScalarAsync();
                
                _logger.LogInformation(
                    "Created payment record {CheckoutId} for order {OrderId}", 
                    checkoutId, payment.OrderId);
                
                return checkoutId;
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint
            {
                _logger.LogError(
                    ex, 
                    "Duplicate payment attempt for order {OrderId}", 
                    payment.OrderId);
                throw new PaymentAccessException(
                    $"Payment for order {payment.OrderId} already exists", ex);
            }
            catch (SqlException ex) when (ex.Number == 547) // FK violation
            {
                _logger.LogError(
                    ex, 
                    "Invalid OrderId {OrderId} in payment record", 
                    payment.OrderId);
                throw new PaymentAccessException(
                    $"Invalid OrderId: {payment.OrderId}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Failed to create payment for order {OrderId}", 
                    payment.OrderId);
                throw new PaymentAccessException(
                    "Payment creation failed", ex);
            }
        }

        public async Task UpdatePaymentStatusAsync(int checkoutId, string status)
        {
            const string sql = @"
                UPDATE Checkout 
                SET 
                    status = @Status,
                    payment_date = CASE 
                        WHEN @Status = 'Completed' THEN GETUTCDATE() 
                        ELSE payment_date 
                    END
                WHERE checkout_id = @CheckoutId";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            try
            {
                command.Parameters.AddRange(new[]
                {
                    new SqlParameter("@CheckoutId", checkoutId),
                    new SqlParameter("@Status", status)
                });

                await connection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    _logger.LogWarning(
                        "No payment record updated for CheckoutId {CheckoutId}", 
                        checkoutId);
                    throw new PaymentAccessException(
                        $"Payment record {checkoutId} not found");
                }

                _logger.LogInformation(
                    "Updated payment {CheckoutId} to status {Status}", 
                    checkoutId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Failed to update payment {CheckoutId}", 
                    checkoutId);
                throw new PaymentAccessException(
                    "Payment status update failed", ex);
            }
        }
    }

    public class PaymentAccessException : Exception
    {
        public PaymentAccessException(string message, Exception innerException = null) 
            : base(message, innerException) { }
    }
}
