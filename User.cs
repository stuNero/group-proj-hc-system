namespace App;

class User
{
    public string SSN;
    string _password;
    public string Name;
    public Role UserRoles
    public User(string ssn, string password, string name, Role userRoles)
    {
        SSN = ssn;
        _password = password;
        Name = name;
        UserRoles = userRoles;
    }
    public bool TryLogin(string ssn, string password)
    {
        return ssn == SSN && password == _password;
    }
    public string GetUserPassword()
    {
        return _password;
    }
}