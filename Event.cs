namespace App;

class Event
{
  public string Title;
  public string Description;

  public DateTime StartDate;
  public DateTime EndDate;
  public EventType MyEventType;

  public List<Participant> Participant = new List<Participant>();

  public Event(string title)
  {
    Title = title;
  }

  public enum EventType
  {
    Request,
    Appointment,
    Entry

  }

}
