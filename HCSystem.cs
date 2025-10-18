using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace App;

class HCSystem
{
    // SYSTEM LISTS
    public List<Event> eventList = new();
    public List<User> users = new();
    public List<Location> locations = new();

    // PUT ALL DIRECTORIES HERE
    string usersFile = @"csv-files/users-list.csv";
    string eventsFile = @"csv-files/events-list.csv";
    string locationsFile = @"csv-files/locations-list.csv";
    public HCSystem()
    {

        if (!Directory.Exists("csv-files"))
        { Directory.CreateDirectory("csv-files"); }

        if (!File.Exists(usersFile))
        { File.WriteAllText(usersFile, ""); }

        if (!File.Exists(eventsFile))
        { File.WriteAllText(eventsFile, ""); }

        if (!File.Exists(locationsFile))
        { File.WriteAllText(locationsFile, ""); }

        LoadUsersFromFile();
        LoadLocationsFromFile();
        LoadEventsFromFile();

    }
    public void LoadUsersFromFile()
    {
        string[] usersCsv = File.ReadAllLines(usersFile);
        foreach (string userLine in usersCsv)
        {
            string[] userSplitData = userLine.Split("~");
            Region userRegion = Enum.Parse<Region>(userSplitData[3]);
            List<Permission> userPermissions = new();

            string[] permissionData = userSplitData[4].Split("^");
            foreach (string permission in permissionData)
            {
                userPermissions.Add(Enum.Parse<Permission>(permission));
            }

            User? user = new(userSplitData[0], userSplitData[1], userSplitData[2], userRegion);
            user.Permissions = userPermissions;
            users.Add(user);
        }
    }
    public void SaveUsersToFile()
    {
        string userLines = "";
        string userPermissionLine = "";
        foreach (User user in users)
        {
            if (user.Permissions.Count == 0)
            {
                userPermissionLine = "None";
            }
            else
            {
                for (int i = 0; i < user.Permissions.Count; i++)
                {
                    if (i != 0)
                    {
                        userPermissionLine += "^";
                    }
                    else
                    {
                        userPermissionLine += $"{user.Permissions[i]}";
                    }
                }
            }
            userLines += $"{user.SSN}~{user.GetUserPassword()}~{user.Name}~{user.UserRegion}~{userPermissionLine}";
            userLines += Environment.NewLine;
        }
        File.WriteAllText(usersFile, userLines);
    }
    public void LoadEventsFromFile()
    {
        string eventFile = @"csv-files\events-list.csv";
        if (!File.Exists(eventFile))
        {
            File.WriteAllText(eventFile, "");
        }
        string[] eventsCsv = File.ReadAllLines(eventFile);
        foreach (string eventLine in eventsCsv)
        {
            string[] eventSplitData = eventLine.Split("~");
            string newEventTitle = eventSplitData[0];
            Event.EventType eventType = Enum.Parse<Event.EventType>(eventSplitData[1]);
            string newEventDescription = eventSplitData[2];
            DateTime newEventStartDate = DateTime.Parse(eventSplitData[3]);
            DateTime newEventEndDate = DateTime.Parse(eventSplitData[4]);
            Location? newEventLocation = null;

            foreach (Location location in locations)
            {
                if (location.Name == eventSplitData[5])
                {
                    newEventLocation = location;
                    break;
                }
            }

            Event? newEvent = new(newEventTitle, eventType);
            newEvent.Description = newEventDescription;
            newEvent.StartDate = newEventStartDate;
            newEvent.EndDate = newEventEndDate;
            newEvent.Location = newEventLocation;
            // Debug.Assert(newEventLocation != null);

            List<Participant> participantsList = new();

            string[] participants = eventSplitData[6].Split("^");
            for (int i = 0; i < participants.Length; i++)
            {
                string[] participantSplitData = participants[i].Split("¤");

                User? partUser = null;
                Role partRole = Role.None;
                foreach (User user in users)
                {
                    if (participantSplitData[0] == user.SSN)
                    {
                        partUser = user;
                        break;
                    }
                }

                switch (participantSplitData[1])
                {
                    case "Admin": partRole = Role.Admin; break;
                    case "Patient": partRole = Role.Patient; break;
                    case "Personnel": partRole = Role.Personnel; break;
                }

                if (partUser != null)
                {
                    participantsList.Add(new(partUser, partRole));
                }
                else
                {
                    break;
                }
                newEvent.Participants = participantsList;
            }
            eventList.Add(newEvent);
        }
    }
    public void SaveEventsToFile()
    {
        string eventLines = "";
        foreach (Event events in eventList)
        {
            string participantLines = "";

            if (events.Participants.Count == 0)
            {
                participantLines = $"None¤None";
            }
            else
            {
                for (int i = 0; i < events.Participants.Count; i++)
                {
                    if (i != 0)
                    {
                        participantLines += "^";
                    }
                    participantLines += $"{events.Participants[i].User.SSN}¤{events.Participants[i].ParticipantRole}";
                }
            }
            string eventLocation = "";
            if (events.Location != null)
            {
                eventLocation = events.Location.Name;
            }

            eventLines += $"{events.Title}~{events.MyEventType}~{events.Description}~{events.StartDate}~{events.EndDate}~{eventLocation}~{participantLines}";
            eventLines += Environment.NewLine;

        }
        File.WriteAllText(eventsFile, eventLines);
    }
    public void LoadLocationsFromFile()
    {
        string[] locationsCsv = File.ReadAllLines(locationsFile);
        foreach (string locationLine in locationsCsv)
        {
            string[] locationSplitData = locationLine.Split("~");

            Region newRegion = Enum.Parse<Region>(locationSplitData[2]);

            Location? location = new(locationSplitData[0], locationSplitData[1], newRegion);
            locations.Add(location);
        }
    }
    public void SaveLocationsToFile()
    {
        string locationLines = "";
        foreach (Location location in locations)
        {
            locationLines += $"{location.Name}~{location.Address}~{location.Region}";

            locationLines += Environment.NewLine;
        }
        File.WriteAllText(locationsFile, locationLines);
    }

