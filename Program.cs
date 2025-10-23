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
  sys.users.Add(new User("admin123", "admin", "admin User"));
  sys.users.Add(new User("testssn1", "test1", "Test Patient"));
  sys.users.Add(new User("testssn2", "test2", "Test Personnel"));
  sys.users.Add(new User("nursessn1", "nurse1", "Test Nurse"));
  sys.users[0].Permissions.Remove(Permission.None);
  sys.users[3].Permissions.Add(Permission.HandleAppointment);
  sys.users[3].UserRegion = Region.Halland;
  // Hard coding all the permission to admins permission list.
  foreach (Permission perm in sys.allPermissionList)
  {
    if (perm != Permission.None)
    {
      sys.users[0].Permissions.Add(perm);
    }
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

          int newSSNlenght = newSSN.Length;
          foreach (Event events in sys.eventList)
          {
            if (events.Title[..newSSNlenght] == newSSN)
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
            Event? newEvent = new($"{newSSN} PatientRequest", Event.EventType.Request);
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

        case "CheatersNeverLearn":
          sys.CheatersDelight();
          break;

        default:
          Console.Write("\nPlease enter a valid input. ");
          Console.ReadKey(true);
          break;
      }
      break;
    case Menu.Main:
      try { Console.Clear(); } catch { }
      Console.WriteLine($"\nWelcome, {activeUser?.Name}");
      Debug.Assert(activeUser != null);

      Console.WriteLine("\n[1] View My Journal");
      Console.WriteLine("\n[2] View My Schedule");
      Console.WriteLine("\n[3] Request an appointment.");
      if (!activeUser.HasPermission(Permission.None))
      {
        Console.WriteLine("\n[4] Handle Accounts");
        Console.WriteLine("\n[5] Handle Registrations");
        Console.WriteLine("\n[6] Handle Appointment");
        Console.WriteLine("\n[7] Add a Location");
        Console.WriteLine("\n[8] Schedule of a Location");
        Console.WriteLine("\n[9] View Permissions");
      }
      Console.WriteLine("\n[x] Logout");
      Console.Write("\n► ");

      switch (Console.ReadLine())
      {

        // View My Journal 
        case "1":
          try { Console.Clear(); } catch { }
          Debug.Assert(activeUser != null);
          sys.ViewEvent(Event.EventType.Entry, activeUser);
          Console.ReadKey(true);
          break;

        // View My Schedule 
        case "2":
          try { Console.Clear(); } catch { }
          Debug.Assert(activeUser != null);
          sys.ViewEvent(Event.EventType.Appointment, activeUser);
          Console.ReadKey(true);
          break;

        // Request Appointment 
        case "3":
          sys.RequestAppointment(activeUser);
          break;

        // Handle Account 
        case "4":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.HandleAccount))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.CreateAccount();
          break;

        // Handle Registration 
        case "5":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.HandleRegistration))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.ViewUserRequests();
          break;

        // Handle Appointment 
        case "6":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.HandleAppointment))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.HandleAppointment();
          Console.ReadKey(true);
          break;

        // Add Location 
        case "7":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.AddLocation))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.AddLocation();
          Console.ReadKey(true);
          break;

        // Schedule Of Location 
        case "8":
          if (!activeUser!.HasPermission(Permission.ScheduleOfLocation))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          try { Console.Clear(); } catch { }

          sys.ScheduleOfLocation();
          Console.ReadKey(true);
          break;

        // View Permissions 
        case "9":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.PermHandlePerm) && !activeUser!.HasPermission(Permission.ViewPermissionList))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.PermissionSystem(activeUser);
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
