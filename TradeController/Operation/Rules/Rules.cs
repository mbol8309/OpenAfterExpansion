using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cAlgo.API.Internals;
using MBR.Operation.Rules;

namespace MBR.Operation.Rules
{
    public abstract class Rule
    {
        public bool ExecutedOnce { set; get; }
        public RuleExecutionIteration Iterations { get; set; }
        public abstract bool IsValid();
        public virtual void Initialize() { }
        public abstract void PreExecute();
        public abstract void PostExecute();

        #region EasyAccessMehtods
        public Symbol Symbol { get { return this.Operation.Symbol; } }
        public static cAlgo.API.Positions Positions { get { return TradeController.Instance().CAlgoPositions; } }
        public static cAlgo.API.PendingOrders PendingOrders { get {return TradeController.Instance().CAlgoPendingOrders; }
}
        #endregion
        public abstract RuleExecutionResult Execute();

        public OperationBase Operation { get; set; }
        public RuleFrequency RuleFrequency { get; set; }

        public Rule(OperationBase operation, RuleExecutionIteration iteration = RuleExecutionIteration.Once)
        {
            RuleFrequency = RuleFrequency.OnTick;
            Operation = operation;
            ExecutedOnce = false;

            this.Iterations = iteration;
        }
    }
}