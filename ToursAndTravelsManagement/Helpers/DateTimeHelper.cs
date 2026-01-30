namespace ToursAndTravelsManagement.Helpers
{
    public class DateTimeHelper
    {
        public static string FormatDate(DateTime date)
        {
            if (date.Year <= 1) // Check if it's default DateTime
                return "Date not available";

            return date.ToString("dd MMM yyyy");
        }

        public static bool IsValidDate(DateTime date)
        {
            return date.Year > 1;
        }
    }
}
