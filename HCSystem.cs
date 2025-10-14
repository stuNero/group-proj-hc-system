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
        if (!File.Exists(usersFile))
        {
            Directory.CreateDirectory("csv-files");
            File.WriteAllText(usersFile, "");
        }

    }
    public void SaveUsers()
    {

    }
}