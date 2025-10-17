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
            for (int i = 0; i < user.Permissions.Count; i++)
            {
                if (user.Permissions[i] == user.Permissions[0])
                {
                    userPermissionLine = $"{user.Permissions[i]}";
                }
                else
                {
                    userPermissionLine += $"^{user.Permissions[i]}";
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
            Location newEventLocation = null;



            Event? newEvent = new(newEventTitle, eventType);
            newEvent.Description = newEventDescription;
            newEvent.StartDate = newEventStartDate;
            newEvent.EndDate = newEventEndDate;

            List<Participant> participantsList = new();

            string[] participants = eventSplitData[5].Split("^");
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

                    if (events.Participants[i] == events.Participants[0])
                    {
                        participantLines = $"{events.Participants[i].User.SSN}¤{events.Participants[i].ParticipantRole}";
                    }
                    else
                    {
                        participantLines += $"^{events.Participants[i].User.SSN}¤{events.Participants[i].ParticipantRole}";
                    }
                }
            }

            eventLines += $"{events.Title}~{events.MyEventType}~{events.Description}~{events.StartDate}~{events.EndDate}~{participantLines}";
            eventLines += Environment.NewLine;
        }
        File.WriteAllText(eventsFile, eventLines);
    }
}