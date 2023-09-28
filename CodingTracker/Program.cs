using System.Configuration;
using System.Collections.Specialized;

// Read all the keys from the config file
NameValueCollection sAll;
sAll = ConfigurationManager.AppSettings;

foreach (string s in sAll.AllKeys)
    Console.WriteLine("Key: " + s + " Value: " + sAll.Get(s));
