namespace App;


enum Permission
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