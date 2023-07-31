using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MBR.Operation
{
    public class QueueOperation
    {
        OperationType OperationType { set; get; }
        double StopLost { set; get; }
        double TakeProfit { set; get; }
        double? EntryPoint { set; get; }
        List<Rule> Rules { get; set; }
    }
}