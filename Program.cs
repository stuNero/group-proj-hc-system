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
  string[] eventSplitData = eventLine.Split("~");
  string newEventTitle = eventSplitData[0];
  string newEventDescription = eventSplitData[1];
  DateTime newEventStartDate = DateTime.Parse(eventSplitData[2]);
  DateTime newEventEndDate = DateTime.Parse(eventSplitData[3]);
  Event.EventType eventType = Event.EventType.Request;

  switch (eventSplitData[4])
  {
    case "Request": eventType = Event.EventType.Request; break;
    case "Entrie": eventType = Event.EventType.Entry; break;
    case "Appoitment": eventType = Event.EventType.Appointment; break;
  }

  List<Participant> participantsList = new();

  string[] participants = eventSplitData[5].Split("^");
  for (int i = 0; i < participants.Length; i++)
  {
    string[] participantSplitData = participants[i].Split("¤");

    User? partUser = null;
    Role partRole = Role.Patient;
    foreach (User user in users)
    {
      if (participantSplitData[0] == user.SSN)
      {
        partUser = user;
        break;
      }
    }
    switch (participantSplitData[1])
    {
      case "Admin": partRole = Role.Admin; break;
      case "Patient": partRole = Role.Patient; break;
      case "Personnel": partRole = Role.Personnel; break;
    }
    if (partUser != null)
    {
      participantsList.Add(new(partUser, partRole));
    }
    else
    {
      break;
    }
  }
  Event? newEvent = new(newEventTitle);
  newEvent.Description = newEventDescription;
  newEvent.StartDate = newEventStartDate;
  newEvent.EndDate = newEventEndDate;
  newEvent.MyEventType = eventType;
  newEvent.Participant = participantsList;

  eventList.Add(newEvent);
}

// TEST CODE
/* Console.WriteLine($"{eventList[0].Title} - {eventList[0].Description} - {eventList[0].StartDate} - {eventList[0].EndDate} - {eventList[0].MyEventType}\n"
+ $"{eventList[0].Participant[0].User.SSN} - {eventList[0].Participant[0].UserRoles}\n"
+ $"{eventList[0].Participant[1].User.SSN} - {eventList[0].Participant[1].UserRoles}\n"
+ $"{eventList[0].Participant[2].User.SSN} - {eventList[0].Participant[2].UserRoles}");
Console.ReadLine(); */

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