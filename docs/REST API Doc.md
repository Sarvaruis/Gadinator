# Gardeninator REST API doc
---------------------
**api/Users**
__GET:__
- api/Users/Logout - logs out current logged user
- api/Users/Current - returns current logged in user
- api/Users/GetAll - returns all users in database
- api/Users/Get/{id} - returns specific user by id INTEGER
__POST:__
- api/Users - adds user to database 
	- ex.: {"Login": "test", "Password": "pass"}
- api/Users/Login - logs user in 
	- ex.: {"Login": "test", "Password": "pass"}
__PUT:__
- api/Users - edits currently logged in user
	- ex.: {"Login": "test", "Password": "pass"}
- api/Users/{id} - edits specific user by id INTEGER
	- ex.: {"Login": "test", "Password": "pass"}
__DELETE:__
- api/Users      - deletes currently logged in user
- api/Users/{id} - deletes specific user by id
---------------------
**api/Projects**




