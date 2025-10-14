namespace App;

class Event
{
  public string Title;
  public string Description;

  public DateTime StartDate;
  public DateTime EndDate;
  public EventType MyEventType;

  public List<Participant> Participants = new List<Participant>();

  public Event(string title, EventType myEventType)
  {
    Title = title;
    MyEventType = myEventType;
  }

  public enum EventType
  {
    Request,
    Appoitment,
    Entrie

  }

}
