using System.Globalization;

namespace CodingTracker
{
    internal class Validation
    {
        internal static DateTime GetDateInput(string message)
        {
            Console.WriteLine($"\n{message}");

            string dateInput = Console.ReadLine();

            if (dateInput == "0")
            {
                Console.Clear();
                DatabaseManager.GetUserInput();
            }

            if (!DateTime.TryParseExact(dateInput, "dd-MM-yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out DateTime date))
            {
                Console.Clear();
                Console.WriteLine("Incorrect format!");
                DatabaseManager.GetUserInput();
            }

            if (date.CompareTo(DateTime.Now) > 0)
            {
                Console.Clear();
                Console.WriteLine("You can't input a future date!");
                DatabaseManager.GetUserInput();
            }

            return date;
        }

        internal static DateTime GetTimeInput(string message)
        {
            Console.WriteLine($"\n{message}");

            string numberInput = Console.ReadLine();

            if (numberInput == "0")
            {
                Console.Clear();
                DatabaseManager.GetUserInput();
            }

            if (!DateTime.TryParseExact(numberInput, "HH:mm", new CultureInfo("en-US"), DateTimeStyles.None, out DateTime hour))
            {
                Console.Clear();
                Console.WriteLine("Incorrect format!");
                DatabaseManager.GetUserInput();
            }

            return hour;
        }

        internal static void CheckDayAndTime(DateTime date, DateTime hour)
        {
            if (hour.Hour > DateTime.Now.Hour && date.ToString("dd-MM-yyyy") == DateTime.Now.ToString("dd-MM-yyyy"))
            {
                Console.Clear();
                Console.WriteLine("You can't input a future hour!");
                DatabaseManager.GetUserInput();
            }

            if (hour.Minute > DateTime.Now.Minute && date.ToString("dd-MM-yyyy") == DateTime.Now.ToString("dd-MM-yyyy"))
            {
                Console.Clear();
                Console.WriteLine("You can't input a future minute!");
                DatabaseManager.GetUserInput();
            }
        }
    }
}
