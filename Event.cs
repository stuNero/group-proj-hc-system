namespace App;

class Event
{
  public string Title;
  public string Description;

  public DateTime StartDate;
  public DateTime EndDate;

  public List<User> Users = new List<User>();

  public Event(string title)
  {
    Title = title;
  }

}
