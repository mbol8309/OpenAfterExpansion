using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using MBR;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class OpenAfterExpansion : Robot
    {
        [Parameter(DefaultValue = 0)]
        public int Hour { get; set; }
        [Parameter(DefaultValue = 0)]
        public int Minute { get; set; }
        [Parameter(DefaultValue = 20)]
        public int StopLossPips { get; set; }
        [Parameter(DefaultValue = 60)]
        public int TakeProfitPips { get; set; }

        [Parameter(DefaultValue = 5)]
        public int MinimunPipsOpen { get; set; }

        [Parameter(DefaultValue = 2)]
        private double Risk { get; set; }

        private DateTime? LastDayCheck { get; set; }
        private double? LastDayOpenPriceAsk { get; set; }
        private double? LastDayOpenPriceBid { get; set; }

        private Ticks Ticks { get; set; }

        private Bars Bars { get; set; }
        private Tick LastTick
        {
            get { return this.Ticks.Last(); }
        }

        TradeController instance;

        protected override void OnStart()
        {
            System.Diagnostics.Debugger.Launch();
            LastDayCheck = null;
            LastDayOpenPriceAsk = null;
            instance = TradeController.Instance().Register(this);
            this.Ticks = MarketData.GetTicks();

        }

        protected override void OnTick()
        {
            DateTime time = this.LastTick.Time;


            if (this.LastDayCheck != null)
            {
                TimeSpan difference = time.Subtract((DateTime)this.LastDayCheck);
                if (difference.TotalDays > 1)
                {
                    Tick tick = Ticks.Where(x => x.Time >= this.LastDayCheck).OrderBy(x => x.Time).First();

                    double openPriceAsk = tick.Ask;
                    double openPriceBid = tick.Bid;

                    this.LastDayCheck = time.Date;
                    LastDayOpenPriceAsk = openPriceAsk;
                    LastDayOpenPriceBid = openPriceBid;

                }
            }
            else
            {
                Tick tick = Ticks.Where(x => x.Time >= this.LastDayCheck).OrderBy(x => x.Time).First();

                double openPriceAsk = tick.Ask;
                double openPriceBid = tick.Bid;
                LastDayOpenPriceAsk = openPriceAsk;
                LastDayOpenPriceBid = openPriceBid;

                this.LastDayCheck = time.Date;
            }

            // first tick of day

            double accountBalance = Account.Balance;
            double lotSize = (accountBalance * this.Risk / 100) / (this.StopLossPips * Symbol.PipValue);
            if ((this.LastTick.Ask - LastDayOpenPriceAsk) / Symbol.TickSize > this.MinimunPipsOpen)
            {

                instance.AddPosition(MBR.Operation.OperationType.Buy, this.StopLossPips, this.TakeProfitPips, lotSize);
            }

            if ((LastDayOpenPriceBid - this.LastTick.Bid) / Symbol.TickSize > this.MinimunPipsOpen)
            {
                instance.AddPosition(MBR.Operation.OperationType.Sell, this.StopLossPips, this.TakeProfitPips, lotSize);
            }
        }

        protected override void OnStop()
        {

            // Put your deinitialization logic here
        }
    }
}
