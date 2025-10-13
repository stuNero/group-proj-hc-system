namespace App;

interface IUser
{
    public bool TryLogin(string ssn, string password);
}