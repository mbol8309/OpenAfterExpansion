
using System.Data;
using System.Runtime.CompilerServices;
using System.Security;
using cAlgo.API;
using cAlgo.API.Internals;
using MBR.Operation;
using MBR.Operation.Rules;

namespace MBR
{

    [Robot(AccessRights = AccessRights.None)]
    public class TradeController : Robot
    {
        //for test
        public static bool OpenTrade()
        {
            
            return true;
        }

        // public Queue<QueueOperation> QueuedOperations { get; } = new Queue<QueueOperation>();

        // public static void AddQueueOperation(QueueOperation operation){
        //     TradeController.Instance().QueuedOperations.Enqueue(operation);
        // }
        #region Properties
        public cAlgo.API.Positions CAlgoPositions { get; set; }
        public cAlgo.API.PendingOrders CAlgoPendingOrders { get; set; }

        private Symbol symbol;

        private bool Registered = false;

        private static TradeController? tradeController = null;

        public List<OperationBase> Operations { get; } = new List<OperationBase>();
        #endregion

        private TradeController() : base()
        {

        }

        private IEnumerable<Operation.PendingOrder> GetPendingOrders()
        {
            return Operations.Where(x => x.OperationMode == OperationMode.PendingOrder).Select(x => (Operation.PendingOrder)x);
        }

        private Operation.PendingOrder? GetPendingOrderById(int id)
        {
            Operation.PendingOrder? po = GetPendingOrders().Where(x => x.Id == id).FirstOrDefault();
            return po;
        }

        public void Print(string text)
        {
            this.Print(text);
        }

        private IEnumerable<Operation.Position> GetPositions()
        {
            return Operations.Where(x => x.OperationMode == OperationMode.Position).Select(x => (Operation.Position)x);
        }

        private Operation.Position? GetPositionsById(int id)
        {
            Operation.Position? po = GetPositions().Where(x => x.Id == id).FirstOrDefault();
            return po;
        }

        public TradeController Register(Robot robot)
        {
            robot.Symbol.Tick += this.OnTick;
            robot.Bars.BarClosed += this.OnBar;

            robot.PendingOrders.Cancelled += this.OrderCancelled;
            robot.PendingOrders.Filled += this.OrderFilled;

            robot.Positions.Closed += this.PositionClosed;
            robot.Positions.Modified += this.PositionModified;

            this.symbol = robot.Symbol;

            this.CAlgoPositions = robot.Positions;
            this.CAlgoPendingOrders = robot.PendingOrders;

            this.Registered = true;
            return this;
        }

        public static TradeController Instance()
        {
            if (tradeController == null)
            {
                tradeController = new TradeController();
            }
            return (TradeController)tradeController;
        }

        public bool AddPendingOrder(OperationType operationType, double entryPrice, double stopLostPips, double takeProfitPips, double lots, List<Operation.Rules.Rule> rules = null)
        {
            double volume = symbol.QuantityToVolumeInUnits(lots);
            //sell stop
            OperationBase? op = null;
            if (operationType == OperationType.SellStop)
            {
                TradeResult result = PlaceStopOrder(TradeType.Sell, symbol.Name, volume, entryPrice, "Auto operation", stopLostPips, takeProfitPips);
                if (!result.IsSuccessful) return false;
                cAlgo.API.PendingOrder p = result.PendingOrder;

                op = new Operation.PendingOrder(p.Id, OperationType.SellStop, (double)p.StopLoss, (double)p.TakeProfit, p.Symbol, entryPrice);
            }
            //sell limit
            if (operationType == OperationType.SellLimit)
            {
                TradeResult result = PlaceLimitOrder(TradeType.Sell, symbol.Name, volume, entryPrice, "Auto operation", stopLostPips, takeProfitPips);
                if (!result.IsSuccessful) return false;
                cAlgo.API.PendingOrder p = result.PendingOrder;

                op = new Operation.PendingOrder(p.Id, OperationType.SellLimit, (double)p.StopLoss, (double)p.TakeProfit, p.Symbol, entryPrice);
            }
            //buy limit
            if (operationType == OperationType.BuyLimit)
            {
                TradeResult result = PlaceLimitOrder(TradeType.Sell, symbol.Name, volume, entryPrice, "Auto operation", stopLostPips, takeProfitPips);
                if (!result.IsSuccessful) return false;
                cAlgo.API.PendingOrder p = result.PendingOrder;

                op = new Operation.PendingOrder(p.Id, OperationType.BuyLimit, (double)p.StopLoss, (double)p.TakeProfit, p.Symbol, entryPrice);
            }
            //buy stop
            if (operationType == OperationType.BuyStop)
            {
                TradeResult result = PlaceStopOrder(TradeType.Sell, symbol.Name, volume, entryPrice, "Auto operation", stopLostPips, takeProfitPips);
                if (!result.IsSuccessful) return false;
                cAlgo.API.PendingOrder p = result.PendingOrder;

                op = new Operation.PendingOrder(p.Id, OperationType.BuyStop, (double)p.StopLoss, (double)p.TakeProfit, p.Symbol, entryPrice);
            }

            if (op != null)
            {
                op.Rules.AddRange(rules);
                Operations.Add(op);
                return true;
            }

            return false;
        }

