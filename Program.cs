using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using App;


HCSystem sys = new();

User? activeUser = null;
Menu currentMenu = Menu.Default;
Console.WriteLine("hello");

if (sys.users.Count <= 0)
{
  sys.users.Add(new User("admin123", "admin", "Admin User"));
  sys.users.Add(new User("testssn1", "test1", "Test Patient"));
  sys.users.Add(new User("testssn2", "test2", "Test Personnel"));
  // Hard coding all the permission to admins permission list.
  foreach (User user in sys.users)
  {
    if (user.SSN == "admin123")
    {
      int permIndex = 1;
      {
        foreach (Permission perm in Enum.GetValues(typeof(Permission)))
        {
          if (perm != Permission.None)
          {
            user.Permissions.Add(perm);
            permIndex++;
          }
        }
      }
    }
    break;
  }
}

sys.SaveUsersToFile();

if (sys.locations.Count <= 0)
{
  sys.locations.Add(new("testVC", "Main Street 1"));
}

sys.SaveLocationsToFile();

if (sys.eventList.Count <= 0)
{
  Event myEvent = new("event", Event.EventType.Request);
  myEvent.Participants.Add(new Participant(sys.users[0], Role.Patient));
  sys.eventList.Add(myEvent);

  Event mySecondEvent = new("My Appointment", Event.EventType.Appointment);
  mySecondEvent.Description = "I have a cold.";
  mySecondEvent.StartDate = new DateTime(2025, 10, 20, 11, 0, 0);
  mySecondEvent.EndDate = new DateTime(2025, 10, 20, 11, 30, 0);
  mySecondEvent.Location = sys.locations[0];
  mySecondEvent.Participants.Add(new(sys.users[0], Role.Patient));
  mySecondEvent.Participants.Add(new(sys.users[1], Role.Personnel));
  mySecondEvent.Participants.Add(new(sys.users[2], Role.Admin));
  sys.eventList.Add(mySecondEvent);
}

sys.SaveEventsToFile();


// TEST CODE >>>>
/* 
foreach (User user in sys.users)
{
  Console.WriteLine($"\n{user.SSN} - {user.Name}");
  for (int i = 0; i < user.Permissions.Count; i++)
  {
    Console.WriteLine($"\n{user.Permissions[i]}");
  }
  Console.WriteLine("\n------------------");
}
Console.ReadLine();


foreach (Location loc in sys.locations)
{
  Console.WriteLine($"{loc.Name} {loc.Address} {loc.Region}");
}
Console.ReadLine();


foreach (Event events in sys.eventList)
{
  Console.WriteLine($"\n{events.Title} - {events.MyEventType} - {events.Description}\n"
  + $"{events.StartDate} - {events.EndDate}");
  if (events.Location != null)
  { Console.WriteLine($"\nLocation: {events.Location.Name} - Adress: {events.Location.Address} - Region: {events.Location.Region}"); }
  foreach (Participant participant in events.Participants)
  {
    Console.WriteLine($"{participant.User.Name} - {participant.User.SSN} - {participant.ParticipantRole}");
  }
  Console.WriteLine("\n----------------");
}
Console.Write("\nPress ENTER to continue.");
Console.ReadLine(); */
// <<<< END OF TEST CODE


bool isRunning = true;
while (isRunning)
{
  switch (currentMenu)
  {
    case Menu.Default:
      try { Console.Clear(); } catch { }
      Console.WriteLine("\n[1] Login \n[2] Request registration as a patient\n[3] Quit");
      Console.Write("\n► ");
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

          foreach (User user in sys.users)
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

          bool foundSSN = false;

          Console.Write("\nPlease input your SSN: ");
          string? newSSN = Console.ReadLine();
          if (string.IsNullOrWhiteSpace(newSSN))
          {
            Console.WriteLine("\nInvalid input");
            Console.ReadLine();
            break;
          }

          foreach (Event events in sys.eventList)
          {
            if (events.Title == newSSN)
            {
              Console.WriteLine("\nThere is already a patient request with the given SSN.");
              Console.Write("\nPress ENTER to go back to previous menu. ");
              Console.ReadLine();
              foundSSN = true;
              break;
            }
          }

          if (!foundSSN)
          {
            Console.Write("\nPlease input an email: ");
            string? newEmail = Console.ReadLine();
            Console.Write("\nWhat is your name? ");
            string? newName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newName))
            {
              Console.WriteLine("\nInvalid input");
              Console.ReadLine();
              break;
            }
            Debug.Assert(newSSN != null);
            Debug.Assert(newEmail != null);
            Debug.Assert(newName != null);

            string newDescription = $"{newSSN} request to be a patient. Name: {newName} - Email: {newEmail}";
            Event? newEvent = new(newSSN, Event.EventType.Request);
            newEvent.Description = newDescription;

            sys.eventList.Add(newEvent);
            sys.SaveEventsToFile();

            Console.WriteLine($"\nYour request have been registered. We'll let you know at {newEmail} when we have made a decision.");
            Console.Write("\nPress ENTER to continue. ");
            Console.ReadLine();
          }
          break;

        case "3":
          isRunning = false;
          break;

        case "CheatingNeverPays":
          sys.CheatersDelight();
          break;

        default:
          Console.WriteLine("\nPlease enter a valid input");
          Console.ReadLine();
          break;
      }
      break;

    case Menu.Main:
      try { Console.Clear(); } catch { }
      Console.WriteLine($"\nWelcome, {activeUser?.Name}");
      Console.WriteLine("\n[1] Create Personnel Account");
      Console.WriteLine("[2] View All Users");
      Console.WriteLine("[3] View Events by Type");
      Console.WriteLine("[m] Manage Permissions \n[v] View Permissions\n\n[x] Logout");
      Console.Write("\n► ");

      switch (Console.ReadLine())
      {
        case "1":
          sys.CreateAccount();
          break;

        case "2":
          // View All Users
          Console.WriteLine("\n=== ALL USERS ===");
          foreach (User user in sys.users)
          {
            Console.WriteLine($"Name: {user.Name} | SSN: {user.SSN}");
          }
          Console.Write("\nPress ENTER to continue.");
          Console.ReadLine();
          break;

        case "3":
          sys.ViewEvents();
          break;

        case "m": // Manage permessions
          sys.ManagePermissions(activeUser);
          break;

        case "v": // View permissions
          sys.ViewPermissions(activeUser, currentMenu);
          break;

        case "x":
          activeUser = null;
          currentMenu = Menu.Default;
          break;

        default:
          Console.Write("\nInvalid input. Press ENTER to continue. ");
          Console.ReadLine();
          break;
      }
      break;
  }
}
