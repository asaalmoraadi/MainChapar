namespace MainChapar.ViewModel.Admin
{
    public class SalesStatsViewModel
    {
        //امار روزانه
        public int TodayProductCount { get; set; }
        public decimal TodayProductSales { get; set; }
        public int TodayPrintCount { get; set; }
        public decimal TodayPrintSales { get; set; }

        // امار ماه
        public int MonthProductCount { get; set; }
        public decimal MonthProductSales { get; set; }
        public int MonthPrintCount { get; set; }
        public decimal MonthPrintSales { get; set; }

        // جدید برای نمودار یا جدول ماهانه
        public List<MonthlySalesItem> MonthlySales { get; set; } = new();

    }
    public class MonthlySalesItem
    {
        public int Month { get; set; } // از 1 تا 12
        public int ProductCount { get; set; }
        public decimal ProductSales { get; set; }
        public int PrintCount { get; set; }
        public decimal PrintSales { get; set; }
    }
}
