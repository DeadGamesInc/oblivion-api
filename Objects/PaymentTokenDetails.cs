/*
 *  OblivionAPI :: PaymentTokenDetails
 *
 *  This class is used to store the details of a payment token.
 * 
 */

namespace OblivionAPI.Objects; 

public class PaymentTokenDetails {
    public string Address { get; set; }
    public string Symbol { get; set; }
    public string CoinGeckoKey { get; set; }
    public decimal Price { get; set; }
    public int Decimals { get; set; } = 18;
}