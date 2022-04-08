using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class Advertisement
    {
        //No annotations at present as the Entity Framework is handling the details and we don't want another foray into Migration Hell.
        public int Id { get; set; }

        [Required]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Required]
        [Url]
        [Display(Name = "Url")]
        public string Url { get; set; }

        //This here because Brokerage can have many ads
        public string BrokerageId
        {
            get;
            set;
        }

        public Brokerage Brokerage
        {
            get;
            set;
        }
    }
}