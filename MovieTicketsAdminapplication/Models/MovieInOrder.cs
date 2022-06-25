using System;

namespace MovieTicketsAdminapplication.Models
{
    public class MovieInOrder
    {
        public Guid MovieId { get; set; }
        public Movie Movie { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public int Amount { get; set; }
    }
}
