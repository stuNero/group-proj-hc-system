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
  sys.users.Add(new User("admin123", "admin", "Admin User", Role.Admin));
  sys.users.Add(new User("testssn1", "test1", "Test Patient", Role.Patient));
  sys.users.Add(new User("testssn2", "test2", "Test Personnel", Role.Personnel));
}

sys.SaveUsersToFile();


if (sys.eventList.Count <= 0)
{
  Event myEvent = new("event", Event.EventType.Request);
  myEvent.Participants.Add(new Participant(sys.users[0], Role.Patient));
  sys.eventList.Add(myEvent);

  Event mySecondEvent = new("My Appointment", Event.EventType.Appointment);
  mySecondEvent.Description = "I have a cold.";
  mySecondEvent.StartDate = new DateTime(2025, 10, 20, 11, 0, 0);
  mySecondEvent.EndDate = new DateTime(2025, 10, 20, 11, 30, 0);
  mySecondEvent.Participants.Add(new(sys.users[0], Role.Patient));
  mySecondEvent.Participants.Add(new(sys.users[1], Role.Personnel));
  mySecondEvent.Participants.Add(new(sys.users[2], Role.Admin));
  sys.eventList.Add(mySecondEvent);
}

sys.SaveEventsToFile();


// TEST CODE >>>>
/* foreach (Event events in sys.eventList)
{
  Console.WriteLine($"\n{events.Title} - {events.MyEventType} - {events.Description}\n"
  + $"{events.StartDate} - {events.EndDate}");
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
        default:
          Console.WriteLine("\nPlease enter a valid input");
          Console.ReadLine();
          break;
      }
      break;

    case Menu.Main:
      try { Console.Clear(); } catch { }
      Console.WriteLine($"\nWelcome, {activeUser?.Name} ({activeUser?.UserRole})");

      // Show role-specific options
      if (activeUser?.UserRole == Role.Admin)
      {
        Console.WriteLine("\n[1] Admin Menu");
      }
      else if (activeUser?.UserRole == Role.Patient)
      {
        Console.WriteLine("\n[1] Patient Options");
      }
      else if (activeUser?.UserRole == Role.Personnel)
      {
        Console.WriteLine("\n[1] Personnel Options");
      }

      Console.WriteLine("\n[x] Logout");
      Console.Write("\n> ");

      string? mainInput = Console.ReadLine();
      switch (mainInput)
      {
        case "1":
          if (activeUser?.UserRole == Role.Admin)
          {
            currentMenu = Menu.Admin;
          }
          else
          {
            Console.Write("\nThis feature is not yet implemented. Press ENTER to continue. ");
            Console.ReadLine();
          }
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

    case Menu.Admin:
      try { Console.Clear(); } catch { }
      Console.WriteLine($"\n=== ADMIN MENU ===");
      Console.WriteLine($"Welcome, {activeUser?.Name}");
      Console.WriteLine("\n[1] Create Personnel Account");
      Console.WriteLine("[2] View All Users");
      Console.WriteLine("[3] View All Events");
      Console.WriteLine("\n[b] Back to Main Menu");
      Console.WriteLine("[x] Logout");
      Console.Write("\n> ");

      string? adminInput = Console.ReadLine();
      switch (adminInput)
      {
        case "1":
          // Create Personnel Account
          Console.Write("\nEnter SSN for new personnel: ");
          string? newSSN = Console.ReadLine();

          if (string.IsNullOrWhiteSpace(newSSN))
          {
            Console.WriteLine("\nInvalid SSN. Press ENTER to continue.");
            Console.ReadLine();
            break;
          }

          Console.Write("Enter password for new personnel: ");
          string? newPassword = Console.ReadLine();

          if (string.IsNullOrWhiteSpace(newPassword))
          {
            Console.WriteLine("\nInvalid password. Press ENTER to continue.");
            Console.ReadLine();
            break;
          }

          Console.Write("Enter name for new personnel: ");
          string? newName = Console.ReadLine();

          if (string.IsNullOrWhiteSpace(newName))
          {
            Console.WriteLine("\nInvalid name. Press ENTER to continue.");
            Console.ReadLine();
            break;
          }

          if (sys.CreatePersonnelAccount(newSSN, newPassword, newName))
          {
            Console.WriteLine($"\nPersonnel account created successfully for {newName}!");
          }
          else
          {
            Console.WriteLine("\nFailed to create account. A user with this SSN already exists.");
          }

          Console.Write("\nPress ENTER to continue.");
          Console.ReadLine();
          break;

        case "2":
          // View All Users
          Console.WriteLine("\n=== ALL USERS ===");
          foreach (User user in sys.users)
          {
            Console.WriteLine($"Name: {user.Name} | SSN: {user.SSN} | Role: {user.UserRole}");
          }
          Console.Write("\nPress ENTER to continue.");
          Console.ReadLine();
          break;

        case "3":
          // View All Events
          Console.WriteLine("\n=== ALL EVENTS ===");
          if (sys.eventList.Count == 0)
          {
            Console.WriteLine("No events found.");
          }
          else
          {
            foreach (Event events in sys.eventList)
            {
              Console.WriteLine($"\nTitle: {events.Title}");
              Console.WriteLine($"Type: {events.MyEventType}");
              Console.WriteLine($"Description: {events.Description}");
              Console.WriteLine($"Start: {events.StartDate}");
              Console.WriteLine($"End: {events.EndDate}");
              Console.WriteLine("Participants:");
              foreach (Participant participant in events.Participants)
              {
                Console.WriteLine($"  - {participant.User.Name} ({participant.ParticipantRole})");
              }
              Console.WriteLine("------------------------");
            }
          }
          Console.Write("\nPress ENTER to continue.");
          Console.ReadLine();
          break;

        case "b":
          currentMenu = Menu.Main;
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