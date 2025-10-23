using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using App;

HCSystem sys = new();
User? activeUser = null;
Menu currentMenu = Menu.Default;

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
          bool foundUser = false;
          Debug.Assert(ssn != null);
          Debug.Assert(password != null);

          foreach (User user in sys.users)
          {
            if (user.TryLogin(ssn, password))
            {
              activeUser = user;
              currentMenu = Menu.Main;
              foundUser = true;
              break;
            }
          }
          if (!foundUser)
          {
            Console.WriteLine("\nNo user was found with those credentials.");
            Console.Write("\nPress ENTER to continue. ");
            Console.ReadLine();
          }
          break;
        case "2":

          bool foundSSN = false;

          Console.Write("\nPlease input your SSN: ");
          string? newSSN = Console.ReadLine()?.Trim();

          if (string.IsNullOrWhiteSpace(newSSN))
          {
            Console.WriteLine("\nInvalid input");
            Console.ReadKey(true);
            break;
          }

          foreach (Event events in sys.eventList)
          {
            if (events.Title.StartsWith(newSSN))
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
        Console.WriteLine("\n[10] Assign User To Region");
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
          {
            Console.WriteLine("\nYou do not have permission to view others permissions.");
            Console.WriteLine("\nYour permissions are:");
            foreach (Permission perm in activeUser.Permissions)
            {
              Console.WriteLine($"\n[{activeUser.Permissions.IndexOf(perm) + 1}] {perm}");
            }
            Console.Write("\nPress ENTER to go back to previos menu. ");
            Console.ReadKey(true); break;
          }
          sys.PermissionSystem(activeUser);
          break;
        case "10":
          try { Console.Clear(); } catch { }
          if (!activeUser!.HasPermission(Permission.AssignRegion))
          { Console.WriteLine("You do not have permission for this."); Console.ReadKey(true); break; }
          sys.AssignToRegion();
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
