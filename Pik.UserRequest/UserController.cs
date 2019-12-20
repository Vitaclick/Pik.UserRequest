using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pik.UserRequest
{
  public class UserController
  {
    static readonly HttpClient client = new HttpClient();
    const string AD_URI_API = "http://vpp-bor:5000/v1/Employee/byLogin?login=";

    public async Task<User> RequestAsync(User user)
    {
      try
      {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.GetAsync(AD_URI_API + $"{user.AccountName}");
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        using var jsonDoc = JsonDocument.Parse(responseBody);

        try
        {
          var jsonData = jsonDoc.RootElement.EnumerateArray().FirstOrDefault();

          user.isFired = jsonData.GetProperty("isFired").GetBoolean();
          user.position = jsonData.GetProperty("position").GetString();
          user.employeeType = jsonData.GetProperty("employeeType").GetString();

          var userData = jsonData.GetProperty("user");
          user.fio = userData.GetProperty("fio").GetString();

          var departmentData = jsonData.GetProperty("department");
          user.department = departmentData.GetProperty("name").GetString();

          user.birthDate = userData.GetProperty("birthDate").GetDateTime();
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
        //var options = new JsonSerializerOptions()
        //{
        //  IgnoreNullValues = true,
        //  WriteIndented = true
        //};
        //return await  JsonSerializer.DeserializeAsync<User>(responseBody, options);
        return user;
      }
      catch (HttpRequestException e)
      {
        Console.WriteLine(user.AccountName + "\n" + e.Message);
        return user;
      }
    }
  }
}