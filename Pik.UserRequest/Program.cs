using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Server.Lib.Databases;

namespace Pik.UserRequest
{
  class Program
  {
    const string MAIN_SHEET_NAME = "UserData";
    const string VDI_SHEET_NAME = "Реестр";
    static UserController _controller = new UserController();
    static Dictionary<string, IList<CellData>> vdiData;

    static async Task Main(string[] args)
    {
      // Data connection
      var assembly = Assembly.GetExecutingAssembly();
      await using var mainStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.serviceAccount.json");
      await using var vdiStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.credentials.json");

      var mainConnector = new SheetConnector("UserRequest", "16Vcj0sVR8pwLd5hE294ylNP4XhV83gP0uDpt1D8efLc");
      var vdiConnector = new SheetConnector("UserRequest", "1n3I5HI1dZ6mYDY2tiSJmUCQz5ENPx9tm6sBK602bC5w");

      if (mainConnector.SetSheetApiConnection(mainStream) && vdiConnector.SetSheetUserConnection(vdiStream))
      {
        // Data collection
        var mainUsersData = mainConnector.ReadSheetData(MAIN_SHEET_NAME, "A:A")
          .Select(kvp => kvp.Value)
          .Select(x => x.FirstOrDefault()?.FormattedValue)
          .Where(u => u != null)
          .ToList();

        var vdiValues = vdiConnector.ReadSheetData(VDI_SHEET_NAME, "E:E")
          .Select(kvp => kvp.Value).ToList();

        var vdiLogins = vdiConnector.ReadSheetData(VDI_SHEET_NAME, "O:O")
            .Select(kvp => kvp.Value.FirstOrDefault())
            .ToList();

        vdiData = vdiLogins.Zip(vdiValues, (k, v) => new { Key = k, Value = v })
          .Where(x => x.Key.FormattedValue != null)
          .ToDictionary(x => x.Key.FormattedValue, x => x.Value);

        // Data parsing
        var output = await ParseUsersAsync(mainUsersData);

        // Output data
        mainConnector.WriteData(MAIN_SHEET_NAME, "B:Z", output);
      }
      else
      {
        Console.WriteLine(@"Cannot connect to either Main nor VDI data table.");
      }
    }

    private static async Task<List<IList<object>>> ParseUsersAsync(List<string> users)
    {
      var output = new List<IList<object>>();
      foreach (var user in users.Select(userName => new User(userName)))
      {
        var userData = new List<object>();
        await _controller.RequestAsync(user);

        if (user.employeeType?.ToLower() == "remote" || user.employeeType?.ToLower() == "outstuff"
                                                     || user.department?.ToLower() == "аутстафф")
        {
          if (vdiData.TryGetValue(user.AccountName, out var userVdiData))
          {
            user.department = userVdiData.FirstOrDefault()?.FormattedValue;
          }
        }
        userData.AddRange(new List<object>{ user.fio, user.email, user.position, user.department, user.employeeType, user.isFired, user.birthDate });

        output.Add(userData);
      }

      return output;
    }
  }
}
