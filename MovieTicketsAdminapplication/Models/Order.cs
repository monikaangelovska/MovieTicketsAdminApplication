using System;
using System.Collections.Generic;

namespace MovieTicketsAdminapplication.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<MovieInOrder> MovieInOrders { get; set; }
    }
}
