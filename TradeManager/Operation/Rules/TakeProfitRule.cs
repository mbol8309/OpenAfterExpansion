using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cAlgo.API;

namespace MBR.Operation.Rules
{
    public class TakeProfitRule : Operation.Rules.Rule
    {
        public double AtPrice { get; }
        public bool IsPercent { get; }

        public double CloseAmount { get; }
        public override RuleExecutionResult Execute()
        {
            RuleExecutionResult result = RuleExecutionResult.None;
            if (this.Operation.OperationMode != OperationMode.Position)
            {
                return result;
            }
            Position pos = (Position)this.Operation;
            cAlgo.API.Position? cpos = Rule.Positions.Where(x => x.Id == pos.Id).FirstOrDefault();

            if (cpos != null)
            {
                double lots = this.Symbol.VolumeInUnitsToQuantity(cpos.VolumeInUnits);
                double closingLots = IsPercent ? (lots * CloseAmount) : (CloseAmount < lots ? CloseAmount : lots);
                TradeResult tradeResults = cpos.ModifyVolume(lots - this.Symbol.QuantityToVolumeInUnits(closingLots));
                if (tradeResults.IsSuccessful){
                    result = RuleExecutionResult.ModifiedOperation;
                    pos.Update(tradeResults.Position);
                }
            }

            return result;
        }

        public override bool IsValid()
        {
            if (this.Operation.OperationMode != OperationMode.Position)
            {
                return false;
            }
            bool results = false;
            Position pos = (Position)this.Operation;
            cAlgo.API.Position? cpos = Rule.Positions.Where(x => x.Id == pos.Id).FirstOrDefault();
            if (pos.OperationType == OperationType.Sell && cpos != null)
            {
                return cpos.CurrentPrice <= this.AtPrice;
            }
            if (pos.OperationType == OperationType.Buy )
            {
                return cpos.CurrentPrice >= this.AtPrice;
            }


            return results;
        }

        public override void PostExecute()
        {
            throw new NotImplementedException();
        }

        public override void PreExecute()
        {
            TradeController.Instance().Print($"Operation ID:{this.Operation.Id} is going to take profit");
        }

        public TakeProfitRule(OperationBase obj, double atPrice,double closeAmount, bool isPercent=false ) : base(obj, RuleExecutionIteration.Once)
        {
            this.AtPrice = atPrice;
            this.CloseAmount = closeAmount;
            this.IsPercent = isPercent;
        }
    }
}