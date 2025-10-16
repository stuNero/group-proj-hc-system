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
        Permissions = GetPermissions(userRole);
    }
    public bool TryLogin(string ssn, string password)
    {
        return ssn == SSN && password == _password;
    }
    public string GetUserPassword()
    {
        return _password;
    }



    public enum Permission
    {
        // Basic User Permissions

        RequestRegistration,
        // Patient Permissions
        ViewOwnJournal,
        RequestAppointment,
        // Personnel Permissions
        ViewPatientJournal,
        RegisterAppointments,
        // Admin Permissions
        HandleRegistrations,
        AddLocations,
        HandlePermissionSystem,
        // Super Admin Permissions
        AssignAdminsRegion,
        HandlePermissionSystemForAdmins,
    }



    public static Dictionary<Role, List<Permission>> rolePermissionDict = new()
    {
        {Role.SuperAdmin, new List<Permission>
            {
            Permission.HandlePermissionSystem,
            Permission.AssignAdminsRegion,
            Permission.HandleRegistrations,
            Permission.AddLocations,
            Permission.HandlePermissionSystemForAdmins

            }
        },
        {Role.Admin, new List<Permission>
            {
                Permission.HandleRegistrations,
                Permission.AddLocations
            }

        },
        {Role.Personnel, new List<Permission>
            {
                Permission.ViewPatientJournal,
                Permission.RegisterAppointments
            }
        },
        {Role.Patient, new List<Permission>
            {
                Permission.ViewOwnJournal,
                 Permission.RequestAppointment
            }
        },
        {Role.None, new List<Permission>
            {
                Permission.RequestRegistration
            }
        }
    };

    public static List<Permission> GetPermissions(Role role)
    {
        if (rolePermissionDict.ContainsKey(role))
        {
            return new List<Permission>(rolePermissionDict[role]);
        }
        return new List<Permission>();
    }

    public bool HasPermission(Permission requiredPermission)
    {
        return Permissions.Contains(requiredPermission);
    }
}