namespace App;

class HCSystem
{
    // SYSTEM LISTS
    public List<Event> eventList = new();
    public List<User> users = new();
    // PUT ALL DIRECTORIES HERE
    string usersFile = @"csv-files\users-list.csv";

    public HCSystem()
    {
        users.Add(new User("testssn1", "", "test1"));
        users.Add(new User("testssn2", "", "test2"));

        if (!Directory.Exists("csv-files"))
        { Directory.CreateDirectory("csv-files"); }

        if (!File.Exists(usersFile))
        { File.WriteAllText(usersFile, ""); }

        SaveUsersToFile();
        LoadUsersFromFile();
    }
    public void LoadUsersFromFile()
    {
        string[] usersCsv = File.ReadAllLines(usersFile);
        foreach (string userLine in usersCsv)
        {
        string[] userSplitData = userLine.Split("~");
        users.Add(new(userSplitData[0], userSplitData[1], userSplitData[2]));
        }
    }
    public void SaveUsersToFile()
    {
        string userLines = "";
        foreach (User user in users)
        {
            userLines += $"{user.SSN}~{user.GetUserPassword()}~{user.Name}";
            userLines += Environment.NewLine;
        }
        File.WriteAllText(usersFile, userLines);
    }
}