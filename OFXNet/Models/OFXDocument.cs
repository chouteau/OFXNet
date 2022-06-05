using OFXNet.Enums;

namespace OFXNet.Models
{
    public class OFXDocument
    {
        public DateTime StatementStart { get; set; }
        public DateTime StatementEnd { get; set; }
        public AccountType AccountType { get; set; }
        public string? Currency { get; set; }
        public SignOn? SignOn { get; set; }
        public Account? Account { get; set; }
        public Balance? Balance { get; set; }
        public List<Transaction> Transactions { get; set; }

        public OFXDocument()
        {
            Transactions = new List<Transaction>();
        }
    }
}