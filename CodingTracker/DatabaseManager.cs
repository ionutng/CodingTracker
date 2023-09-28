using Microsoft.Data.Sqlite;
using System.Configuration;
using System.Globalization;

namespace CodingTracker
{
    internal class DatabaseManager
    {
        static readonly string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString");
        internal static void CreateDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        CREATE TABLE coding (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Date TEXT,
                            StartTime TEXT,
                            EndTime TEXT,
                            Duration TEXT)
                    ";

                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        internal static void GetUserInput()
        {
            while (true)
            {
                Console.WriteLine("\nWelcome to the Coding Tracker app!");
                Console.WriteLine("What would you like to do?\n");
                Console.WriteLine("Type 0 to Close Application.");
                Console.WriteLine("Type 1 to View All Records.");
                Console.WriteLine("Type 2 to Insert Record.");
                Console.WriteLine("Type 3 to Delete Record.");
                Console.WriteLine("Type 4 to Update Record.");
                Console.WriteLine("------------------------------");

                string userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "0":
                        Console.WriteLine("\nHave a good day!");
                        Environment.Exit(0);
                        break;
                    case "1":
                        GetRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        //Delete();
                        break;
                    case "4":
                        //Update();
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Wrong input! Please type a number between 0 and 4.");
                        break;
                }
            }
        }

        static void GetRecords()
        {
            Console.Clear();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Date, StartTime, EndTime, Duration FROM coding ORDER BY Date ASC";

                List<Coding> tableData = new List<Coding>();

                SqliteDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(new Coding
                        {
                            Date = DateTime.ParseExact(reader.GetString(0), "dd-MM-yyyy", new CultureInfo("en-US")),
                            StartTime = DateTime.ParseExact(reader.GetString(1),"HH:mm", new CultureInfo("en-US")),
                            EndTime = DateTime.ParseExact(reader.GetString(2), "HH:mm", new CultureInfo("en-US")),
                            Duration = TimeSpan.Parse(reader.GetString(3))
                        });
                    }
                }
                else
                {
                    Console.WriteLine("\nThere are no records yet!");
                }

                connection.Close();

                if (tableData.Count > 0)
                    Console.WriteLine("\nThe records are:");

                foreach (var data in tableData)
                {
                    Console.WriteLine($"Date: {data.Date:dd-MM-yyyy}");
                    Console.WriteLine($"Start Time: {data.StartTime:HH:mm} - End Time: {data.EndTime:HH:mm}");
                    Console.WriteLine($"Duration: {data.Duration.Hours} hours, {data.Duration.Minutes} minutes\n");
                }
            }
        }

        static void Insert()
        {
            DateTime date = Validation.GetDateInput("Please insert the date: (Format: dd-MM-yyyy) or Type 0 to return to the main menu.");

            DateTime startTime = Validation.GetTimeInput("Please insert the time when you started coding: (Format: HH:mm) or Type 0 to return to the main menu.");

            Validation.CheckDayAndTime(date, startTime);

            DateTime endTime = Validation.GetTimeInput("Please insert the time when you finished coding: (Format: HH:mm) or Type 0 to return to the main menu.");
            
            Validation.CheckDayAndTime(date, endTime);

            TimeSpan duration = endTime - startTime;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"INSERT INTO coding (Date, StartTime, EndTime, Duration) VALUES (\"{date:dd-MM-yyyy}\", \"{startTime:HH:mm}\", \"{endTime:HH:mm}\", \"{duration}\")";

                command.ExecuteNonQuery();

                connection.Close();
            }

            Console.Clear();
            Console.WriteLine("The record has been inserted!");
        }
    }
    internal class Coding
    {
        internal DateTime Date { get; set; }
        internal DateTime StartTime {  get; set; }

        internal DateTime EndTime { get; set; }

        internal TimeSpan Duration { get; set; }
    }
}
