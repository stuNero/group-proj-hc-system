using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Formats.Tar;

namespace App;

class HCSystem
{
    // SYSTEM LISTS
    public List<Event> eventList = new();
    public List<User> users = new();
    public List<Location> locations = new();
    public List<Permission> allPermissionList = new();

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
        foreach (User user in users)
        {
            string userPermissionLine = "";
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
                    userPermissionLine += $"{user.Permissions[i]}";
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

    public bool CheckUser(string ssn)
    {
        // Check if user with this SSN already exists
        foreach (User user in users)
        {
            if (user.SSN == ssn)
            {
                return false; // User already exists
            }
        }

        // Create new user

        return true;
    }


    public void PermissionSystem(User? activeUser)
    {
        Debug.Assert(activeUser != null);
        bool isRunning = true;
        while (isRunning)
        {
            try { Console.Clear(); } catch { }
            User? targetUser = null;
            bool isSelectingUser = true;
            while (isSelectingUser)
            {
                int userIndex = 1;
                List<User> selectableUser = new();
                // List of all users except activeuser
                foreach (User user in users)
                {
                    if (user != activeUser)
                    {
                        Console.WriteLine($"\n[{userIndex}] {user.Name} | SSN: {user.SSN}  ");
                        selectableUser.Add(user);
                        userIndex++;
                    }
                }
                Console.WriteLine("========================================");
                Console.WriteLine("\nPress [b] if you want to go back.");
                Console.Write("\nSelect a user to manage permission: ");

                string userInput = Console.ReadLine() ?? "".ToLower().Trim();
                if (userInput == "b")
                {
                    return;
                }
                else if (string.IsNullOrEmpty(userInput))
                {

                    Console.Write("\nPlease select a valid user: ");
                    try { Console.Clear(); } catch { }
                    continue;
                }

                if (!int.TryParse(userInput, out int selectedUser) || selectedUser < 1 || selectedUser > selectableUser.Count)
                {
                    Console.Write("\nPlease select a valid user: ");
                }

                else
                {
                    targetUser = selectableUser[selectedUser - 1];
                    break;
                }
                try { Console.Clear(); } catch { }
            }
            // MANAGE PERMISSION WITH VIEW
            if (activeUser.HasPermission(Permission.PermHandlePerm))
            {
                try { Console.Clear(); } catch { }
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
                    foreach (Permission perm in allPermissionList)
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
                        permIndex++;
                    }

                    Console.WriteLine("==================================");
                    Console.WriteLine("\nWrite 'done' when you are satisfied.");
                    Console.WriteLine($"\nSelect permission to give to: [{targetUser.Name}]");
                    Console.Write("\n► ");

                    string userInput = Console.ReadLine() ?? "".ToLower().Trim();
                    if (string.IsNullOrEmpty(userInput))
                    {
                        Console.WriteLine("\nPlease select a valid permission:");
                    }
                    else if (userInput == "done") { break; }
                    else
                    {
                        if (!int.TryParse(userInput, out int selectedPerm) || selectedPerm < 1 || selectedPerm > allPermissionList.Count)
                        {
                            Console.WriteLine("\nPlease select a valid permission:");
                        }
                        else
                        {
                            // add and remove permisson logic
                            Permission perm = allPermissionList[selectedPerm - 1];
                            if (!targetUser.HasPermission(perm))
                            { targetUser.Permissions.Add(perm); }

                            else { targetUser.Permissions.Remove(perm); }

                            // if a user has permhandelperm than the user should also have viewpermissionlist
                            if (targetUser.HasPermission(Permission.PermHandlePerm))
                            { targetUser.Permissions.Add(Permission.ViewPermissionList); }

                            // if a user has other permissions than remove none.
                            if (targetUser.HasPermission(Permission.None))
                            { targetUser.Permissions.Remove(Permission.None); }


                            // if a user selects none than remove all other permissions.
                            if (selectedPerm == 1)
                            {
                                targetUser.Permissions.Clear();
                                targetUser.Permissions.Add(Permission.None);
                            }
                            SaveUsersToFile();
                        }
                    }
                    try { Console.Clear(); } catch { }
                }
            }
            // VIEW PERMISSIONS ONLY
            else if (!activeUser.HasPermission(Permission.PermHandlePerm))
            {
                while (true)
                {
                    if (targetUser.Permissions.Contains(Permission.None))
                    {
                        try { Console.Clear(); } catch { }
                        Console.WriteLine($"\n{targetUser.Name} has no permissions.");
                        Console.WriteLine($"\nPress enter to continue...");
                        Console.ReadLine();
                    }
                    else
                    {
                        try { Console.Clear(); } catch { }
                        int permIndex = 1;
                        if (targetUser == null)
                        {
                            return;
                        }
                        Debug.Assert(targetUser != null);
                        Console.WriteLine($"\nPermission status for:     [{targetUser.Name}] \n");
                        targetUser.Permissions.Sort();

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
                            permIndex++;
                        }
                        Console.WriteLine("==================================");
                        Console.WriteLine("\nPress enter to continue...");
                        Console.ReadLine();
                        break;
                    }
                }
            }
        }
    }



    public bool CreateAccount()
    {
        Console.Write("\nEnter SSN for new User: ");
        string? newSSN = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newSSN))
        {
            Console.WriteLine("\nInvalid SSN. Press ENTER to continue.");
            Console.ReadLine();
            return false;
        }

        Console.Write("Enter password for new User: ");
        string? newPassword = Console.ReadLine();

        if (newPassword == null)
        {
            Console.WriteLine("\nInvalid password. Press ENTER to continue.");
            Console.ReadLine();
            return false;
        }

        Console.Write("Enter name for new User: ");
        string? newName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newName))
        {
            Console.WriteLine("\nInvalid name. Press ENTER to continue.");
            Console.ReadLine();
            return false;
        }

        if (CheckUser(newSSN))
        {
            User newUser = new(newSSN, newPassword, newName);
            users.Add(newUser);
            SaveUsersToFile();
            Console.WriteLine($"\nUser account created successfully for {newName}!");
            return true;
        }
        else
        {
            Console.WriteLine("\nFailed to create account. A user with this SSN already exists.");
            Console.Write("\nPress ENTER to continue.");
            Console.ReadLine();
            return false;
        }

        
    }
    public void ViewEvents(Event.EventType? eventType)
    {
        string typeTitle = "";

        if (eventType == Event.EventType.Request)
        {
            typeTitle = "REQUEST EVENTS";
        }
        else if (eventType == Event.EventType.Appointment)
        {
            typeTitle = "APPOINTMENT EVENTS";
        }


        Console.Clear();
        Console.WriteLine($"\n=== {typeTitle} ===");

        List<Event> filteredEvents = new List<Event>();
        if (eventType != null)
        {
            foreach (Event singleEvent in eventList)
            {
                if (singleEvent.MyEventType == eventType)
                {
                    filteredEvents.Add(singleEvent);
                }
            }
        }
        else
        {
            Console.WriteLine("Something went wrong, no event type is selected");
            Console.ReadKey(true);
            return;
        }

        if (filteredEvents.Count == 0)
        {
            Console.WriteLine($"No {typeTitle.ToLower()} found.");
        }
        else
        {
            int index = 1;
            foreach (Event events in filteredEvents)
            {
                Console.WriteLine($"\n --- {index++} ---");
                Console.WriteLine($"\nTitle: {events.Title}");
                Console.WriteLine($"Type: {events.MyEventType}");
                if (!string.IsNullOrWhiteSpace(events.Description))
                {
                    Console.WriteLine($"Description: {events.Description}");
                }

                if (events.StartDate != default)
                {
                    Console.WriteLine($"Start: {events.StartDate}");
                }

                if (events.EndDate != default)
                {
                    Console.WriteLine($"End: {events.EndDate}");
                }

                if (events.Participants.Count != 0)
                {
                    Console.WriteLine("Participants:");
                    foreach (Participant participant in events.Participants)
                    {
                        Console.WriteLine($"  - {participant.User.Name} ({participant.ParticipantRole})");
                    }
                }

                Console.WriteLine("------------------------");
            }
        }
        Console.Write("\nPress [b] to go back or select a number to select a request.");

        string? userInput = Console.ReadLine();

        if (userInput == "b")
        {
            return;
        }
        else if (int.TryParse(userInput, out int selectedRequest) && selectedRequest >= 1 && selectedRequest <= filteredEvents.Count)
        {
            Event SelectedRequest = filteredEvents[selectedRequest - 1];
            Console.Clear();
            Console.WriteLine($"\n === Selected Events ===");
            Console.WriteLine($"\nTitle: {SelectedRequest.Title}");
            Console.WriteLine($"Type: {SelectedRequest.MyEventType}");
            if (!string.IsNullOrWhiteSpace(SelectedRequest.Description))
            {
                Console.WriteLine($"Description: {SelectedRequest.Description}");
            }

            if (SelectedRequest.StartDate != default)
            {
                Console.WriteLine($"Start: {SelectedRequest.StartDate}");
            }

            if (SelectedRequest.EndDate != default)
            {
                Console.WriteLine($"End: {SelectedRequest.EndDate}");
            }

            if (SelectedRequest.Participants.Count != 0)
            {
                Console.WriteLine("Participants:");
                foreach (Participant participant in SelectedRequest.Participants)
                {
                    Console.WriteLine($"  - {participant.User.Name} ({participant.ParticipantRole})");
                }
            }
            Console.WriteLine("\n === Request Options ===");
            Console.WriteLine("[1] Accept Request");
            Console.WriteLine("[2] Deny request");
            Console.WriteLine("[b] Go back");
            Console.Write("\n► ");
            

            string? requestChoice = Console.ReadLine();
            if (requestChoice == "1")
            {
                Console.Clear();
        Console.WriteLine($"=== Accept Request ===");
        Console.WriteLine($"\n Request: {SelectedRequest.Description}");
                if (CreateAccount())
                {
                    Console.WriteLine("\nThe request has been accepted and account created.");
                    eventList.Remove(SelectedRequest);
                    SaveEventsToFile();
                }
                else {
                    Console.WriteLine("\nFailed to create account. The request has not been accepted.");
                }
            }
            else if (requestChoice == "2")
            {
                Console.Clear();
                Console.WriteLine("You have denied the request.");
                Console.WriteLine("Press ENTER to continue");
                eventList.Remove(SelectedRequest);
                SaveEventsToFile();
                Console.ReadLine();
            }
            else if (requestChoice == "b")
            {
                return;
            }

        }
        else
        {
            Console.WriteLine("\nWrong input, press ENTER to go back to menu.");
            Console.ReadKey(true);
        }
    }
    public void ViewEvent(Event.EventType eventType, User activeUser)
    {
        if (eventType == Event.EventType.Entry)
        {
            Console.WriteLine("Your Journal");
        }
        else
        {
            Console.WriteLine("Your Appointment");
        }
        foreach (Event event1 in eventList)
        {
            if (event1.MyEventType == Event.EventType.Entry)
            {
                foreach (Participant participant in event1.Participants)
                {
                    if (activeUser == participant.User)
                    {
                        Console.WriteLine("Title:       " + event1.Title);
                        Console.WriteLine("Description: " + event1.Description);
                        Console.WriteLine("Start Date:  " + event1.StartDate);
                        Console.WriteLine("End Date:    " + event1.EndDate);
                        Console.WriteLine("Location:    " + event1.Location!.Name);
                        Console.WriteLine("             " + event1.Location.Address);
                        Console.WriteLine("             " + event1.Location.Region);
                        Console.WriteLine("Participants: \n_________");
                        foreach (Participant part1 in event1.Participants)
                        {
                            Console.WriteLine("Name: " + part1.User.Name);
                            Console.WriteLine("Role: " + part1.ParticipantRole);
                            Console.WriteLine("_________");
                        }
                    }
                }
            }
        }
    }
}
