using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Admin
{
    public class ChartDataVM
    {
        public string Name { get; set; }
        public int AvgLastTenFiles { get; set; }
        public List<ChartVM> LastSixMonths { get; set; }
        public List<ChartVM> LastSixWeeks { get; set; }
    }


    public class ChartVM
    {
        public string ChartKey { get; set; }
        public int? CountTotal { get; set; }
    }
}
