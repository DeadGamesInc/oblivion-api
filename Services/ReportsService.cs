using System.Linq;
using System.Numerics;

namespace OblivionAPI.Services; 

public class ReportsService {
    private readonly DatabaseService _database;
    private readonly LookupService _lookup;

    public ReportsService(DatabaseService database, LookupService lookup) {
        _database = database;
        _lookup = lookup;
    }

    public async Task<SalesReport_Volume> SalesReport_24HVolume(ChainID chainID) =>
        await SalesReport_VolumeReport(chainID, TimeSpan.FromHours(24));

    public async Task<SalesReport_Volume> SalesReport_30DVolume(ChainID chainID) =>
        await SalesReport_VolumeReport(chainID, TimeSpan.FromDays(30));

    public async Task<SalesReport_Volume> SalesReport_CurrentMonthVolume(ChainID chainID) =>
        await SalesReport_MonthlyVolumeReport(chainID, DateTime.Now);

    public async Task<SalesReport_Volume> SalesReport_PreviousMonthVolume(ChainID chainID) =>
        await SalesReport_MonthlyVolumeReport(chainID, DateTime.Now.AddMonths(-1));

    private async Task<SalesReport_Volume> SalesReport_VolumeReport(ChainID chainID, TimeSpan timeframe) {
        var payments = Globals.Payments.Find(a => a.ChainID == chainID);
        if (payments == null) return null;

        var report = new SalesReport_Volume();
        var collections = await _database.GetCollections(chainID);

        var sales = await _database.GetSales(chainID);
        var sales1155 = await _database.GetSales1155(chainID);

        if (sales != null) {
            var salesPeriod = sales.Where(a => DateTime.Now - a.SaleDate < timeframe);

            report.TotalSales = salesPeriod.Count();

            decimal totalVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = salesPeriod.Where(a => a.PaymentToken == token.Address);
                var tokenVolume = new BigInteger();
                tokenVolume = tokenSales.Aggregate(tokenVolume,
                    (current, sale) => BigInteger.Add(current, BigInteger.Parse(sale.Amount)));
                totalVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
            }

            report.TotalVolume = totalVolume;

            foreach (var collection in collections) {
                var collectionSales = salesPeriod.Where(a => a.CollectionId == collection.ID);
                decimal collectionVolume = 0;

                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = collectionSales.Where(a => a.PaymentToken == token.Address);

                    var tokenVolume = new BigInteger();
                    tokenVolume = tokenSales.Aggregate(tokenVolume,
                        (current, sale) => BigInteger.Add(current, BigInteger.Parse(sale.Amount)));

                    collectionVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
                }

                report.Collections.Add(new SalesReport_CollectionVolume(collection.ID, collectionVolume, collection.Name,
                    collection.Image));
            }
        }

        if (sales1155 != null) {
            var salesPeriod = sales1155.Where(a => DateTime.Now - a.SaleDate < timeframe);

            report.TotalSales1155 = salesPeriod.Count();

            decimal totalVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = salesPeriod.Where(a => a.PaymentToken == token.Address);
                var tokenVolume = new BigInteger();
                tokenVolume = tokenSales.Aggregate(tokenVolume,
                    (current, sale) => BigInteger.Add(current, BigInteger.Parse(sale.Amount)));
                totalVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
            }

            report.TotalVolume1155 = totalVolume;

            foreach (var collection in collections) {
                var collectionSales = salesPeriod.Where(a => a.CollectionId == collection.ID);
                decimal collectionVolume = 0;

                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = collectionSales.Where(a => a.PaymentToken == token.Address);

                    var tokenVolume = new BigInteger();
                    tokenVolume = tokenSales.Aggregate(tokenVolume,
                        (current, sale) => BigInteger.Add(current, BigInteger.Parse(sale.Amount)));

                    collectionVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
                }

                report.Collections1155.Add(new SalesReport_CollectionVolume(collection.ID, collectionVolume, collection.Name,
                    collection.Image));
            }
        }
        

        var releaseSales = await _database.GetReleaseSales(chainID);
        var releaseSales1155 = await _database.GetReleaseSales1155(chainID);

        if (releaseSales != null) {
            var releaseSalesPeriod = releaseSales.Where(a => DateTime.Now - a.SaleTime < timeframe);
            report.TotalReleaseSales = releaseSalesPeriod.Count();

            decimal totalReleaseVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = releaseSalesPeriod.Where(a => a.PaymentToken == token.Address);
                var tokenVolume = new BigInteger();
                tokenVolume = tokenSales.Select(sale => BigInteger.Parse(sale.Price) * sale.Quantity)
                    .Aggregate(tokenVolume, (current, amount) => current + amount);
                totalReleaseVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
            }

            report.TotalReleaseVolume = totalReleaseVolume;

            var releases = await _database.GetReleases(chainID);

            foreach (var release in releases) {
                var releaseSalesCheck = releaseSalesPeriod.Where(a => a.ID == release.ID);
                decimal releaseVolume = 0;

                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = releaseSalesCheck.Where(a => a.PaymentToken == token.Address);

                    var tokenVolume = new BigInteger();
                    tokenVolume = tokenSales.Select(sale => BigInteger.Parse(sale.Price) * sale.Quantity)
                        .Aggregate(tokenVolume, (current, amount) => current + amount);

                    releaseVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
                }

                var collection = collections.Find(c => c.Nfts.Contains(release.NFT));
                var releaseCollection = collection is null ? null : new ReleaseCollection(collection.ID, collection.Name, collection.Image);

                report.Releases.Add(new SalesReport_ReleaseVolume(release.ID, releaseVolume, releaseCollection));
            }
        }

        if (releaseSales1155 != null) {
            var releaseSalesPeriod = releaseSales1155.Where(a => DateTime.Now - a.SaleTime < timeframe);
            report.TotalReleaseSales1155 = releaseSalesPeriod.Count();

            decimal totalReleaseVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = releaseSalesPeriod.Where(a => a.PaymentToken == token.Address);
                var tokenVolume = new BigInteger();
                tokenVolume = tokenSales.Select(sale => BigInteger.Parse(sale.Price) * sale.Quantity)
                    .Aggregate(tokenVolume, (current, amount) => current + amount);
                totalReleaseVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
            }

            report.TotalReleaseVolume1155 = totalReleaseVolume;

            var releases = await _database.GetReleases1155(chainID);

            foreach (var release in releases) {
                var releaseSalesCheck = releaseSalesPeriod.Where(a => a.ID == release.ID);
                decimal releaseVolume = 0;

                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = releaseSalesCheck.Where(a => a.PaymentToken == token.Address);

                    var tokenVolume = new BigInteger();
                    tokenVolume = tokenSales.Select(sale => BigInteger.Parse(sale.Price) * sale.Quantity)
                        .Aggregate(tokenVolume, (current, amount) => current + amount);

                    releaseVolume += await ConvertTokensToUSD(tokenVolume, token.Decimals, token.CoinGeckoKey);
                }

                var collection = collections.Find(c => c.Nfts.Contains(release.NFT));
                var releaseCollection = collection is null ? null : new ReleaseCollection(collection.ID, collection.Name, collection.Image);

                report.Releases1155.Add(new SalesReport_ReleaseVolume(release.ID, releaseVolume, releaseCollection));
            }
        }

        return report;
    }
        
    private async Task<SalesReport_Volume> SalesReport_MonthlyVolumeReport(ChainID chainID, DateTime month) {
        var payments = Globals.Payments.Find(a => a.ChainID == chainID);
        if (payments == null) return null;

        var report = new SalesReport_Volume();
        var collections = await _database.GetCollections(chainID);

        var sales = await _database.GetSales(chainID);
        var sales1155 = await _database.GetSales1155(chainID);

        if (sales != null) {
            var salesPeriod = sales.Where(a => a.SaleDate.Month == month.Month && a.SaleDate.Year == month.Year);

            report.TotalSales = salesPeriod.Count();

            decimal totalVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = salesPeriod.Where(a => a.PaymentToken == token.Address);
                decimal tokenVolume = 0;
                foreach (var sale in tokenSales) {
                    tokenVolume += await ConvertTokensToUSD(BigInteger.Parse(sale.Amount), token.Decimals, token.CoinGeckoKey,
                        sale.SaleDate);
                }
                totalVolume += tokenVolume;
            }

            report.TotalVolume = totalVolume;

            foreach (var collection in collections) {
                var collectionSales = salesPeriod.Where(a => a.CollectionId == collection.ID);
                decimal collectionVolume = 0;

                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = collectionSales.Where(a => a.PaymentToken == token.Address);
                    decimal tokenVolume = 0;
                    foreach (var sale in tokenSales) {
                        tokenVolume += await ConvertTokensToUSD(BigInteger.Parse(sale.Amount), token.Decimals, token.CoinGeckoKey,
                            sale.SaleDate);
                    }
                    collectionVolume += tokenVolume;
                }

                report.Collections.Add(new SalesReport_CollectionVolume(collection.ID, collectionVolume, collection.Name,
                    collection.Image));
            }
        }

        if (sales1155 != null) {
            var salesPeriod = sales1155.Where(a => a.SaleDate.Month == month.Month && a.SaleDate.Year == month.Year);

            report.TotalSales1155 = salesPeriod.Count();

            decimal totalVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = salesPeriod.Where(a => a.PaymentToken == token.Address);
                decimal tokenVolume = 0;
                foreach (var sale in tokenSales) {
                    tokenVolume += await ConvertTokensToUSD(BigInteger.Parse(sale.Amount), token.Decimals, token.CoinGeckoKey,
                        sale.SaleDate);
                }
                totalVolume += tokenVolume;
            }

            report.TotalVolume1155 = totalVolume;

            foreach (var collection in collections) {
                var collectionSales = salesPeriod.Where(a => a.CollectionId == collection.ID);
                decimal collectionVolume = 0;

                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = collectionSales.Where(a => a.PaymentToken == token.Address);
                    decimal tokenVolume = 0;
                    foreach (var sale in tokenSales) {
                        tokenVolume += await ConvertTokensToUSD(BigInteger.Parse(sale.Amount), token.Decimals, token.CoinGeckoKey,
                            sale.SaleDate);
                    }
                    collectionVolume += tokenVolume;
                }

                report.Collections1155.Add(new SalesReport_CollectionVolume(collection.ID, collectionVolume, collection.Name,
                    collection.Image));
            }
        }

        var releaseSales = await _database.GetReleaseSales(chainID);
        var releaseSales1155 = await _database.GetReleaseSales1155(chainID);

        if (releaseSales != null) {
            var releaseSalesPeriod = releaseSales.Where(a => a.SaleTime.Month == month.Month && a.SaleTime.Year == month.Year);
            report.TotalReleaseSales = releaseSalesPeriod.Count();

            decimal totalReleaseVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = releaseSalesPeriod.Where(a => a.PaymentToken == token.Address);
                decimal tokenVolume = 0;
                foreach (var sale in tokenSales) {
                    tokenVolume += await ConvertTokensToUSD(BigInteger.Parse(sale.Price) * sale.Quantity, token.Decimals, token.CoinGeckoKey,
                        sale.SaleTime);
                }
                totalReleaseVolume += tokenVolume;
            }

            report.TotalReleaseVolume = totalReleaseVolume;

            var releases = await _database.GetReleases(chainID);

            foreach (var release in releases) {
                var releaseSalesCheck = releaseSalesPeriod.Where(a => a.ID == release.ID);
                decimal releaseVolume = 0;

                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = releaseSalesCheck.Where(a => a.PaymentToken == token.Address);
                    decimal tokenVolume = 0;
                    foreach (var sale in tokenSales) {
                        tokenVolume += await ConvertTokensToUSD(BigInteger.Parse(sale.Price) * sale.Quantity, token.Decimals, token.CoinGeckoKey,
                            sale.SaleTime);
                    }
                    releaseVolume += tokenVolume;
                }

                var collection = collections.Find(c => c.Nfts.Contains(release.NFT));
                var releaseCollection = collection is null ? null : new ReleaseCollection(collection.ID, collection.Name, collection.Image);
                
                report.Releases.Add(new SalesReport_ReleaseVolume(release.ID, releaseVolume, releaseCollection));
            }
        }

        if (releaseSales1155 != null) {
            var releaseSalesPeriod = releaseSales1155.Where(a => a.SaleTime.Month == month.Month && a.SaleTime.Year == month.Year);
            report.TotalReleaseSales1155 = releaseSalesPeriod.Count();

            decimal totalReleaseVolume = 0;

            foreach (var token in payments.PaymentTokens) {
                var tokenSales = releaseSalesPeriod.Where(a => a.PaymentToken == token.Address);
                decimal tokenVolume = 0;
                foreach (var sale in tokenSales) {
                    tokenVolume += await ConvertTokensToUSD(BigInteger.Parse(sale.Price) * sale.Quantity, token.Decimals, token.CoinGeckoKey,
                        sale.SaleTime);
                }
                totalReleaseVolume += tokenVolume;
            }

            report.TotalReleaseVolume1155 = totalReleaseVolume;

            var releases = await _database.GetReleases(chainID);

            foreach (var release in releases) {
                var releaseSalesCheck = releaseSalesPeriod.Where(a => a.ID == release.ID);
                decimal releaseVolume = 0;

                foreach (var token in payments.PaymentTokens) {
                    var tokenSales = releaseSalesCheck.Where(a => a.PaymentToken == token.Address);
                    decimal tokenVolume = 0;
                    foreach (var sale in tokenSales) {
                        tokenVolume += await ConvertTokensToUSD(BigInteger.Parse(sale.Price) * sale.Quantity, token.Decimals, token.CoinGeckoKey,
                            sale.SaleTime);
                    }
                    releaseVolume += tokenVolume;
                }

                var collection = collections.Find(c => c.Nfts.Contains(release.NFT));
                var releaseCollection = collection is null ? null : new ReleaseCollection(collection.ID, collection.Name, collection.Image);
                
                report.Releases1155.Add(new SalesReport_ReleaseVolume(release.ID, releaseVolume, releaseCollection));
            }
        }

        return report;
    }

    private async Task<decimal> ConvertTokensToUSD(BigInteger tokens, int decimals, string coinGeckoKey, DateTime? date = null) {
        var ratio = BigInteger.Pow(10, decimals);

        var tokenAmount = (double)tokens / (double)ratio;
        decimal price;
        if (date == null) price = await _lookup.GetCurrentPrice(coinGeckoKey);
        else price = await _lookup.GetHistoricalPrice(coinGeckoKey, date);

        return (decimal)tokenAmount * price;
    }
}