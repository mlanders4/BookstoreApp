using System;
using System.Data;
using System.Data.SqlClient;
using Bookstore.Checkout.Models.Entities;

namespace Bookstore.Checkout.Accessors
{
    public class PaymentAccessor : IPaymentAccessor
    {
        private readonly string _connectionString;

        public PaymentAccessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreatePaymentAsync(PaymentEntity payment)
        {
            const string sql = @"
                INSERT INTO Checkout (credit_card_number, expiry_date, amount)
                OUTPUT INSERTED.checkout_id
                VALUES (@CardNumber, @ExpiryDate, @Amount)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CardNumber", payment.MaskedCardNumber);
                command.Parameters.AddWithValue("@ExpiryDate", payment.ExpiryDate);
                command.Parameters.AddWithValue("@Amount", payment.Amount);
                
                await connection.OpenAsync();
                return (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task UpdatePaymentStatusAsync(int checkoutId, string status)
        {
            // Note: Your Checkout table doesn't have a status column, so this is optional
            const string sql = "UPDATE Checkout SET status = @Status WHERE checkout_id = @CheckoutId";
            
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CheckoutId", checkoutId);
                command.Parameters.AddWithValue("@Status", status);
                
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
