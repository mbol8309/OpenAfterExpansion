using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using cAlgo.API.Internals;
using MBR.Operation.Rules;
using MBR.Operation.Rules;

namespace MBR.Operation
{
    public class OperationBase
    {
        public OperationType OperationType { get; }
        public int Id { get; }

        public Symbol Symbol { get; }
        public double StopLost { get; set; }
        public double? TakeProfit { get; set; }
        public DateTime CreatedAt { get;}

        public List<Rule> Rules { get; } = new List<Rule>();

        public OperationMode OperationMode { get; }

        public OperationBase(int id, OperationType type, double stopLost, double? takeProfit, Symbol symbol, OperationMode mode)
        {
            Id = id;
            CreatedAt = DateTime.Now;
            StopLost = stopLost;
            TakeProfit = takeProfit;
            Symbol = symbol;
            OperationMode = mode;
            OperationType = type;
        }

        public virtual OperationResult EvaluateRules(RuleFrequency frequency = RuleFrequency.OnTick)
        {
            List<Rule> rules = Rules
            .Where(x=>x.RuleFrequency == frequency)
            .Where(x => x.Iterations == RuleExecutionIteration.Many || (x.Iterations == RuleExecutionIteration.Once && x.ExecutedOnce == false))
            .ToList();
            foreach(Rule rule in Rules)
            {
                if (rule.IsValid()){
                    try
                    {
                        RuleExecutionResult ruleResult = RuleExecutionResult.None;
                        rule.PreExecute();
                        ruleResult = rule.Execute();
                        rule.PostExecute();
                        rule.ExecutedOnce = true;

                        if (ruleResult == RuleExecutionResult.ClosedOperation){
                            //stop all other rules
                            return OperationResult.Delete;
                        }
                    }
                    catch(Exception ex){
                        //TODO:Print exception
                    }
                }

            }


            return OperationResult.None;
        }
    }
}