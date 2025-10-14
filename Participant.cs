
namespace App;

class Participant
{

    public User User;
    public Roles UserRoles;

    public Participant (User user, Roles userRoles)
    {
        User = user;
        UserRoles = userRoles;
    }
}