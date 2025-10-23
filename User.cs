using System.Dynamic;
using System.Security;

namespace App;

class User
{
    public string SSN;
    string _password;
    public string Name;
    public Region UserRegion;
    public List<Permission> Permissions = new();
    public User(string ssn, string password, string name, Region userRegion = Region.None)
    {
        SSN = ssn;
        _password = password;
        Name = name;
        UserRegion = userRegion;
        Permissions.Add(Permission.None);
    }
    public bool TryLogin(string ssn, string password)
    {
        return ssn == SSN && password == _password;
    }
    public string GetUserPassword()
    {
        return _password;
    }

    public bool HasPermission(Permission requiredPermission)
    {
        return Permissions.Contains(requiredPermission);
    }
}