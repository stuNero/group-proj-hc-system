namespace App;

class Event
{
  public string Title;
  public EventType MyEventType;
  public string? Description;
  public DateTime StartDate;
  public DateTime EndDate;
  public Location? Location;

  public List<Participant> Participants = new List<Participant>();

  public Event(string title, EventType myEventType)
  {
    Title = title;
    MyEventType = myEventType;
  }

  public enum EventType
  {
    None,
    Request,
    Appointment,
    Entry,
  }

}
