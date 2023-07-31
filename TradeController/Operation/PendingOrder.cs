using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cAlgo.API.Internals;

namespace MBR.Operation
{
    public class PendingOrder : OperationBase
    {
        public double EntryPrice { get; set; }

        public PendingOrder(int id, OperationType type, double stopLost, double takeProfit, Symbol symbol, double entryPrice) : base(id, type, stopLost, takeProfit, symbol, OperationMode.PendingOrder)
        {
            EntryPrice = entryPrice;
        }

        public void Update(cAlgo.API.PendingOrder pos)
        {
            this.TakeProfit = pos.TakeProfit;
            this.StopLost = Convert.ToDouble(pos.StopLoss);
            this.EntryPrice = pos.TargetPrice;
        }
    }
}