using Rabit.Info;

namespace Rabit.Models
{
    public class Daily
    {
        public string Name { get; set; }
        public string Market { get; set; }
        public BuySell Side { get; set; } // Buy, Sell
        public decimal JunilClose { get; set; }
    }
}