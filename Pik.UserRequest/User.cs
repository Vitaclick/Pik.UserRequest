using System;

namespace Pik.UserRequest
{
  public class User
  {
    public string AccountName { get; set; }
    public string fio { get; set; }
    public string email => AccountName + "@pik.ru";
    public string department { get; set; }
    public string position { get; set; }
    public bool isFired { get; set; }
    public string employeeType { get; set; }
    public DateTime birthDate { get; set; }
    public User(string rawUsername)
    {
      AccountName = ExtractAccountName(rawUsername);
    }
    private string ExtractAccountName(string user)
    {

      var cleanedName = user
        .ToLower()
        .Trim(' ', '.')
        .Split(" ")[0]
        .Replace(@"main\", "")
        .Replace("@pik.ru", "");
      return cleanedName;

    }
  }
}