using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using App;


HCSystem sys = new();

User? activeUser = null;
Menu currentMenu = Menu.Default;

if (sys.users.Count <= 0)
{
  sys.users.Add(new User("testssn1", "", "test1"));
  sys.users.Add(new User("testssn2", "", "test2"));
  sys.users.Add(new User("testssn3", "", "test3"));
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


// TEST CODE
foreach (Event events in sys.eventList)
{
  Console.WriteLine($"\n{events.Title} - {events.MyEventType} - {events.Description}\n"
  + $"{events.StartDate} - {events.EndDate}");
  foreach (Participant participant in events.Participants)
  {
    Console.WriteLine($"{participant.User.Name} - {participant.User.SSN} - {participant.UserRoles}");
  }
  Console.WriteLine("\n----------------");
}
Console.Write("\nPress ENTER to continue.");
Console.ReadLine();


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

          sys.users.Add(new User(newSSN, newPassword, newName));
          sys.SaveUsersToFile();
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
          foreach (Event userEvent in sys.eventList)
          {
            foreach (Participant part in userEvent.Participants)
            {
              Debug.Assert(activeUser != null);
              if (part.User.SSN == activeUser.SSN)
              {
                Console.WriteLine($"\nRequest already exsists, you can just wait for now...\n");
                Console.ReadLine();
                ssnFound = true;
                break;
              }
            }
          }
          if (ssnFound == false)
          {
            Debug.Assert(activeUser != null);
            Participant newParticipant = new(activeUser, Role.Patient);
            Event newEvent = new($"New Event", Event.EventType.Request);
            newEvent.StartDate = DateTime.Now;
            newEvent.Description = $"{activeUser.Name} is requesting to become a patient.";
            newEvent.Participants.Add(newParticipant);
            sys.eventList.Add(newEvent);
            sys.SaveEventsToFile();
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