namespace App;

class Location
{
  public string Name;
  public string Address;
  public Region Region;
  public Location(string name, string address, Region region = Region.None)
  {
    Name = name;
    Address = address;
    Region = region;
  }
}