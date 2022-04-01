namespace Assignment2.Models.ViewModels
{
    public class BrokerageViewModel
    {
        public IEnumerable<Client> Clients { get; set; }
        public IEnumerable<Brokerage> Brokerages { get; set; }
        public IEnumerable<Subscription> Subscriptions { get; set; }
    }

    public class FileInputViewModel
    {
        public string BrokerageId { get; set; }
        public string BrokerageTitle { get; set; }
        public IFormFile File { get; set; }
    }

    public class AdsViewModel
    {
        public Brokerage Brokerage { get; set; }
        public IEnumerable<Advertisement> Advertisements { get; set; }
    }

    public class ClientSubscriptionsViewModel
    {
        public Client Client { get; set; }
        public IEnumerable<BrokerageSubscriptionsViewModel> Subscriptions { get; set; }
    }

    public class BrokerageSubscriptionsViewModel
    {
        public string BrokerageId { get; set; }
        public string Title { get; set; }
        public bool IsMember { get; set; }
    }

}
