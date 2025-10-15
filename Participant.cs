
namespace App;

class Participant
{
    public User User;
    public Role ParticipantRole;

    public Participant(User user, Role userRoles)
    {
        User = user;
        ParticipantRole = userRoles;
    }
}