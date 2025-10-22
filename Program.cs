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
  sys.users.Add(new("admin123", "admin", "admin User"));
  sys.users.Add(new("testssn1", "test1", "Test Patient"));
  sys.users.Add(new("testssn2", "test2", "Test Personnel"));
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

if (sys.locations.Count <= 0)
{
  sys.locations.Add(new("testVC", "Main Street 1"));
  sys.locations.Add(new("Halmstad Sjukhus", "Lasarettvägen"));
  sys.locations[1].Region = Region.Halland;
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
      Debug.Assert(activeUser != null);
      if (!activeUser.Permissions.Contains(Permission.None))
      {
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
        Console.WriteLine("\n[m] Manage Permissions \n\n[v] View Permissions\n");
      }
      Console.WriteLine("\n[a] Request an apointment.");
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
          Console.WriteLine("W I P");
          Console.ReadKey(true);
          break;
        // HandleAppointment
        case "3":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.HandleAppointment))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          Console.WriteLine("W I P");
          Console.ReadKey(true);
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

          Console.WriteLine("Name of Location?");
          Console.Write(">");
          string? locName = Console.ReadLine();
          bool check = false;
          foreach (Location location in sys.locations)
          {
            if (location.Name == locName) check = true; break;
          }
          if (!check) { Console.WriteLine("Location already exists"); Console.ReadKey(true); break; }
          try { Console.Clear(); } catch { }
          Console.WriteLine("Address of Location?");
          Console.Write(">");
          string? locAddress = Console.ReadLine();
          List<Region> regionList = new();
          foreach (Region region in Enum.GetValues(typeof(Region)))
          {
            regionList.Add(region);
          }

          for (int i = 1; i < regionList.Count; i++)
          {
            Console.WriteLine($"[{i}] {regionList[i].ToString()}");
          }
          Console.Write("Choose region for location: ");
          int.TryParse(Console.ReadLine(), out int nr);
          Region locRegion = (Region)(nr);
          Debug.Assert(locName != null && locAddress != null);
          sys.locations.Add(new Location(locName, locAddress, locRegion));
          sys.SaveLocationsToFile();
          Console.ReadKey(true);
          break;
        // ScheduleOfLocation
        case "6":
          if (!activeUser!.HasPermission(Permission.ScheduleOfLocation))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          try { Console.Clear(); } catch { }
          Console.WriteLine("Which location do you want to see schedule of?");
          for (int i = 0; i < sys.locations.Count; i++)
          {
            Console.WriteLine($"[{i + 1}]\nName: {sys.locations[i].Name} \nAddress: {sys.locations[i].Name}");
          }
          Console.Write(">");
          string? choice = Console.ReadLine();

          if (!int.TryParse(choice, out nr))
          {
            Console.WriteLine("Invalid Location");
            break;
          }
          try { Console.Clear(); } catch { }
          foreach (Event scheduledEvent in sys.eventList)
          {
            if (scheduledEvent.Location == sys.locations[nr - 1])
            {
              Console.WriteLine("____________________________________________");
              Console.WriteLine($"Title: {scheduledEvent.Title}\nDescription: {scheduledEvent.Description}" +
              $"\nStart Date: {scheduledEvent.StartDate}\nEnd Date: {scheduledEvent.EndDate}\nType:{scheduledEvent.MyEventType}");
              Console.WriteLine("Participants: ");
              foreach (Participant participant in scheduledEvent.Participants)
              {
                Console.WriteLine($"Name: {participant.User.Name}:\nSSN:{participant.User.SSN}\nRole: {participant.ParticipantRole}");
              }
              Console.WriteLine("____________________________________________");
            }
          }
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
        // ViewPermissionList
        case "8":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.ViewPermissionList))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          Console.WriteLine("W I P");
          Console.ReadKey(true);
          break;
        // PermHandlePerm
        case "9":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.PermHandlePerm))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          Console.WriteLine("W I P");
          Console.ReadKey(true);
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
        case "k":
          try { Console.Clear(); } catch { }
          sys.ViewEvents();
          break;

        case "m": // Manage permissions
          try { Console.Clear(); } catch { }
          sys.ManagePermissions(activeUser);
          break;

        case "v": // View permissions
          try { Console.Clear(); } catch { }
          sys.ViewPermissions(activeUser, currentMenu);
          break;

        case "x":
          activeUser = null;
          currentMenu = Menu.Default;
          break;

        case "a":
          sys.RequestAppointment(activeUser);
          break;

        default:
          Console.Write("\nInvalid input. Press ENTER to continue. ");
          Console.ReadKey(true);
          break;
      }
      break;
  }
}
