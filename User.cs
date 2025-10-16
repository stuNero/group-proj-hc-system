using System.Dynamic;
using System.Security;

namespace App;

class User
{
    public string SSN;
    string _password;
    public string Name;
    public List<Permission> Permissions = new();
    public Role UserRole;
    public User(string ssn, string password, string name, Role userRole = Role.None)
    {
        SSN = ssn;
        _password = password;
        Name = name;
        UserRole = userRole;
        Permissions = HCSystem.rolePermissionDict[userRole];
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