        public bool AddPosition(OperationType operationType, double stopLostPips, double takeProfitPips, double lots, List<Operation.Rules.Rule> rules = null)
        {
            double volume = symbol.QuantityToVolumeInUnits(lots);
            //sell stop
            OperationBase? op = null;
            if (operationType == OperationType.Sell)
            {
                TradeResult result = ExecuteMarketOrder(TradeType.Sell, symbol.Name, volume, "Auto operation", stopLostPips, takeProfitPips);
                if (!result.IsSuccessful) return false;
                cAlgo.API.Position p = result.Position;

                op = new Operation.Position(p.Id, OperationType.Sell, (double)p.StopLoss, p.TakeProfit, p.Symbol, p.EntryPrice, DateTime.Now);
            }

            if (operationType == OperationType.Buy)
            {
                TradeResult result = ExecuteMarketOrder(TradeType.Buy, symbol.Name, volume, "Auto operation", stopLostPips, takeProfitPips);
                if (!result.IsSuccessful) return false;
                cAlgo.API.Position p = result.Position;

                op = new Operation.Position(p.Id, OperationType.Buy, (double)p.StopLoss, p.TakeProfit, p.Symbol, p.EntryPrice, DateTime.Now);
            }

            if (op != null)
            {
                op.Rules.AddRange(rules);
                Operations.Add(op);
                return true;
            }

            return false;
        }

        #region Events
        public void OrderFilled(PendingOrderFilledEventArgs obj)
        {
            int id = obj.PendingOrder.Id;
            Operation.PendingOrder? po = this.GetPendingOrderById(id);
            if (po != null)
            {
                OperationType type = OperationType.Sell;
                if (obj.Position.TradeType == TradeType.Buy) type = OperationType.Buy;
                if (obj.Position.TradeType == TradeType.Sell) type = OperationType.Sell;

                Operation.Position p = new Operation.Position(obj.Position.Id, type, Convert.ToDouble(obj.Position.StopLoss), obj.Position.TakeProfit, obj.Position.Symbol, obj.Position.EntryPrice, obj.Position.EntryTime);
                Operations.Add(p);
                Operations.Remove(po);
            }
        }

        public void OrderCancelled(PendingOrderCancelledEventArgs obj)
        {
            Operation.PendingOrder? po = this.GetPendingOrderById(obj.PendingOrder.Id);
            if (po != null)
            {
                this.Operations.Remove(po);
            }
        }

        public void OrderModified(PendingOrderModifiedEventArgs obj)
        {
            Operation.PendingOrder? po = this.GetPendingOrderById(obj.PendingOrder.Id);
            if (po != null)
            {
                po.Update(obj.PendingOrder);
            }
        }

        public void PositionClosed(PositionClosedEventArgs obj)
        {
            Operation.Position? pos = GetPositionsById(obj.Position.Id);
            if (pos != null)
            {
                this.Operations.Remove(pos);
            }
        }

        public void PositionModified(PositionModifiedEventArgs obj)
        {
            Operation.Position? pos = GetPositionsById(obj.Position.Id);
            if (pos != null)
            {
                pos.Update(obj.Position);
            }
        }

        private void OnTick(SymbolTickEventArgs obj)
        {
            Parallel.ForEach<OperationBase>(Operations, operation =>
            {
                OperationResult result = operation.EvaluateRules(RuleFrequency.OnTick);
                if (result == OperationResult.Delete)
                {
                    Operations.Remove(operation);
                }
            });
        }

        private void OnBar(BarClosedEventArgs obj)
        {
            Parallel.ForEach<OperationBase>(Operations, operation =>
            {
                OperationResult result = operation.EvaluateRules(RuleFrequency.OnBar);
                if (result == OperationResult.Delete)
                {
                    Operations.Remove(operation);
                }
            });
        }
        #endregion
    }
}
