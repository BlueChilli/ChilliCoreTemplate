using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Admin
{
    public class StatisticsModel
    {
        public List<StatisticModel> StatisticsRow1 { get; set; }

        public List<StatisticModel> StatisticsRow2 { get; set; }
    }

    public class StatisticModel
    {
        public string Title { get; set; }

        public string PeriodLabel { get; set; }

        public string LabelType { get; set; }

        public int Total { get; set; }

        public int PercentageChange { get; set; }

        public List<int> Data { get; set; }

        public List<string> Labels { get; set; }
    }
}
