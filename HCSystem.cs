using System.Diagnostics.Tracing;

namespace App;

class HCSystem
{
    // SYSTEM LISTS
    public List<Event> eventList = new();
    public List<User> users = new();
    // PUT ALL DIRECTORIES HERE
    string usersFile = @"csv-files\users-list.csv";
    string eventsFile = @"csv-files\events-list.csv";

    public HCSystem()
    {

        if (!Directory.Exists("csv-files"))
        { Directory.CreateDirectory("csv-files"); }

        if (!File.Exists(usersFile))
        { File.WriteAllText(usersFile, ""); }

        if (!File.Exists(eventsFile))
        { File.WriteAllText(eventsFile, ""); }

        LoadUsersFromFile();
        LoadEventsFromFile();

    }
    public void LoadUsersFromFile()
    {
        string[] usersCsv = File.ReadAllLines(usersFile);
        foreach (string userLine in usersCsv)
        {
            string[] userSplitData = userLine.Split("~");
            Role userRole = Role.None;
            switch (userSplitData[3])
            {
                case "Admin": userRole = Role.Admin; break;
                case "Patient": userRole = Role.Patient; break;
                case "Personnel": userRole = Role.Personnel; break;
            }

            users.Add(new(userSplitData[0], userSplitData[1], userSplitData[2], userRole));
        }
    }
    public void SaveUsersToFile()
    {
        string userLines = "";
        foreach (User user in users)
        {
            userLines += $"{user.SSN}~{user.GetUserPassword()}~{user.Name}~{user.UserRole}";
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
            Event.EventType eventType = Event.EventType.Request;
            string newEventDescription = eventSplitData[2];
            DateTime newEventStartDate = DateTime.Parse(eventSplitData[3]);
            DateTime newEventEndDate = DateTime.Parse(eventSplitData[4]);

            switch (eventSplitData[1])
            {
                case "Request": eventType = Event.EventType.Request; break;
                case "Enty": eventType = Event.EventType.Entry; break;
                case "Appointment": eventType = Event.EventType.Appointment; break;
            }

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

    public bool CreatePersonnelAccount(string ssn, string password, string name)
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
        User newPersonnel = new(ssn, password, name, Role.Personnel);
        users.Add(newPersonnel);
        SaveUsersToFile();
        return true;
    }
}