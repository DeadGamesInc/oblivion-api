﻿/*
 *  OblivionAPI :: PaymentDetails
 *
 *  This class is used to store the details of approved payment methods by blockchain ID on the market.
 * 
 */

using System.Collections.Generic;

namespace OblivionAPI.Objects; 

public class PaymentDetails {
    public ChainID ChainID;
    public List<PaymentTokenDetails> PaymentTokens;
}