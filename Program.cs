using System.Diagnostics;
using App;

// Logout
// Main Menu

List<IUser> users = new();

IUser? activeUser = null;

bool isRunning = true;
while (isRunning)
{
  try { Console.Clear(); } catch { }
  Console.WriteLine("\n[1] Login \n[2] Register Account\n[3] Quit\n");
  Console.Write("> ");
  string? input = Console.ReadLine();

  switch (input)
  {
    case "1":
      Console.Write("Please input your SSN: ");
      string? ssn = Console.ReadLine();
      Console.Write("Please input a password: ");
      string? password = Console.ReadLine();

      Debug.Assert(ssn != null);
      Debug.Assert(password != null);

      foreach (IUser user in users)
      {
        if (user.TryLogin(ssn, password))
        {
          activeUser = user;
          break;
        }
      }
      if (activeUser is User u)
      {
        Console.WriteLine("Welcome " + u.Name);
        Console.ReadLine();
      }
      break;
    case "2":
      Console.Write("Please input your SSN: ");
      string? newSSN = Console.ReadLine();
      if (string.IsNullOrWhiteSpace(newSSN))
      {
        Console.WriteLine("Invalid input");
        Console.ReadLine();
        break;
      }

      Console.Write("Please input a password: ");
      string? newPassword = Console.ReadLine();
      Console.Write("What is your name? ");
      string? newName = Console.ReadLine();
      if (string.IsNullOrWhiteSpace(newName))
      {
        Console.WriteLine("Invalid input");
        Console.ReadLine();
        break;
      }
      Debug.Assert(newSSN != null);
      Debug.Assert(newPassword != null);
      Debug.Assert(newName != null);
      users.Add(new User(newSSN, newPassword, newName));
      break;
    case "3":
      isRunning = false;
      break;
    default:
      Console.WriteLine("Please enter a valid input");
      Console.ReadLine();
      break;
  }
}