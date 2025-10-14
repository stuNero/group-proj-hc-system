namespace App;

class Admin
{
  public string Email;
  string _password;
  public string Name;

  public Admin(string email, string password, string name)
  {
    Email = email;
    _password = password;
    Name = name;
  }

  public bool TryLogin(string email, string password)
  {
    return email == Email && password == _password;
  }
}