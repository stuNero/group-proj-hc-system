using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using App;

List<Event> eventList = new();
List<Participant> participantList = new();
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
  Event.EventType eventType = Event.EventType.Request;
  string newEventDescription = eventSplitData[2];
  DateTime newEventStartDate = DateTime.Parse(eventSplitData[3]);
  DateTime newEventEndDate = DateTime.Parse(eventSplitData[4]);

  switch (eventSplitData[1])
  {
    case "Request": eventType = Event.EventType.Request; break;
    case "Enty": eventType = Event.EventType.Entry; break;
    case "Appointment": eventType = Event.EventType.Appointment; break;
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
  Event? newEvent = new(newEventTitle, eventType);
  newEvent.Description = newEventDescription;
  newEvent.StartDate = newEventStartDate;
  newEvent.EndDate = newEventEndDate;
  newEvent.Participants = participantsList;

  eventList.Add(newEvent);
}

bool isRunning = true;
while (isRunning)
{
  switch (currentMenu)
  {
    case Menu.Default:
      try { Console.Clear(); } catch { }
      Console.WriteLine("\n[1] Login \n[2] Register Account\n[3] Quit");
      Console.Write("\n> ");
      string? input = Console.ReadLine();

      switch (input)
      {
        case "1":
          Console.Write("\nPlease input your SSN: ");
          string? ssn = Console.ReadLine();
          Console.Write("\nPlease input a password: ");
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
          Console.Write("\nPlease input your SSN: ");
          string? newSSN = Console.ReadLine();
          if (string.IsNullOrWhiteSpace(newSSN))
          {
            Console.WriteLine("\nInvalid input");
            Console.ReadLine();
            break;
          }

          Console.Write("\nPlease input a password: ");
          string? newPassword = Console.ReadLine();
          Console.Write("\nWhat is your name? ");
          string? newName = Console.ReadLine();
          if (string.IsNullOrWhiteSpace(newName))
          {
            Console.WriteLine("\nInvalid input");
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
          Console.WriteLine("\nPlease enter a valid input");
          Console.ReadLine();
          break;
      }
      break;
    case Menu.Main:
      try { Console.Clear(); } catch { }
      Console.WriteLine("\n[1] Send patient registeration request \n[x] Logout");
      Console.Write("\n> ");

      switch (Console.ReadLine())
      {
        case "1":
          bool ssnFound = false;
          foreach (Event userEvent in eventList)
          {
            foreach (Participant part in userEvent.Participants)
            {
              if (part.User.SSN == activeUser.SSN)
              {
                Console.WriteLine($"\nRequest already exsists, you can just wait for now...\n");
                Console.ReadLine();
                ssnFound = true;
                break;
              }
            }
            break;
          }
          if (ssnFound == false)
          {
            Participant newParticipant = new(activeUser, Role.Patient);
            Event newEvent = new($"New Event", Event.EventType.Request);
            newEvent.StartDate = DateTime.Now;
            newEvent.Description = $"\n{activeUser.Name} is requesting to become a patient.";
            newEvent.Participants.Add(newParticipant);
            eventList.Add(newEvent);
            Console.WriteLine("\nYour request is sent!\n");
            Console.ReadLine();
            break;
          }
          break;


        case "x":
          activeUser = null;
          currentMenu = Menu.Default;
          break;
      }

      break;
  }

}