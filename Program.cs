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

// Hard coding all the permission to admins permission list.
List<Permission> allPermList = new();
foreach (User user in sys.users)
{
  if (user.SSN == "admin123")
  {
    int permIndex = 1;
    {
      foreach (Permission perm in Enum.GetValues(typeof(Permission)))
      {
        user.Permissions.Add(perm);
        allPermList.Add(perm);
        permIndex++;
      }
    }
  }
  break;
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
        Console.WriteLine("\n[1] Patient Menu");
      }
      else if (activeUser?.UserRole == Role.Personnel)
      {
        Console.WriteLine("\n[1] Personnel Menu");
      }



      Console.WriteLine("[m] Manage Permissions \n[v] View Permissions\n\n[x] Logout");
      Console.Write("\n► ");

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

        case "m": // Manage permessions
          try { Console.Clear(); } catch { }
          if (activeUser.Permissions.Contains(Permission.PermHandlePerm))
          {
            User targetUser = null;
            bool isSelectingUser = true;
            while (isSelectingUser)
            {
              int userIndex = 1;
              List<User> selectableUser = new();
              // List of all users
              foreach (User user in sys.users)
              {
                if (user != activeUser)
                {
                  Console.WriteLine($"\n[{userIndex}] {user.Name} | SSN: {user.SSN} | Role: {user.UserRole} ");
                  selectableUser.Add(user);
                  userIndex++;
                }
              }
              Console.WriteLine("========================================");
              Console.WriteLine("\nWrite 'done' if you want to go back.");
              Console.Write("\nSelect a user to manage permission: ");

              string userInput = Console.ReadLine().ToLower().Trim();
              if (userInput == "done")
              {
                isSelectingUser = false;
                break;
              }
              else if (string.IsNullOrEmpty(userInput))
              {

                Console.Write("\nPlease select a valid user: ");
              }
              else
              {
                if (!int.TryParse(userInput, out int selectedUser) || selectedUser < 1 || selectedUser > selectableUser.Count)
                {
                  Console.Write("\nPlease select a valid user: ");
                }

                else
                {
                  targetUser = selectableUser[selectedUser - 1];
                  if (targetUser.Permissions.Contains(Permission.SysPermissions) && !activeUser.Permissions.Contains(Permission.SysPermissions))
                  {
                    Console.WriteLine($"\nYou are not authorized to manage [{targetUser.Name}'s] Permissions.");
                    Console.WriteLine("\nPress enter to continue...");
                    Console.ReadLine();

                  }
                  else if (targetUser.SSN != activeUser.SSN)
                  {
                    isSelectingUser = false;
                    break;
                  }
                }
              }
              try { Console.Clear(); } catch { }
            }
            try { Console.Clear(); } catch { }
            List<Permission> permList = new();
            bool isSelectingperm = true;
            while (isSelectingperm)
            {
              int permIndex = 1;
              if (targetUser == null)
              {
                break;
              }
              Debug.Assert(targetUser != null);
              Console.WriteLine($"\nPermission status for:     [{targetUser.Name}] \n");
              foreach (Permission perm in activeUser.Permissions)
              {
                bool targetUserPermBool = false;

                Debug.Assert(targetUser != null);
                if (targetUser.Permissions.Contains(perm))
                {
                  targetUserPermBool = true;
                  string index = $"[{permIndex}]".PadRight(4);
                  string permName = perm.ToString().PadRight(21);
                  Console.WriteLine($"\n{index}   {permName}{targetUserPermBool}");
                }
                else
                {
                  targetUserPermBool = false;
                  string index = $"[{permIndex}]".PadRight(4);
                  string permName = perm.ToString().PadRight(21);
                  Console.WriteLine($"\n{index}   {permName}{targetUserPermBool}");
                }

                permList.Add(perm);
                permIndex++;
              }
              Console.WriteLine("==================================");
              Console.WriteLine("\nWrite 'done' when you are satisfied.");
              Console.WriteLine($"\nSelect permission to give to: [{targetUser.Name}]");
              Console.Write("\n► ");

              string userInput = Console.ReadLine().ToLower().Trim();
              if (string.IsNullOrEmpty(userInput))
              {
                Console.WriteLine("\nPlease select a valid permission:");
              }
              else if (userInput == "done")
              {
                isSelectingperm = false;
                break;
              }
              else
              {
                if (!int.TryParse(userInput, out int selectedPerm) || selectedPerm < 1 || selectedPerm > activeUser.Permissions.Count)
                {
                  Console.WriteLine("\nPlease select a valid permission:");
                }
                else
                {
                  Permission perm = permList[selectedPerm - 1];
                  if (!targetUser.Permissions.Contains(perm))
                  {
                    targetUser.Permissions.Add(perm);
                  }
                  else
                  {

                    targetUser.Permissions.Remove(perm);

                  }
                }
              }
              try { Console.Clear(); } catch { }
            }
          }
          else
          {
            Console.WriteLine("\nSorry you are not authorized to manage permissions.");
            Console.WriteLine("\nPress enter to continue...");
            Console.ReadLine();
          }
          break;

        case "v": // View permissions
          try { Console.Clear(); } catch { }
          if (activeUser.Permissions.Contains(Permission.ViewPermissionList))
          {
            User targetUser = null;
            bool isSelectingUser = true;
            while (isSelectingUser)
            {
              int userIndex = 1;
              List<User> selectableUser = new();
              // List of all users
              foreach (User user in sys.users)
              {
                if (user != activeUser)
                {
                  Console.WriteLine($"\n[{userIndex}] {user.Name} | SSN: {user.SSN} | Role: {user.UserRole} ");
                  selectableUser.Add(user);
                  userIndex++;
                }
              }
              Console.WriteLine("========================================");
              Console.WriteLine("\nWrite 'done' if you want to go back.");
              Console.Write("\nSelect a user to view permission: ");

              string userInput = Console.ReadLine().ToLower().Trim();
              if (userInput == "done")
              {
                isSelectingUser = false;
                currentMenu = Menu.Main;
                break;
              }
              else if (string.IsNullOrEmpty(userInput))
              {

                Console.Write("\nPlease select a valid user: ");
              }
              else
              {
                if (!int.TryParse(userInput, out int selectedUser) || selectedUser < 1 || selectedUser > selectableUser.Count)
                {
                  Console.Write("\nPlease select a valid user: ");
                }
                else
                {
                  targetUser = selectableUser[selectedUser - 1];
                  if (targetUser.Permissions.Count == 0)
                  {
                    try { Console.Clear(); } catch { }
                    Console.WriteLine($"\n{targetUser.Name} has no permissions.");
                    Console.WriteLine($"\nPress enter to continue...");
                    Console.ReadLine();
                  }
                  else
                  {
                    try { Console.Clear(); } catch { }
                    List<Permission> permList = new();

                    int permIndex = 1;
                    if (targetUser == null)
                    {
                      currentMenu = Menu.Main;
                      break;
                    }
                    Debug.Assert(targetUser != null);
                    Console.WriteLine($"\nPermission status for:     [{targetUser.Name}] \n");

                    foreach (Permission perm in targetUser.Permissions)
                    {
                      bool targetUserPermBool = false;

                      Debug.Assert(targetUser != null);
                      if (targetUser.Permissions.Contains(perm))
                      {
                        targetUserPermBool = true;
                        string index = $"[{permIndex}]".PadRight(4);
                        string permName = perm.ToString().PadRight(21);
                        Console.WriteLine($"\n{index}   {permName}{targetUserPermBool}");
                      }
                      else
                      {
                        targetUserPermBool = false;
                        string index = $"[{permIndex}]".PadRight(4);
                        string permName = perm.ToString().PadRight(21);
                        Console.WriteLine($"\n{index}   {permName}{targetUserPermBool}");
                      }

                      permList.Add(perm);
                      permIndex++;
                    }
                    Console.WriteLine("==================================");
                    Console.WriteLine("\nPress enter to continue...");
                    Console.ReadLine();
                    break;
                  }

                }
              }
              try { Console.Clear(); } catch { }
            }
          }
          else
          {
            Console.WriteLine("\nSorry you are not authorized to view permissions.");
            Console.WriteLine("\nPress enter to continue...");
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
      Console.WriteLine("[3] View Events by Type");
      Console.WriteLine("\n[b] Back to Main Menu");
      Console.WriteLine("[x] Logout");
      Console.Write("\n► ");

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

          if (newPassword == null)
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
          Console.WriteLine("\n=== VIEW EVENTS BY TYPE ===");
          Console.WriteLine("[1] Request Events");
          Console.WriteLine("[2] Appointment Events");
          Console.WriteLine("[3] Entry Events");
          Console.WriteLine("[4] All Events");
          Console.WriteLine("\n[b] Back to Admin Menu");
          Console.Write("\n► ");

          string? eventTypeChoice = Console.ReadLine();
          Event.EventType? selectedType = null;
          string typeTitle = "";

          switch (eventTypeChoice)
          {
            case "1":
              selectedType = Event.EventType.Request;
              typeTitle = "REQUEST EVENTS";
              break;
            case "2":
              selectedType = Event.EventType.Appointment;
              typeTitle = "APPOINTMENT EVENTS";
              break;
            case "3":
              selectedType = Event.EventType.Entry;
              typeTitle = "ENTRY EVENTS";
              break;
            case "4":
              selectedType = null;
              typeTitle = "ALL EVENTS";
              break;
            case "b":
              break;
            default:
              Console.Write("\nInvalid input. Press ENTER to continue.");
              Console.ReadLine();
              break;
          }

          if (eventTypeChoice != "b" && eventTypeChoice != null && (eventTypeChoice == "1" || eventTypeChoice == "2" || eventTypeChoice == "3" || eventTypeChoice == "4"))
          {
            Console.WriteLine($"\n=== {typeTitle} ===");

            List<Event> filteredEvents = new List<Event>();
            if (selectedType.HasValue)
            {
              foreach (Event singleEvent in sys.eventList)
              {
                if (singleEvent.MyEventType == selectedType.Value)
                {
                  filteredEvents.Add(singleEvent);
                }
              }
            }
            else
            {
              foreach (Event singleEvent in sys.eventList)
              {
                filteredEvents.Add(singleEvent);
              }
            }

            if (filteredEvents.Count == 0)
            {
              Console.WriteLine($"No {typeTitle.ToLower()} found.");
            }
            else
            {
              foreach (Event events in filteredEvents)
              {
                Console.WriteLine($"\nTitle: {events.Title}");
                Console.WriteLine($"Type: {events.MyEventType}");
                if (string.IsNullOrWhiteSpace(events.Description))
                { }
                else
                {
                  Console.WriteLine($"Description: {events.Description}");
                }
                if (events.StartDate == default(DateTime)) { }
                else
                {
                  Console.WriteLine($"Start: {events.StartDate}");
                }

                if (events.EndDate == default(DateTime))
                {

                }
                else
                {
                  Console.WriteLine($"End: {events.EndDate}");
                }
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
          }
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