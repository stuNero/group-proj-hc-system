using System.Dynamic;

namespace App;

class User
{
    public string SSN;
    string _password;
    public string Name;
    public Role UserRole;
    public User(string ssn, string password, string name, Role userRole = Role.None)
    {
        SSN = ssn;
        _password = password;
        Name = name;
        UserRole = userRole;
    }
    public bool TryLogin(string ssn, string password)
    {
        return ssn == SSN && password == _password;
    }
    public string GetUserPassword()
    {
        return _password;
    }

    public void GetPermission(Role role)
    {
        if (role == Role.SuperAdmin)
        {
            SuperAdmin SuperAdminPermissions = new();
        }
        else if (role == Role.Admin)
        {
            Admin AdminPermissions = new();
        }
        else if (role == Role.Personnel)
        {
            Personal PersonalPermissions = new();
        }
        else
        {
            Console.WriteLine($"\nUser has no system permissions.");
        }
    }



    enum SuperAdmin
    {
        PermToGiveAdminPerm,
    }

    enum Admin
    {
        PermToRegPatient,
        PermToGivePersPerm,
    }

    enum Personal
    {
        PermToViewJournal,
        PermToRegAppointment,
        PermToModAppointment,
    }
}