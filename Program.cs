using System.Diagnostics;
using System.Reflection;
using App;

List<Event> eventList = new();
List<Participant> participantList = new();
List<User> users = new();
User? activeUser = null;
Menu currentMenu = Menu.Default;


users.Add(new User("1", "", "a"));
users.Add(new User("2", "", "b"));
Event myEvent = new("event", Event.EventType.Request);
myEvent.Participants.Add(new Participant(users[0], Role.Patient));
eventList.Add(myEvent);


string usersFile = @"csv-files\users-list.csv";
if (!File.Exists(usersFile))
{
  Directory.CreateDirectory("csv-files");
  File.WriteAllText(usersFile, "");
}
string[] usersCsv = File.ReadAllLines(usersFile);
foreach (string userLine in usersCsv)
{
  string[] userSplitData = userLine.Split(",");
  users.Add(new(userSplitData[0], userSplitData[1], userSplitData[2]));
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

            Console.WriteLine($"enddate {newEvent.EndDate}");
            Console.WriteLine($"startdate {newEvent.StartDate}");
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