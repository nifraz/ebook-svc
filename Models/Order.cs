﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Net;

namespace ebook_svc.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Placed";  // could be used for future (e.g., shipped, delivered)

        public int? AddressId { get; set; }   // shipping address used
        [ValidateNever]
        public Address Address { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = [];
    }
}
