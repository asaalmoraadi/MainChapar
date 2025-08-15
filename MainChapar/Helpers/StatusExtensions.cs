namespace MainChapar.Helpers
{
    public static class StatusExtensions
    {
        public static string ToFarsiStatus(this string status)
        {
            return status switch
            {
                "Submitted" => "ثبت‌شده",
                "Approved" => "تأیید‌شده",
                "Rejected" => "رد‌شده",
                "Processing" => "در حال پردازش",
                "Draft" => "پیش‌فرض",
                "Completed" => "انجام‌شده",
                _ => "نامشخص"
            };
        }
    }
}
