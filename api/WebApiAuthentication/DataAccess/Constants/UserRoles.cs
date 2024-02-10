namespace WebApiAuthentication.DataAccess.Constants
{
	public static class USER_ROLES
	{
		public const string ADMIN = "ADMIN";
		public const string MANAGER = "MANAGER";
		public const string ARTIST = "ARTIST";

		public static HashSet<string> GetAllRoles()
		{
			return new HashSet<string>(new[] { ADMIN, MANAGER, ARTIST }, StringComparer.OrdinalIgnoreCase);
		}
	}

}
//HashSet Contents: The HashSet will contain three elements:
//"ADMIN", "MANAGER", and "ARTIST". These are the values of
//the constants ADMIN, MANAGER, and ARTIST from the USER_ROLES class.

//Case - Insensitive Comparisons:
//Due to the use of StringComparer.OrdinalIgnoreCase,
//any string comparison done against the contents of this HashSet
//(such as checks to see if the HashSet contains a specific string)
//will be case-insensitive.
//This means that searching for "admin", "Admin", or "ADMIN" in the HashSet
//will successfully find the "ADMIN" entry, and similarly for the other roles.