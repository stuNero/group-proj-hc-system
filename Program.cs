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
  foreach (User user in sys.users)
  {
    if (user.SSN == "admin123")
    {
      int permIndex = 1;
      {
        foreach (Permission perm in Enum.GetValues(typeof(Permission)))
        {
          user.Permissions.Add(perm);
          permIndex++;
        }
      }
    }
    break;
  }
}
if (sys.locations.Count <= 0) { sys.locations.Add(new("testVC", "Main Street 1")); }

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

sys.SaveLocationsToFile();
sys.SaveUsersToFile();
sys.SaveEventsToFile();

bool isRunning = true;
while (isRunning)
{
  switch (currentMenu)
  {
    case Menu.Default:
      // try { Console.Clear(); } catch { }
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
      Console.WriteLine("\n[8] View A Users List of Permissions");
      Console.WriteLine("\n[9] Give Permission to Handle Permissions"); 
      Console.WriteLine("\n[j] View All Users");
      Console.WriteLine("\n[k] View Events by Type");
      Console.WriteLine("\n[m] Manage Permissions \n\n[v] View Permissions\n\n[x] Logout");
      Console.Write("\n> ");

      switch (Console.ReadLine())
      {
        // HandleAccount
        case "1":
          if (!activeUser.HasPermission(Permission.HandleAccount))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true);break; }
          sys.CreatePersonnelAccount();
          break;
        // HandleRegistration
        case "2":
          if (!activeUser.HasPermission(Permission.HandleRegistration))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          break;
        // HandleAppointment
        case "3":
          if (!activeUser.HasPermission(Permission.HandleAppointment))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          break;
        // JournalEntries
        case "4":
          if (!activeUser.HasPermission(Permission.JournalEntries))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          break;
        // AddLocation
        case "5":
          if (!activeUser.HasPermission(Permission.AddLocation))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          Location newLocation = null;

          Console.WriteLine("Name of Location?");
          Console.Write(">");
          string locName = Console.ReadLine();
          Console.WriteLine("Address of Location?");
          Console.Write(">");
          string locAddress = Console.ReadLine();
          List<Region> regionList = new();
          foreach (Region region in Enum.GetValues(typeof(Region)))
          {
            regionList.Add(region);
          }
                    
          for (int i = 1; i < regionList.Count; i++)
          {
            Console.WriteLine($"[{i}] {regionList[i].ToString()}");
          }
          int.TryParse(Console.ReadLine(), out int nr);
          Region locRegion = (Region)(nr);
          sys.locations.Add(new Location(locName, locAddress, locRegion));
          sys.SaveLocationsToFile();
          Console.ReadKey(true);
          break;
        // ScheduleOfLocation
        case "6":
          if (!activeUser.HasPermission(Permission.ScheduleOfLocation))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          
          break;
        // AssignRegion
        case "7":
          if (!activeUser.HasPermission(Permission.AssignRegion))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          break;
        // ViewPermissionList
        case "8":
          if (!activeUser.HasPermission(Permission.ViewPermissionList))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          break;
        // PermHandlePerm
        case "9":
          if (!activeUser.HasPermission(Permission.PermHandlePerm))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          break;
        case "j":
          // View All Users
          Console.WriteLine("\n=== ALL USERS ===");
          foreach (User user in sys.users)
          {
            Console.WriteLine($"Name: {user.Name} | SSN: {user.SSN}");
          }
          Console.Write("\nPress ENTER to continue.");
          Console.ReadKey(true);
          break;
        case "k":
          sys.ViewEvents();
          break;

        case "m": // Manage permissions
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
          Console.ReadKey(true);
          break;
      }
      break;
  }
}