    public bool CheckPersonnel(string ssn, string password, string name)
    {
        // Check if user with this SSN already exists
        foreach (User user in users)
        {
            if (user.SSN == ssn)
            {
                return false; // User already exists
            }
        }

        // Create new personnel user

        return true;
    }
    public void CreatePersonnelAccount()
    {
        Console.Write("\nEnter SSN for new personnel: ");
        string? newSSN = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newSSN))
        {
            Console.WriteLine("\nInvalid SSN. Press ENTER to continue.");
            Console.ReadLine();
            return;
        }

        Console.Write("Enter password for new personnel: ");
        string? newPassword = Console.ReadLine();

        if (newPassword == null)
        {
            Console.WriteLine("\nInvalid password. Press ENTER to continue.");
            Console.ReadLine();
            return;
        }

        Console.Write("Enter name for new personnel: ");
        string? newName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newName))
        {
            Console.WriteLine("\nInvalid name. Press ENTER to continue.");
            Console.ReadLine();
            return;
        }

        if (CheckPersonnel(newSSN, newPassword, newName))
        {
            User newPersonnel = new(newSSN, newPassword, newName);
            users.Add(newPersonnel);
            SaveUsersToFile();
            Console.WriteLine($"\nPersonnel account created successfully for {newName}!");
        }
        else
        {
            Console.WriteLine("\nFailed to create account. A user with this SSN already exists.");
        }

        Console.Write("\nPress ENTER to continue.");
        Console.ReadLine();
    }
    public void ViewEvents()
    {
        Console.WriteLine("\n=== VIEW EVENTS BY TYPE ===");
        Console.WriteLine("[1] Request Events");
        Console.WriteLine("[2] Appointment Events");
        Console.WriteLine("[3] Entry Events");
        Console.WriteLine("[4] All Events");
        Console.WriteLine("\n[b] Back to Admin Menu");
        Console.Write("\n> ");

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
                foreach (Event singleEvent in eventList)
                {
                    if (singleEvent.MyEventType == selectedType.Value)
                    {
                        filteredEvents.Add(singleEvent);
                    }
                }
            }
            else
            {
                foreach (Event singleEvent in eventList)
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
    }
}
