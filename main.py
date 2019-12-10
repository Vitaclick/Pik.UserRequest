import requests
import json
import csv

requestApi = "http://vpp-bor:5000/v1/Employee/byLogin"

with open("./users.txt", "r") as userListFile:
  users = userListFile.read().splitlines()

  for i, user in enumerate(users):
    response = requests.get(requestApi, params={"login": user})
    if response.status_code == 200:
      jsonData = response.json()
      if len(jsonData) > 0:
        userData = jsonData[0]
        if userData["department"] != None:
          userDepartment = userData['department']["name"]
          if len(userDepartment) > 2:
            if not "R&D" in userDepartment:
              users[i] = [users[i] + "@pik.ru", users[i], userDepartment]

csv.register_dialect('myDialect',
delimiter = ',',
quotechar = '"',
quoting = csv.QUOTE_ALL)

with open("./result.csv", "w", newline="") as resultFile:
  wr = csv.writer(resultFile, dialect="myDialect")
  # for r in users:
  wr.writerows(users)