﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DataAccess.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }

        public string? Img { get; set; }
        public DateTime Created { get; set; }
        public bool IsAvail { get; set; }

        public bool ShowHomePage { get; set; }
        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public Author? Author { get; set; }

        public ICollection<BasketItems>? BasketItems { get; set; }
    }
}
