using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using OblivionAPI.Config;
using OblivionAPI.Objects;
using OblivionAPI.Reports;

namespace OblivionAPI.Services {
    public class ReportsService {
        private readonly DatabaseService _database;
        private readonly LookupService _lookup;

        public ReportsService(DatabaseService database, LookupService lookup) {
            _database = database;
            _lookup = lookup;
        }

        public async Task<SalesReport_24HVolume> SalesReport_24HVolume(ChainID chainID) {
            var payments = Globals.Payments.Find(a => a.ChainID == chainID);
            if (payments == null) return null;
            
            var report = new SalesReport_24HVolume();

            var sales = await _database.GetSales(chainID);
            if (sales == null) return null;
            
            var sales24Hour = sales.Where(a => DateTime.Now - a.SaleDate < TimeSpan.FromHours(24));

            report.TotalSales = sales24Hour.Count();

            decimal totalVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = sales24Hour.Where(a => a.PaymentToken == token.Address);
                var tokenVolume = new BigInteger();
                tokenVolume = tokenSales.Aggregate(tokenVolume, (current, sale) => BigInteger.Add(current, BigInteger.Parse(sale.Amount)));
                totalVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
            }

            report.TotalVolume = totalVolume;

            var collections = await _database.GetCollections(chainID);

            foreach (var collection in collections) {
                var collectionSales = sales24Hour.Where(a => a.CollectionId == collection.ID);
                decimal collectionVolume = 0;
                
                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = collectionSales.Where(a => a.PaymentToken == token.Address);
                    
                    var tokenVolume = new BigInteger();
                    tokenVolume = tokenSales.Aggregate(tokenVolume, (current, sale) => BigInteger.Add(current, BigInteger.Parse(sale.Amount)));

                    collectionVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
                }
                
                report.Collections.Add(new SalesReport_CollectionVolume(collection.ID, collectionVolume));
            }

            return report;
        }

        private async Task<decimal> ConvertTokensToUSD(BigInteger tokens, int decimals, string coinGeckoKey) {
            var ratio = BigInteger.Pow(10, decimals);

            var tokenAmount = (double)tokens / (double)ratio;
            var price = await _lookup.GetCurrentPrice(coinGeckoKey);

            return (decimal)tokenAmount * price;
        }
    }
}
