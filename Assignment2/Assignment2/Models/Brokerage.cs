﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class Brokerage
    {
        [Required]
        [Display(Name = "Registration Number")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id
        {
            get;
            set;
        }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Title
        {
            get;
            set;
        }

        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Fee
        {
            get;
            set;
        }

        public IList<Subscription> Subscriptions
        {
            get;
            set;
        }
    }
}
