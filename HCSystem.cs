using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

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

        // Create new personnel user

        return true;
    }

    public void ManagePermissions(User? activeUser) // Permission method for managing all the permissions. only users with sufficient permission has acces to this.
    {
        try { Console.Clear(); } catch { }
        Debug.Assert(activeUser != null);
        if (activeUser.Permissions.Contains(Permission.PermHandlePerm))
        {
            User? targetUser = null;
            bool isSelectingUser = true;
            while (isSelectingUser)
            {
                int userIndex = 1;
                List<User> selectableUser = new();
                // List of all users
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
                foreach (Permission perm in users[0].Permissions)
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

                string userInput = Console.ReadLine() ?? "".ToLower().Trim();
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("\nPlease select a valid permission:");
                }
                else if (userInput == "done")
                {
                    isSelectingperm = false;
                    SaveUsersToFile();
                    break;
                }
                else
                {
                    if (!int.TryParse(userInput, out int selectedPerm) || selectedPerm < 1 || selectedPerm > users[0].Permissions.Count)
                    {
                        Console.WriteLine("\nPlease select a valid permission:");
                    }
                    else
                    {
                        Permission perm = users[0].Permissions[selectedPerm - 1];
                        if (!targetUser.Permissions.Contains(perm))
                        {
                            targetUser.Permissions.Add(perm);
                        }
                        else
                        {

                            targetUser.Permissions.Remove(perm);

                        }
                        // None permisson logic
                        if (targetUser.Permissions.Count > 1 && targetUser.Permissions.Contains(Permission.None))
                        {
                            targetUser.Permissions.Remove(Permission.None);
                            Console.WriteLine("\nAll permissions must be false for None to be true...");
                        }
                        else if (targetUser.Permissions.Count == 0)
                        {
                            targetUser.Permissions.Add(Permission.None);
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
    }


    public void ViewPermissions(User? activeUser, Menu currentMenu) // Method for vewing all the permissions and only users with sufficent permission can view this.
    {

        try { Console.Clear(); } catch { }
        Debug.Assert(activeUser != null);
        if (activeUser.Permissions.Contains(Permission.ViewPermissionList))
        {
            User? targetUser = null;
            bool isSelectingUser = true;
            while (isSelectingUser)
            {
                int userIndex = 1;
                List<User> selectableUser = new();
                // List of all users
                foreach (User user in users)
                {
                    if (user != activeUser)
                    {
                        Console.WriteLine($"\n[{userIndex}] {user.Name} | SSN: {user.SSN} ");
                        selectableUser.Add(user);
                        userIndex++;
                    }
                }
                Console.WriteLine("========================================");
                Console.WriteLine("\nPress [b] if you want to go back.");
                Console.Write("\nSelect a user to view permission: ");

                string userInput = Console.ReadLine() ?? "".ToLower().Trim();
                if (userInput == "b")
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
    }

    public void CreateAccount()
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

        if (CheckUser(newSSN))
        {
            User newPersonnel = new(newSSN, newPassword, newName);
            users.Add(newPersonnel);
            SaveUsersToFile();
            Console.WriteLine($"\nAccount created successfully for {newName}!");
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
    public void RequestAppointment(User activeUser)
    {
        try { Console.Clear(); } catch { }
        Console.WriteLine("\nRequest Appointment\n");
        Console.WriteLine("\nSelect region\n");

        foreach (Region region in Region.GetValues(typeof(Region)))
        {
            int regionIndex = (int)region;
            if (region != Region.None)
            { Console.WriteLine($"[{regionIndex}] {region}"); }
        }
        Console.Write("\nRegion [1-21]: ");
        int selectedRegion = Convert.ToInt32(Console.ReadLine());
        Location? selectedLocation = null;

        if (selectedRegion < 1 || selectedRegion > 21)
        {
            Console.Write("\nInvalid input. Press ENTER to go back to previous menu. ");
            Console.ReadLine();
            return;
        }
        else
        {
            bool foundLocation = false;
            Console.WriteLine("");
            foreach (Location location in locations)
            {
                if (selectedRegion == (int)location.Region)
                {
                    Console.WriteLine($"ID: [{locations.IndexOf(location) + 1}] - {location.Name}");
                    Console.WriteLine($"{location.Address}");
                    foundLocation = true;
                }
            }

            if (foundLocation)
            {
                Console.Write("\nSelect location ID: ");
                int selectedLocID = Convert.ToInt32(Console.ReadLine());

                if (selectedLocID < 0 || selectedLocID > locations.Count)
                {
                    Console.Write("\nInvalid input. Press ENTER to continue. ");
                    Console.ReadLine();
                    return;
                }

                selectedLocation = locations[selectedLocID - 1];
                if ((int)selectedLocation.Region != selectedRegion)
                {
                    Console.Write("\nInvalid input. Press ENTER to continue. ");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("\nDescribe the reason of the appointment:");
                Console.Write("► ");
                string? newReason = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(newReason))
                {
                    Console.WriteLine("\nReason can't be empty.");
                    Console.Write("\nPress ENTER to go back to previous menu. ");
                    Console.ReadLine();
                    return;
                }
                else
                {
                    Console.WriteLine("\nDo you have a desired date and time? Leave empty if none.\nKeep in mind that the appointment time is subject to availability.");
                    Console.WriteLine("\nPlease use (DD/MM/YY - HH:mm) format.");
                    Console.Write("► ");

                    string? desiredTime = Console.ReadLine();

                    try { Console.Clear(); } catch { }
                    Console.WriteLine("\nReview the appointment request before sending:");
                    Console.WriteLine($"\nYour name: {activeUser.Name}\nYour SSN: {activeUser.SSN}");
                    Console.WriteLine($"Reason of the appointment:\n"
                    + $"'{newReason}'");
                    Console.WriteLine($"\nDesired time: {(desiredTime == "" ? "None" : desiredTime)}");

                    Console.WriteLine($"\nAt: {selectedLocation.Name} - Address: {selectedLocation.Address}");

                    Console.WriteLine("\nIs the information correct?");
                    Console.Write("Y/N? ('N' would take you back to previous menu): ");
                    switch (Console.ReadLine()?.ToLower())
                    {
                        case "y":
                            Event? newEvent = new($"AppointmentRequest", Event.EventType.Request);
                            newEvent.Description = $"{newReason} | Desired time: {(desiredTime == "" ? "None" : desiredTime)}";
                            newEvent.Location = selectedLocation;
                            newEvent.Participants.Add(new(activeUser, Role.Patient));
                            eventList.Add(newEvent);
                            SaveEventsToFile();
                            Console.WriteLine("\nYour appoinment request has been registered.");
                            Console.Write("\nPress ENTER to continue. ");
                            Console.ReadLine();
                            break;

                        case "n": return;

                        default:
                            Console.Write("\nInvalid input. Press ENTER to continue. ");
                            Console.ReadLine();
                            break;
                    }
                }
            }
            else
            {
                Console.Write("\nInvalid Input. Press ENTER to go back to previous menu. ");
                Console.ReadLine();
                return;
            }
        }
    }
    public void HandleAppointment()
    {
        bool foundAppointment = false;
        Console.WriteLine("\nAppointment requests\n");
        foreach (Event events in eventList)
        {
            if (events.MyEventType == Event.EventType.Request && events.Title == "AppointmentRequest")
            {
                Console.WriteLine($"\nAppointment request ID = [{eventList.IndexOf(events) + 1}]");
                Console.WriteLine("\nDescription: " + events.Description);
                Console.WriteLine("Location:    " + events.Location!.Name);
                Console.WriteLine("             " + events.Location.Address);
                Console.WriteLine("             " + events.Location.Region);
                Console.WriteLine($"Patient: {events.Participants[0].User.Name}.");
                Console.WriteLine($"------------------------");
                foundAppointment = true;
            }
        }
        if (foundAppointment)
        {
            Console.Write("\nSelect ID the the request you want to handle: ");
            int requestID = Convert.ToInt32(Console.ReadLine());

            if (requestID < eventList.Count || requestID > eventList.Count)
            {
                Console.Write("\nInvalid input. Press ENTER to go back to previous menu. ");
                return;
            }
            else
            {
                Event? newEvent = null;
                foreach (Event events in eventList)
                {
                    if (requestID == eventList.IndexOf(events) + 1)
                    {
                        newEvent = events;
                        break;
                    }
                }
                Console.WriteLine("\nDo you want to accept this appointment request?");
                Console.Write("Y/N? ('N' would take you back to previous menu): ");

                switch (Console.ReadLine()?.ToLower())
                {
                    case "y":
                        Console.Write("\nAppointment time (DD/MM/YY HH:mm:ss): ");
                        string? dateInput = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(dateInput))
                        {
                            DateTime newDateTime;
                            if (DateTime.TryParse(dateInput, out newDateTime))
                            {

                                Debug.Assert(newEvent != null);
                                Console.WriteLine("\nSelect personnel: ");
                                foreach (User user in users)
                                {
                                    if (user != newEvent.Participants[0].User)
                                    {
                                        Console.WriteLine($"\nID: [{users.IndexOf(user) + 1}] {user.Name}");
                                    }
                                }
                                Console.Write("\n► ");
                                int selectedPersonnel = Convert.ToInt32(Console.ReadLine());
                                if (selectedPersonnel < 1 || selectedPersonnel > users.Count || users[selectedPersonnel - 1] == newEvent.Participants[0].User)
                                {
                                    Console.Write("\nInvalid input. Press ENTER to go back to previous menu. ");
                                    return;
                                }
                                else
                                {
                                    newEvent.Title = newEvent.Participants[0].User.SSN;
                                    Debug.Assert(newEvent.Description != null);
                                    string[] descriptionSplit = newEvent.Description.Split("|");
                                    newEvent.Description = descriptionSplit[0].Trim();
                                    newEvent.MyEventType = Event.EventType.Appointment;
                                    newEvent.StartDate = newDateTime;
                                    newEvent.EndDate = newDateTime.AddMinutes(30);
                                    newEvent.Participants.Add(new(users[selectedPersonnel - 1], Role.Personnel));
                                    SaveEventsToFile();
                                    Console.WriteLine("\nAppointment accepted.");
                                    Console.Write("\nPress ENTER to go back to previous menu. ");
                                    break;
                                }
                            }
                            else
                            {
                                Console.Write("\nInvalid input. Press ENTER to go back to previos menu. ");
                                return;
                            }
                        }
                        else
                        {
                            Console.Write("\nInvalid input. Press ENTER to go back to previos menu. ");
                            return;
                        }

                    case "n":
                        Debug.Assert(newEvent != null);
                        newEvent.MyEventType = Event.EventType.None;
                        SaveEventsToFile();
                        Console.WriteLine("\nAppointment rejected. Press ENTER to go back to previos menu. ");
                        return;

                    default:
                        Console.Write("\nInvalid input. Press ENTER to continue. ");
                        Console.ReadLine();
                        break;
                }
            }
        }
        else
        {
            Console.WriteLine("\nNo appointment request found.");
            Console.Write("\nPress ENTER to go back to previous menu. ");
            return;
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
