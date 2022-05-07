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
                
                report.Collections.Add(new SalesReport_CollectionVolume(collection.ID, collectionVolume, collection.Name, collection.Image));
            }

            var releaseSales = await _database.GetReleaseSales(chainID);
            if (releaseSales == null) return report;

            var releaseSales24Hour = releaseSales.Where(a => DateTime.Now - a.SaleTime < TimeSpan.FromHours(24));
            report.TotalReleaseSales = releaseSales24Hour.Count();

            decimal totalReleaseVolume = 0;
            
            foreach (var token in payments.PaymentTokens) {
                var tokenSales = releaseSales24Hour.Where(a => a.PaymentToken == token.Address);
                var tokenVolume = new BigInteger();
                tokenVolume = tokenSales.Select(sale => BigInteger.Parse(sale.Price) * sale.Quantity).Aggregate(tokenVolume, (current, amount) => current + amount);
                totalReleaseVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
            }

            report.TotalReleaseVolume = totalReleaseVolume;

            var releases = await _database.GetReleases(chainID);
            
            foreach (var release in releases) {
                var releaseSalesCheck = releaseSales24Hour.Where(a => a.ID == release.ID);
                decimal releaseVolume = 0;
                
                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = releaseSalesCheck.Where(a => a.PaymentToken == token.Address);
                    
                    var tokenVolume = new BigInteger();
                    tokenVolume = tokenSales.Select(sale => BigInteger.Parse(sale.Price) * sale.Quantity).Aggregate(tokenVolume, (current, amount) => current + amount);

                    releaseVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
                }
                
                report.Releases.Add(new SalesReport_ReleaseVolume(release.ID, releaseVolume));
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
