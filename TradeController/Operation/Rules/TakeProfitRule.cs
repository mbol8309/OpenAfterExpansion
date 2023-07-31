using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cAlgo.API;

namespace MBR.Operation.Rules
{
    public class TakeProfitRule : Operation.Rules.Rule
    {
        public double AtPrice { get; set; }
        public bool IsPercent { get; set; }

        public double CloseAmount { get; set; }
        public override RuleExecutionResult Execute()
        {
            RuleExecutionResult result = RuleExecutionResult.None;
            if (this.Operation.OperationMode != OperationMode.Position)
            {
                return result;
            }
            Position pos = (Position)this.Operation;
            cAlgo.API.Position cpos = Rule.Positions.Where(x => x.Id == pos.Id).FirstOrDefault();

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
            cAlgo.API.Position cpos = Rule.Positions.Where(x => x.Id == pos.Id).FirstOrDefault();
            if (pos.OperationType == OperationType.Sell && cpos != null)
            {
                return Symbol.Bid <= this.AtPrice;
            }
            if (pos.OperationType == OperationType.Buy )
            {
                return Symbol.Ask >= this.AtPrice;
            }


            return results;
        }

        public override void PostExecute()
        {
            throw new NotImplementedException();
        }

        public override void PreExecute()
        {
            TradeController.Instance().Print(String.Format("Operation ID:{0} is going to take profit", this.Operation.Id));
        }

        public TakeProfitRule(OperationBase obj, double atPrice,double closeAmount, bool isPercent=false ) : base(obj, RuleExecutionIteration.Once)
        {
            this.AtPrice = atPrice;
            this.CloseAmount = closeAmount;
            this.IsPercent = isPercent;
        }
    }
}