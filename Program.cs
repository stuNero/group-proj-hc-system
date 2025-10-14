using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using App;

List<Event> eventList = new();
List<User> users = new();
User? activeUser = null;
Menu currentMenu = Menu.Default;

if (!Directory.Exists("csv-files"))
{
  Directory.CreateDirectory("csv-files");
}

string usersFile = @"csv-files\users-list.csv";
if (!File.Exists(usersFile))
{
  File.WriteAllText(usersFile, "");
}
string[] usersCsv = File.ReadAllLines(usersFile);
foreach (string userLine in usersCsv)
{
  string[] userSplitData = userLine.Split(",");
  users.Add(new(userSplitData[0], userSplitData[1], userSplitData[2]));
}

string eventFile = @"csv-files\events-list.csv";
if (!File.Exists(eventFile))
{
  File.WriteAllText(eventFile, "");
}
string[] eventsCsv = File.ReadAllLines(eventFile);
foreach (string eventLine in eventsCsv)
{
  Event newEvent = null;
  string[] eventSplitData = eventLine.Split("~");
  newEvent.Title = eventSplitData[0];
  newEvent.Description = eventSplitData[1];
  newEvent.StartDate = DateTime.Parse(eventSplitData[2]);
  newEvent.EndDate = DateTime.Parse(eventSplitData[3]);
  // newEvent.Participant = eventSplitData[4]; 
}


bool isRunning = true;
while (isRunning)
{
  switch (currentMenu)
  {
    case Menu.Default:
      try { Console.Clear(); } catch { }
      Console.WriteLine("\n[1] Login \n[2] Register Account\n[3] Quit\n");
      Console.Write("> ");
      string? input = Console.ReadLine();

      switch (input)
      {
        case "1":
          Console.Write("Please input your SSN: ");
          string? ssn = Console.ReadLine();
          Console.Write("Please input a password: ");
          string? password = Console.ReadLine();

          Debug.Assert(ssn != null);
          Debug.Assert(password != null);

          foreach (User user in users)
          {
            if (user.TryLogin(ssn, password))
            {
              activeUser = user;
              currentMenu = Menu.Main;
              break;
            }
          }
          break;

        case "2":
          Console.Write("Please input your SSN: ");
          string? newSSN = Console.ReadLine();
          if (string.IsNullOrWhiteSpace(newSSN))
          {
            Console.WriteLine("Invalid input");
            Console.ReadLine();
            break;
          }

          Console.Write("Please input a password: ");
          string? newPassword = Console.ReadLine();
          Console.Write("What is your name? ");
          string? newName = Console.ReadLine();
          if (string.IsNullOrWhiteSpace(newName))
          {
            Console.WriteLine("Invalid input");
            Console.ReadLine();
            break;
          }
          Debug.Assert(newSSN != null);
          Debug.Assert(newPassword != null);
          Debug.Assert(newName != null);
          users.Add(new User(newSSN, newPassword, newName));

          string newUserLine = $"{newSSN},{newPassword},{newName}";
          File.AppendAllText(usersFile, newUserLine + Environment.NewLine);

          break;
        case "3":
          isRunning = false;
          break;
        default:
          Console.WriteLine("Please enter a valid input");
          Console.ReadLine();
          break;
      }
      break;
    case Menu.Main:
      try { Console.Clear(); } catch { }
      Console.WriteLine("[1] Logout");

      switch (Console.ReadLine())
      {
        case "1":
          activeUser = null;
          currentMenu = Menu.Default;
          break;
        default:
          Console.WriteLine("Something went wrong");
          Console.ReadLine();
          break;
      }

      break;
  }

}