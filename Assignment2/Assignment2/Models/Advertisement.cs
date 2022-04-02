using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class Advertisement
    {
        //[Required]
        //[Display(Name = "Advertisement Id")]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
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