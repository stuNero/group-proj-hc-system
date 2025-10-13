
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

      break;

    case "2":
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

