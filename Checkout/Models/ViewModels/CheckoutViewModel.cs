using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Checkout.Models.ViewModels
{
    /// <summary>
    /// Frontend-specific model for checkout page binding
    /// </summary>
    public class CheckoutViewModel
    {
        [Required]
        public string CustomerName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public AddressViewModel ShippingAddress { get; set; }

        [Required]
        public PaymentMethodViewModel PaymentMethod { get; set; }

        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CartItemViewModel> Items { get; set; } = new();

        public class AddressViewModel
        {
            [Required, MaxLength(100)]
            public string Street { get; set; }

            [Required, MaxLength(50)]
            public string City { get; set; }

            [Required, MaxLength(20)]
            public string PostalCode { get; set; }
        }

        public class PaymentMethodViewModel
        {
            [Required, CreditCard]
            public string CardNumber { get; set; }

            [Required, RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$")]
            public string ExpiryDate { get; set; }

            [Required, StringLength(4, MinimumLength = 3)]
            public string CVV { get; set; }
        }

        public class CartItemViewModel
        {
            [Required]
            public string BookId { get; set; }

            [Range(1, 100)]
            public int Quantity { get; set; }
        }
    }
}
