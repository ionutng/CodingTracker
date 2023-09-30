using ConsoleTableExt;
using Microsoft.Data.Sqlite;
using System.Configuration;
using System.Data;
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
                Console.WriteLine("Type 3 to Update Record.");
                Console.WriteLine("Type 4 to Delete Record.");
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
                        Update();
                        break;
                    case "4":
                        Delete();
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
                command.CommandText = $"SELECT Date, StartTime, EndTime, Duration FROM coding ORDER BY Date ASC, StartTime ASC";

                var tableData = new DataTable();
                tableData.Load(command.ExecuteReader());

                connection.Close();

                if (tableData.Rows.Count > 0)
                {
                    ConsoleTableBuilder
                        .From(tableData)
                        .WithTitle("RECORDS", ConsoleColor.Cyan, ConsoleColor.DarkGray)
                        .WithColumn("Date", "Start Time", "End Time", "Duration")
                        .WithTextAlignment(new Dictionary<int, TextAligntment>
                        {
                            {0, TextAligntment.Center },
                            {1, TextAligntment.Center },
                            {2, TextAligntment.Center },
                            {3, TextAligntment.Center }
                        })
                        .WithCharMapDefinition(new Dictionary<CharMapPositions, char> {
                            {CharMapPositions.BottomLeft, '=' },
                            {CharMapPositions.BottomCenter, '=' },
                            {CharMapPositions.BottomRight, '=' },
                            {CharMapPositions.BorderTop, '=' },
                            {CharMapPositions.BorderBottom, '=' },
                            {CharMapPositions.BorderLeft, '|' },
                            {CharMapPositions.BorderRight, '|' },
                            {CharMapPositions.DividerY, '|' },
                        })
                        .WithHeaderCharMapDefinition(new Dictionary<HeaderCharMapPositions, char> {
                            {HeaderCharMapPositions.TopLeft, '=' },
                            {HeaderCharMapPositions.TopCenter, '=' },
                            {HeaderCharMapPositions.TopRight, '=' },
                            {HeaderCharMapPositions.BottomLeft, '|' },
                            {HeaderCharMapPositions.BottomCenter, '-' },
                            {HeaderCharMapPositions.BottomRight, '|' },
                            {HeaderCharMapPositions.Divider, '|' },
                            {HeaderCharMapPositions.BorderTop, '=' },
                            {HeaderCharMapPositions.BorderBottom, '-' },
                            {HeaderCharMapPositions.BorderLeft, '|' },
                            {HeaderCharMapPositions.BorderRight, '|' },
                        })
                        .ExportAndWriteLine();
                }
                else
                    Console.WriteLine("\nThere are no records yet!");
            }
        }

        static void Insert()
        {
            DateTime date = Validation.GetDateInput("Please insert the date: (Format: dd-MM-yyyy) or Type 0 to return to the main menu.");


            DateTime startTime = Validation.GetTimeInput("Please insert the time when you started coding: (Format: HH:mm) or Type 0 to return to the main menu.");

            if (CheckDuplicate(date, startTime))
            {
                Console.Clear();
                Console.WriteLine($"A record with the date {date:dd-MM-yyyy}, studying at {startTime:HH:mm} already exists!");
                GetUserInput();
            }

            Validation.CheckDayAndTime(date, startTime);

            DateTime endTime = Validation.GetTimeInput("Please insert the time when you finished coding: (Format: HH:mm) or Type 0 to return to the main menu.");

            if (CheckDuplicate(date, endTime))
            {
                Console.Clear();
                Console.WriteLine($"A record with the date {date:dd-MM-yyyy}, studying at {endTime:HH:mm} already exists!");
                GetUserInput();
            }

            Validation.CheckDayAndTime(date, endTime);

            TimeSpan duration = Validation.GetDuration(startTime, endTime);

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

        static void Update()
        {
            if (GetNumberOfRecords() == 0)
            {
                Console.Clear();
                Console.WriteLine("\nThere are no records yet!");
                GetUserInput();
            }

            Console.WriteLine("\nWhat would you like to update?");
            Console.WriteLine("Type 1 for starting time");
            Console.WriteLine("Type 2 for ending time");
            Console.WriteLine("Type 3 for both");
            Console.WriteLine("\nType 0 if you wish to return to the main menu.");

            string updateChoice = Console.ReadLine();

            if (updateChoice == "1")
            {
                GetRecords();
                DateTime date = Validation.GetDateInput("\nType the date for the record that you would like to update. Type it in the Format: (dd-MM-yyyy)");

                if (!DateExists(date))
                {
                    Console.Clear();
                    Console.WriteLine($"\nThe record with Date: {date:dd-MM-yyyy} doesn't exist.");
                    GetUserInput();
                }

                DateTime startTime = Validation.GetTimeInput("Please insert the new starting time: (Format: HH:mm) or Type 0 to return to the main menu.");

                Validation.CheckDayAndTime(date, startTime);

                TimeSpan duration = Validation.GetDuration(startTime, GetTime("EndTime", date));

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"UPDATE coding SET StartTime = \"{startTime:HH:mm}\", Duration = \"{duration}\" where Date = \"{date:dd-MM-yyyy}\"";

                    int rowCount = command.ExecuteNonQuery();

                    if (rowCount == 0)
                    {
                        Console.Clear();
                        Console.WriteLine($"\nA record with the date {date:dd-MM-yyyy} doesn't exist.");
                        GetUserInput();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("\nThe record has been successfully updated!");
                    }

                    connection.Close();
                }
            }
            else if (updateChoice == "2")
            {
                GetRecords();
                DateTime date = Validation.GetDateInput("\nType the date for the record that you would like to update. Type it in the Format: (dd-MM-yyyy)");

                if (!DateExists(date))
                {
                    Console.Clear();
                    Console.WriteLine($"\nThe record with Date: {date:dd-MM-yyyy} doesn't exist.");
                    GetUserInput();
                }

                DateTime endTime = Validation.GetTimeInput("Please insert the new ending time: (Format: HH:mm) or Type 0 to return to the main menu.");

                Validation.CheckDayAndTime(date, endTime);

                TimeSpan duration = Validation.GetDuration(GetTime("StartTime", date), endTime);

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"UPDATE coding SET EndTime = \"{endTime:HH:mm}\", Duration = \"{duration}\" where Date = \"{date:dd-MM-yyyy}\"";

                    int rowCount = command.ExecuteNonQuery();

                    if (rowCount == 0)
                    {
                        Console.Clear();
                        Console.WriteLine($"\nA record with the date {date:dd-MM-yyyy} doesn't exist.");
                        GetUserInput();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("\nThe record has been successfully updated!");
                    }

                    connection.Close();
                }
            }
            else if (updateChoice == "3")
            {
                GetRecords();
                DateTime date = Validation.GetDateInput("\nType the date for the record that you would like to update. Type it in the Format: (dd-MM-yyyy)");
                
                if (!DateExists(date))
                {
                    Console.Clear();
                    Console.WriteLine($"\nThe record with Date: {date:dd-MM-yyyy} doesn't exist.");
                    GetUserInput();
                }

                DateTime startTime = Validation.GetTimeInput("Please insert the new starting time: (Format: HH:mm) or Type 0 to return to the main menu.");

                Validation.CheckDayAndTime(date, startTime);

                DateTime endTime = Validation.GetTimeInput("Please insert the new ending time: (Format: HH:mm) or Type 0 to return to the main menu.");

                Validation.CheckDayAndTime(date, startTime);

                TimeSpan duration = Validation.GetDuration(startTime, endTime);

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"UPDATE coding SET StartTime = \"{startTime:HH:mm}\", EndTime = \"{endTime:HH:mm}\", Duration = \"{duration}\" where Date = \"{date:dd-MM-yyyy}\"";

                    int rowCount = command.ExecuteNonQuery();

                    if (rowCount == 0)
                    {
                        Console.Clear();
                        Console.WriteLine($"\nA record with the date {date:dd-MM-yyyy} doesn't exist.");
                        GetUserInput();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("\nThe record has been successfully updated!");
                    }

                    connection.Close();
                }
            }
            else
            {
                Console.Clear();
                GetUserInput();
            }
        }

        static void Delete()
        {
            if (GetNumberOfRecords() == 0)
            {
                Console.Clear();
                Console.WriteLine("\nThere are no records yet!");
                GetUserInput();
            }

            Console.WriteLine("\nType 1 if you wish to delete only one record.");
            Console.WriteLine("Type 2 if you wish to delete all of the records.");
            Console.WriteLine("\nType 0 if you wish to return to the main menu.");

            string deleteOption = Console.ReadLine();


            if (deleteOption == "1")
            {
                GetRecords();
                DateTime date = Validation.GetDateInput("\nWhich day would you like to delete? Type using the Format: (dd-MM-yyyy)");

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"DELETE FROM coding WHERE Date = \"{date:dd-MM-yyyy}\"";

                    int rowCount = command.ExecuteNonQuery();

                    if (rowCount == 0)
                    {
                        Console.Clear();
                        Console.WriteLine($"\nThe record with Date: {date:dd-MM-yyyy} doesn't exist.");
                        GetUserInput();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("\nThe record has been successfully deleted!");
                    }

                    connection.Close();
                }
            }
            else if (deleteOption == "2")
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"DELETE FROM coding";

                    int rowCount = command.ExecuteNonQuery();

                    if (rowCount == 0)
                    {
                        Console.Clear();
                        Console.WriteLine("\nThere are no records yet");
                        GetUserInput();
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("\nAll of the records have been successfully deleted!");
                    }

                    connection.Close();
                }
            }
            else
            {
                Console.Clear();
                GetUserInput();
            }
        }

        static int GetNumberOfRecords()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Date, StartTime, EndTime, Duration FROM coding";

                List<Coding> tableData = new();

                SqliteDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(new Coding
                        {
                            Date = DateTime.ParseExact(reader.GetString(0), "dd-MM-yyyy", new CultureInfo("en-US")),
                            StartTime = DateTime.ParseExact(reader.GetString(1), "HH:mm", new CultureInfo("en-US")),
                            EndTime = DateTime.ParseExact(reader.GetString(2), "HH:mm", new CultureInfo("en-US")),
                            Duration = TimeSpan.Parse(reader.GetString(3))
                        });
                    }
                }

                connection.Close();

                return tableData.Count;
            }
        }

        static bool DateExists(DateTime date)
        {
            bool dateFound = false;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Date, StartTime, EndTime, Duration FROM coding";

                List<Coding> tableData = new();

                SqliteDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(new Coding
                        {
                            Date = DateTime.ParseExact(reader.GetString(0), "dd-MM-yyyy", new CultureInfo("en-US")),
                            StartTime = DateTime.ParseExact(reader.GetString(1), "HH:mm", new CultureInfo("en-US")),
                            EndTime = DateTime.ParseExact(reader.GetString(2), "HH:mm", new CultureInfo("en-US")),
                            Duration = TimeSpan.Parse(reader.GetString(3))
                        });
                    }
                }

                connection.Close();

                foreach (var data in tableData)
                    if (data.Date.ToString("dd-MM-yyyy") == date.ToString("dd-MM-yyyy"))
                        dateFound = true;

                return dateFound;
            }
        }

        static bool CheckDuplicate(DateTime date, DateTime time)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Date, StartTime, EndTime, Duration FROM coding WHERE Date = \"{date:dd-MM-yyyy}\"";

                List<Coding> tableData = new();

                SqliteDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(new Coding
                        {
                            Date = DateTime.ParseExact(reader.GetString(0), "dd-MM-yyyy", new CultureInfo("en-US")),
                            StartTime = DateTime.ParseExact(reader.GetString(1), "HH:mm", new CultureInfo("en-US")),
                            EndTime = DateTime.ParseExact(reader.GetString(2), "HH:mm", new CultureInfo("en-US")),
                            Duration = TimeSpan.Parse(reader.GetString(3))
                        });
                    }
                }

                connection.Close();

                foreach (var data in tableData)
                {
                    if (data.StartTime.Hour == data.EndTime.Hour)
                    {
                        if (time.Hour == data.StartTime.Hour && time.Minute >= data.StartTime.Minute && time.Minute <= data.EndTime.Minute)
                            return true;
                    }
                    else
                    {
                        if (time.Hour > data.StartTime.Hour && time.Hour < data.EndTime.Hour)
                            return true;
                        if (time.Hour == data.StartTime.Hour && time.Minute >= data.StartTime.Minute)
                            return true;
                        if (time.Hour == data.EndTime.Hour && time.Minute <= data.EndTime.Minute)
                            return true;
                    }
                }

                return false;
            }
        }

        static DateTime GetTime(string firstTime, DateTime date)
        {
            DateTime time = DateTime.MinValue;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT {firstTime} FROM coding WHERE Date = \"{date:dd-MM-yyyy}\"";


                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                    time = DateTime.ParseExact(reader.GetString(0), "HH:mm", new CultureInfo("en-US"));

                connection.Close();

                return time;
            }
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
