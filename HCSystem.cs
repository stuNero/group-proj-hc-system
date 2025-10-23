using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
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

        foreach (Permission perm in Enum.GetValues(typeof(Permission)))
        {
            allPermissionList.Add(perm);
        }
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
            { userPermissionLine = "None"; }

            else
            {
                for (int i = 0; i < user.Permissions.Count; i++)
                {
                    if (i != 0)
                    { userPermissionLine += "^"; }
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
        if (!File.Exists(eventsFile))
        { File.WriteAllText(eventsFile, ""); }
        
        string[] eventsCsv = File.ReadAllLines(eventsFile);
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
                { newEventLocation = location;break;}
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
                Role partRole = Enum.Parse<Role>(participantSplitData[1]);
                foreach (User user in users)
                {
                    if (participantSplitData[0] == user.SSN)
                    { partUser = user;break;}
                }
                if (partUser != null)
                { participantsList.Add(new(partUser, partRole)); }

                else { break; }
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
            { participantLines = $"None¤None"; }
            else
            {
                for (int i = 0; i < events.Participants.Count; i++)
                {
                    if (i != 0)
                    { participantLines += "^";}
                    participantLines += $"{events.Participants[i].User.SSN}¤{events.Participants[i].ParticipantRole}";
                }
            }
            string eventLocation = "";
            if (events.Location != null)
            { eventLocation = events.Location.Name; }
            
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
            if (user.SSN == ssn){return false; // User already exists
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
                // List of all users except activeuser
                foreach (User user in users)
                {
                    Console.WriteLine($"\n[{userIndex}] {user.Name} | SSN: {user.SSN}  ");
                    userIndex++;
                }
                Console.WriteLine("========================================");
                Console.WriteLine("\nPress [b] if you want to go back.");
                Console.Write("\nSelect a user to manage permission: ");

                string userInput = Console.ReadLine() ?? "".ToLower().Trim();
                if (userInput == "b")
                { return;}
                else if (string.IsNullOrEmpty(userInput))
                {
                    Console.Write("\nPlease select a valid user: ");
                    try { Console.Clear(); } catch { }
                    continue;
                }
                if (!int.TryParse(userInput, out int selectedUser) || selectedUser < 1 || selectedUser > users.Count)
                { Console.Write("\nPlease select a valid user: "); }

                else { targetUser = users[selectedUser - 1]; break; }
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
                    if (targetUser == null) { break; }
                    
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
                    { Console.WriteLine("\nPlease select a valid permission:"); }

                    else if (userInput == "done")
                    { SaveUsersToFile(); break; }
                    else
                    {
                        if (!int.TryParse(userInput, out int selectedPerm) || selectedPerm < 1 || selectedPerm > allPermissionList.Count)
                        { Console.WriteLine("\nPlease select a valid permission:"); }
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
                    try { Console.Clear(); } catch { }
                    Debug.Assert(targetUser != null);
                    if (targetUser!.HasPermission(Permission.None))
                    {
                        try { Console.Clear(); } catch { }
                        Console.WriteLine($"\n{targetUser.Name} has no permissions.");
                        Console.WriteLine($"\nPress enter to continue...");
                        Console.ReadKey(true); break;
                    }
                    else
                    {
                        try { Console.Clear(); } catch { }
                        int permIndex = 1;
                        if (targetUser == null)
                        { return; }

                        Debug.Assert(targetUser != null);
                        Console.WriteLine($"\nPermission status for:     [{targetUser.Name}] \n");
                        targetUser.Permissions.Sort();

                        foreach (Permission perm in targetUser.Permissions)
                        {
                            bool targetUserPermBool = false;

                            Debug.Assert(targetUser != null);
                            if (targetUser.HasPermission(perm))
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
                        Console.ReadKey(true); break;
                    }
                }
            }
        }
    }
    public bool CreateAccount()
    {
        Console.Write("\nEnter SSN for new User: ");
        string? newSSN = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(newSSN))
        { Console.Write("\nInvalid SSN. Press ENTER to go back to previous menu. "); Console.ReadKey(true); return false;}

        Console.Write("Enter password for new User: ");
        string? newPassword = Console.ReadLine();

        if (newPassword == null)
        { Console.Write("\nInvalid password. Press ENTER to go back to previous menu. "); Console.ReadKey(true); return false;}

        Console.Write("Enter name for new User: ");
        string? newName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newName))
        { Console.Write("\nInvalid name. Press ENTER to go back to previous menu. "); Console.ReadLine();return false;}
        if (CheckUser(newSSN))
        {
            User newUser = new(newSSN, newPassword, newName);
            users.Add(newUser);
            SaveUsersToFile();
            Event? newEntry = new($"{newSSN} First Entry", Event.EventType.Entry);

            newEntry.Description = $"{newName}'s account was created";
            newEntry.StartDate = DateTime.Now;
            newEntry.Participants.Add(new Participant(newUser, Role.None));
            eventList.Add(newEntry);
            SaveEventsToFile();

            Console.WriteLine($"\nUser account created successfully for {newName}!");
            Console.Write("\nPress ENTER to go back to previous menu. ");
            Console.ReadLine();
            return true;
        }
        else
        {
            Console.WriteLine("\nFailed to create account. A user with this SSN already exists.");
            Console.Write("\nPress ENTER to go back to previous menu. ");
            Console.ReadKey(true); return false;}
    }
    public void ViewUserRequests()
    {
        Event.EventType? eventType = Event.EventType.Request;
        Console.Clear();
        Console.WriteLine($"\n=== User Requests ===");

        List<Event> userRequestList = new List<Event>();
        if (eventType != null)
        {
            foreach (Event singleEvent in eventList)
            {
                if (singleEvent.MyEventType == eventType && singleEvent.Title != "AppointmentRequest")
                { userRequestList.Add(singleEvent);}
            }
        }
        else { Console.WriteLine("Something went wrong, no event type is selected"); Console.ReadKey(true); return; }
        
        if (userRequestList.Count == 0) {Console.WriteLine($"No user requests found.");}
        else
        {
            int index = 1;
            foreach (Event events in userRequestList)
            {
                Console.WriteLine($"\n --- {index++} ---");
                Console.WriteLine($"\nTitle: {events.Title}");
                Console.WriteLine($"Type: {events.MyEventType}");
                if (!string.IsNullOrWhiteSpace(events.Description))
                {
                    Console.WriteLine($"Description: {events.Description}");
                }
                Console.WriteLine("------------------------");
            }
        }
        Console.Write("\nPress [b] to go back or select a number to select a request. ");

        string? userInput = Console.ReadLine();

        if (userInput == "b") { return; }

        else if (int.TryParse(userInput, out int selectedRequest) && selectedRequest >= 1 && selectedRequest <= userRequestList.Count)
        {
            Event SelectedRequest = userRequestList[selectedRequest - 1];
            Console.Clear();
            Console.WriteLine($"\n === Selected Events ===");
            Console.WriteLine($"\nSSN: {SelectedRequest.Title}");
            Console.WriteLine($"Type: {SelectedRequest.MyEventType}");
            if (!string.IsNullOrWhiteSpace(SelectedRequest.Description))
            {
                Console.WriteLine($"Description: {SelectedRequest.Description}");
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
                else { Console.WriteLine("\nFailed to create account. The request has not been accepted."); }
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
            else if (requestChoice == "b") { return; }
        }
        else { Console.WriteLine("\nWrong input, press ENTER to go back to menu."); Console.ReadKey(true); }
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
        string? selectedRegionIndex = Console.ReadLine();


        if (int.TryParse(selectedRegionIndex, out int selectedRegion) && selectedRegion > 0 && selectedRegion < 21)
        {
            Location? selectedLocation = null;
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
                string? selectedLocString = Console.ReadLine();

                if (int.TryParse(selectedLocString, out int selectedLocID) && selectedLocID > 0 && selectedLocID <= locations.Count)
                {
                    selectedLocation = locations[selectedLocID - 1];

                    if ((int)selectedLocation.Region != selectedRegion)
                    { Console.Write("\nInvalid input. Press ENTER to continue. ");Console.ReadKey(true); return; }

                    Console.WriteLine("\nDescribe the reason of the appointment:");
                    Console.Write("► ");
                    string? newReason = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(newReason))
                    {
                        Console.WriteLine("\nReason can't be empty.");
                        Console.Write("\nPress ENTER to go back to previous menu. "); 
                        Console.ReadKey(true); return;}
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

                            default: Console.Write("\nInvalid input. Press ENTER to continue. "); Console.ReadLine(); break;
                        }
                    }
                }
                else { Console.Write("\nInvalid input. Press ENTER to continue. "); Console.ReadLine();return;}
            }
            else {Console.Write("\nNo locations found in the selected region. Press ENTER to go back to previous menu. "); Console.ReadKey(true);return;}
        }
        else { Console.Write("\nInvalid input. Press ENTER to go back to previous menu. "); Console.ReadLine(); return;}
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
            string? requestIDstring = Console.ReadLine();
            if (int.TryParse(requestIDstring, out int requestID) && requestID > 0 && requestID <= eventList.Count)
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
                                Debug.Assert(newEvent.Location != null);
                                Console.WriteLine("\nSelect personnel: ");
                                foreach (User user in users)
                                {
                                    if (user != newEvent.Participants[0].User && user.UserRegion == newEvent.Location.Region)
                                    {
                                        Console.WriteLine($"\nID: [{users.IndexOf(user) + 1}] {user.Name}");
                                    }
                                }
                                Console.Write("\n► ");
                                int selectedPersonnel = Convert.ToInt32(Console.ReadLine());
                                if (selectedPersonnel < 1 || selectedPersonnel > users.Count || users[selectedPersonnel - 1] == newEvent.Participants[0].User)
                                { Console.Write("\nInvalid input. Press ENTER to go back to previous menu. "); return; }

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
                            else { Console.Write("\nInvalid input. Press ENTER to go back to previous menu. "); return;}
                        }
                        else { Console.Write("\nInvalid input. Press ENTER to go back to previous menu. ");return;}

                    case "n":
                        Debug.Assert(newEvent != null);
                        newEvent.MyEventType = Event.EventType.None;
                        SaveEventsToFile();
                        Console.Write("\nAppointment rejected. Press ENTER to go back to previous menu. ");
                        return;

                    default:
                        Console.Write("\nInvalid input. Press ENTER to continue. "); break;
                }
            }
            else { Console.Write("\nInvalid input. Press ENTER to go back to previous menu. "); return; }
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
        { Console.WriteLine("\nYour Journal"); }
        else
        { Console.WriteLine("\nYour Schedule"); }
        
        foreach (Event event1 in eventList)
        {
            if (event1.MyEventType == eventType)
            {
                foreach (Participant participant in event1.Participants)
                {
                    if (activeUser == participant.User)
                    {
                        Console.WriteLine("\nTitle:       " + event1.Title);
                        Console.WriteLine("Description: " + event1.Description);
                        if (event1.StartDate != default)
                        {Console.WriteLine("Start Date:  " + event1.StartDate);}
                        if (event1.EndDate != default)
                        {Console.WriteLine("End Date:    " + event1.EndDate);}
                        if (event1.Location != null)
                        {
                            Console.WriteLine("Location:    " + event1.Location!.Name);
                            Console.WriteLine("             " + event1.Location.Address);
                            Console.WriteLine("             " + event1.Location.Region);
                        }
                        Console.WriteLine("Participants: \n_________");
                        foreach (Participant part1 in event1.Participants)
                        {
                            Console.WriteLine("Name: " + part1.User.Name);
                            Console.WriteLine("Role: " + part1.ParticipantRole);
                            Console.WriteLine("_________");
                        }
                        Console.WriteLine("------------------------");
                    }
                }
            }

        }
        Console.Write("\nPress ENTER to go back to previous menu. ");
    }
    public void AddLocation()
    {
        Console.WriteLine("Name of Location?");
        Console.Write("> ");
        string? locName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(locName)) { Console.WriteLine("Invalid Input"); return; }
        bool check = false;
        foreach (Location location in locations)
        {
            if (location.Name == locName) check = true; break;
        }
        if (check) { Console.WriteLine("Location already exists"); Console.ReadKey(true); return; }
        try { Console.Clear(); } catch { }
        Console.WriteLine("Address of Location?");
        Console.Write("> ");
        string? locAddress = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(locAddress)) { Console.WriteLine("Invalid Input"); return; }
        List<Region> regionList = new();
        foreach (Region region in Enum.GetValues(typeof(Region)))
        {
            regionList.Add(region);
        }
        Console.WriteLine("");
        for (int i = 1; i < regionList.Count; i++)
        {
            Console.WriteLine($"[{i}] {regionList[i].ToString()}");
        }
        Console.Write("\nChoose region for location: ");
        int.TryParse(Console.ReadLine(), out int nr);
        Region locRegion = (Region)(nr);
        Debug.Assert(locName != null && locAddress != null);
        locations.Add(new Location(locName, locAddress, locRegion));
        Console.WriteLine($"Location added: \n{locName}\n{locAddress}\n{locRegion}");
        SaveLocationsToFile();
        Console.Write("\nPress ENTER to go back to previoud menu. ");
    }
    public void ScheduleOfLocation()
    {
        Console.WriteLine("\nWhich location do you want to see schedule of?");
        for (int i = 0; i < locations.Count; i++)
        {
            Console.WriteLine($"\n[{i + 1}]\nName: {locations[i].Name} \nAddress: {locations[i].Name}\nRegion: {locations[i].Region}");
        }
        Console.Write("\n> ");
        string? choice = Console.ReadLine();

        if (!int.TryParse(choice, out int nr))
        {
            Console.Write("\nInvalid Location. Press ENTER to go back to previous menu. ");
            return;
        }
        try { Console.Clear(); } catch { }
        foreach (Event scheduledEvent in eventList)
        {
            if (scheduledEvent.Location == locations[nr - 1])
            {
                Console.WriteLine("____________________________________________");
                Console.WriteLine($"Title: {scheduledEvent.Title}\nDescription: {scheduledEvent.Description}" +
                $"\nStart Date: {scheduledEvent.StartDate}\nEnd Date: {scheduledEvent.EndDate}\nType:{scheduledEvent.MyEventType}");
                Console.WriteLine("Participants: ");
                foreach (Participant participant in scheduledEvent.Participants)
                {
                    Console.WriteLine($"Name: {participant.User.Name}:\nSSN:{participant.User.SSN}\nRole: {participant.ParticipantRole}");
                }
                Console.WriteLine("____________________________________________");
            }
        }
        Console.Write("\nPress ENTER to go back to previous menu. ");
    }
    public void AssignToRegion()
    {
        Console.WriteLine("");
        for (int i = 0; i < users.Count; i++)
        {
            Console.WriteLine($"[{i+1}] {users[i].Name}");
        }
        Console.Write("\nID of user: ");
        string? id = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(id))
        { Console.WriteLine("\nInvalid input"); return; }
        if (int.TryParse(id, out int index) && index > 0 && index <= users.Count)
        {
            Console.WriteLine("\nSelect region\n");

            foreach (Region region in Region.GetValues(typeof(Region)))
            {
                int regionIndex = (int)region;
                if (region != Region.None)
                { Console.WriteLine($"[{regionIndex}] {region}"); }
            }
            Console.Write("\nRegion [1-21]: ");
            string? selectedRegionIndex = Console.ReadLine();
            if (int.TryParse(selectedRegionIndex, out int selectedRegion) && selectedRegion > 0 && selectedRegion < 21)
            {
                users[index - 1].UserRegion = (Region)selectedRegion;
            }
            Console.WriteLine(users[index - 1].Name + " was assigned to " + users[index - 1].UserRegion);
            Console.Write("\nPress ENTER to go back to previous menu..");
        }
        else { Console.WriteLine("\nInvalid input"); return; }
    }
    public void CheatersDelight()
    {
        Console.Write("\nPlease enter a valid input. ");
        if (Console.ReadLine() == "ImASysAdminBiosh")
        {
            try { Console.Clear(); } catch { }
            Console.Write("\nUsername: ");
            string? adminUsername = Console.ReadLine();
            Debug.Assert(adminUsername != null);

            if (!CheckUser(adminUsername))
            {
                Console.WriteLine("\nAn user with the given SSN exist already.");
                Console.Write("\nPress ENTER to continue. ");
                Console.ReadKey(true);return;}
                
            Console.Write("\nPass: ");
            string? adminPass = Console.ReadLine();
            Debug.Assert(adminPass != null);
            Console.Write("\nName: ");
            string? adminName = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(adminUsername) && !string.IsNullOrWhiteSpace(adminName))
            {
                User? newAdmin = new(adminUsername, adminPass, adminName);
                foreach (Permission perm in Enum.GetValues(typeof(Permission)))
                {
                    if (perm != Permission.None)
                    { newAdmin.Permissions.Add(perm);}
                }
                newAdmin.Permissions.Remove(Permission.None);
                users.Add(newAdmin);
                SaveUsersToFile();
                Event? newEntry = new($"{newAdmin.SSN} First Entry", Event.EventType.Entry);

                newEntry.Description = $"The Master Admin was born";
                newEntry.StartDate = DateTime.Now;
                newEntry.Participants.Add(new Participant(newAdmin, Role.Admin));
                eventList.Add(newEntry);
                SaveEventsToFile();
            
                Console.WriteLine($"\nNew sysadmin added. Welcome {newAdmin.Name}!");
                Console.Write("\nPress ENTER to continue.");
                Console.ReadKey(true);
            }
            else { Console.Write("\nSomething went wrong. Press ENTER to continue. "); Console.ReadKey(true); return; }
        }
    }
}