
namespace App;

class Participant
{
    public User User;
    public Role UserRoles;

    public Participant(User user, Role userRoles)
    {
        User = user;
        UserRoles = userRoles;
    }
}