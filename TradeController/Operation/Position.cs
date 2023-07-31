using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cAlgo.API.Internals;

namespace MBR.Operation
{
    public class Position : OperationBase
    {
        public DateTime OpenedAt { get; set; }

        public double EnterAtPrice { get; set; }

        public Position(int id,OperationType type, double stopLost, double? takeProfit, Symbol symbol, double? priceEntered = null, DateTime? openTime = null) : base(id, type, stopLost, takeProfit, symbol, OperationMode.Position)
        {
            OpenedAt = openTime != null ? (DateTime)openTime : DateTime.Now;
            EnterAtPrice = priceEntered != null ? (double)priceEntered : (new List<OperationType>() { OperationType.Sell, OperationType.SellLimit, OperationType.SellStop }.Contains(this.OperationType) ? Symbol.Bid : Symbol.Ask);

        }
        public void Update(cAlgo.API.Position pos)
        {
            this.TakeProfit = pos.TakeProfit;
            this.StopLost = Convert.ToDouble(pos.StopLoss);
        }
    }
}