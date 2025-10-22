using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using App;

HCSystem sys = new();
User? activeUser = null;
Menu currentMenu = Menu.Default;

if (sys.users.Count <= 0)
{
  User admin1 = new User("admin123", "admin", "admin User");
  foreach (Permission perm in Enum.GetValues(typeof(Permission)))
  {
    Console.WriteLine(perm);
    admin1.Permissions.Add(perm);
  }
  foreach (Permission perm in admin1.Permissions)
  {
    Console.WriteLine(perm);
  }
  sys.users.Add(admin1);
  sys.users.Add(new User("testssn1", "test1", "Test Patient"));
  sys.users.Add(new User("testssn2", "test2", "Test Personnel"));
  // Hard coding all the permission to admins permission list.
  foreach (Permission perm in sys.allPermissionList)
  {
    if (perm != Permission.None)
    {
      sys.users[0].Permissions.Add(perm);
    }
  }

}
foreach (Permission perm in Enum.GetValues(typeof(Permission)))
{
  sys.allPermissionList.Add(perm);
}
if (sys.locations.Count <= 0)
{
  sys.locations.Add(new("testVC", "Main Street 1"));
}
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

  Event testEntry = new("My Entry", Event.EventType.Entry);
  testEntry.Description = "User has too long fingers";
  testEntry.StartDate = new DateTime(2025, 10, 20, 11, 0, 0);
  testEntry.EndDate = new DateTime(2025, 10, 20, 11, 30, 0);
  testEntry.Location = sys.locations[0];
  testEntry.Participants.Add(new(sys.users[0], Role.Patient));
  testEntry.Participants.Add(new(sys.users[1], Role.Personnel));
  testEntry.Participants.Add(new(sys.users[2], Role.Admin));
  sys.eventList.Add(testEntry);
}

sys.SaveLocationsToFile();
sys.SaveUsersToFile();
sys.SaveEventsToFile();

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
            Console.ReadKey(true);
            break;
          }

          foreach (Event events in sys.eventList)
          {
            if (events.Title == newSSN)
            {
              Console.WriteLine("\nThere is already a patient request with the given SSN.");
              Console.Write("\nPress ENTER to go back to previous menu. ");
              Console.ReadKey(true);
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
              Console.ReadKey(true);
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
            Console.ReadKey(true);
          }
          break;
        case "3":
          isRunning = false;
          break;
        default:
          Console.WriteLine("\nPlease enter a valid input");
          Console.ReadKey(true);
          break;
      }
      break;
    case Menu.Main:
      try { Console.Clear(); } catch { }
      Console.WriteLine($"\nWelcome, {activeUser?.Name}");
      Console.WriteLine("\n[1] Handle Accounts");
      Console.WriteLine("\n[2] Handle Registrations");
      Console.WriteLine("\n[3] Handle Appointment");
      Console.WriteLine("\n[4] Handle Journal Entries");
      Console.WriteLine("\n[5] Add a Location");
      Console.WriteLine("\n[6] Schedule of a Location");
      Console.WriteLine("\n[7] Assign User to a Region");
      Console.WriteLine("\n[8] View Permissions");
      Console.WriteLine("\n[g] View My Journal");
      Console.WriteLine("\n[h] View My Schedule");
      Console.WriteLine("\n[j] View All Users");
      Console.WriteLine("\n[k] View Events by Type");
      Console.WriteLine("\n[x] Logout");
      Console.Write("\n> ");

      switch (Console.ReadLine())
      {

        // HandleAccount
        case "1":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.HandleAccount))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.CreateAccount();
          break;

        // HandleRegistration
        case "2":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.HandleRegistration))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.ViewEvents(Event.EventType.Request);
          break;

        // HandleAppointment
        case "3":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.HandleAppointment))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.ViewEvents(Event.EventType.Appointment);
          break;

        // JournalEntries
        case "4":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.JournalEntries))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          Console.WriteLine("W I P");
          Console.ReadKey(true);
          break;

        // AddLocation
        case "5":
          if (!activeUser!.HasPermission(Permission.AddLocation))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          try { Console.Clear(); } catch { }

          sys.AddLocation();

          Console.ReadKey(true);
          break;

        // ScheduleOfLocation
        case "6":
          if (!activeUser!.HasPermission(Permission.ScheduleOfLocation))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          try { Console.Clear(); } catch { }
          
          sys.ScheduleOfLocation();

          Console.ReadKey(true);
          break;

        // AssignRegion
        case "7":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.AssignRegion))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          Console.WriteLine("W I P");
          Console.ReadKey(true);
          break;

        case "8": // Permissions
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.PermHandlePerm))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.PermissionSystem(activeUser);
          break;

        // View My Journal
        case "g":
          try { Console.Clear(); } catch { }
          Debug.Assert(activeUser != null);
          sys.ViewEvent(Event.EventType.Entry, activeUser);
          Console.ReadKey(true);
          break;

        // View My Schedule
        case "h":
          try { Console.Clear(); } catch { }
          Debug.Assert(activeUser != null);
          sys.ViewEvent(Event.EventType.Appointment, activeUser);
          Console.ReadKey(true);
          break;

        // View All Users
        case "j":
          try { Console.Clear(); } catch { }
          Console.WriteLine("\n=== ALL USERS ===");
          foreach (User user in sys.users)
          {
            Console.WriteLine($"Name: {user.Name} | SSN: {user.SSN}");
          }
          Console.Write("\nPress ENTER to continue.");
          Console.ReadKey(true);
          break;
        
        // Log out
        case "x":
          activeUser = null;
          currentMenu = Menu.Default;
          break;

        default:
          Console.Write("\nInvalid input. Press ENTER to continue. ");
          Console.ReadKey(true);
          break;
      }
      break;
  }
}
