using CodingTracker;
using System.Configuration;

// Storing the database path in a string and checking to see if the database exists
string dbPath = ConfigurationManager.AppSettings.Get("DatabasePath");
bool dbExists = File.Exists(dbPath);

// Create the database if it doesn't exist
if (!dbExists)
{
    DatabaseManager.CreateDatabase();
    DatabaseManager.GetUserInput();
}
else
{
    DatabaseManager.GetUserInput();
}
