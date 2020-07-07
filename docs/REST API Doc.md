# Gardeninator REST API doc
---------------------------
Request settings:
URL base: localhost:4990/
Headers: Content-Type - application/json
Some listed requests have body examples below.
---------------------------
**Users**

__GET:__
- api/Users/Logout   - logs out current logged user
- api/Users/Current  - returns current logged in user
- api/Users/GetAll   - returns all users in database
- api/Users/Get/{id} - returns specific user by id INTEGER

__POST:__
- api/Users       - adds user to database 
	- ex.: {"Login": "test", "Password": "pass"}
- api/Users/Login - logs user in 
	- ex.: {"Login": "test", "Password": "pass"}
	
__PUT:__
- api/Users      - edits currently logged in user
	- ex.: {"Login": "test", "Password": "pass"}
- api/Users/{id} - edits specific user by id INTEGER
	- ex.: {"Login": "test", "Password": "pass"}
	
__DELETE:__
- api/Users      - deletes currently logged in user
- api/Users/{id} - deletes specific user by id
---------------------
**Projects**

__GET:__
- api/Projects/Current            - returns currently choosen project
- api/Projects/Unload             - unloads the project from session data
- api/Projects/GetAll             - returns all projects in database
- api/Projects/GetFromCurrentUser - returns all projects of logged in user
- api/Projects/GetBackgroundData  - returns current choosen project background base64 data
- api/Projects/GetByUser/{id}     - returns all projects of specific user (by id INTEGER)
- api/Projects/Get/{id}           - returns specific project (by id INTEGER)

__POST:__
- api/Projects      - add project to currently logged or specific user
	- ex.: {"Name": "test", "GridWidth": 10, "GridHeight": 10}
	- ex.: {"Name": "test", "GridWidth": 10, "GridHeight": 10, "UserId": 1}
- api/Projects/Load - load specific users project
	- ex.: {"ProjectId": 1}
	
__PUT:__
- api/Projects                  - changes currently choosen or specific project
	- ex.: {"Name": "test", "GridWidth": 10, "GridHeight": 10}
	- ex.: {"ProjectId": 1, "Name": "test", "GridWidth": 10, "GridHeight": 10, "UserId": 1}
- api/Projects/ChangeBackground - changes background image or creates if there is none to current choosen or specific project when user is logged in or specific project and specific user (foreign key has to be matched), background can be deleted by putting empty name on input
	- ex.: {"Name": "test", "ImgData": "base64 encoded data"}
	- ex.: {"ProjectId": 1, "Name": "test", "ImgData": "base64 encoded data"}
	- ex.: {"UserId": 1, "ProjectId": 1, "Name": "test", "ImgData": "base64 encoded data"}

__DELETE:__
- api/Projects      - deletes current choosen project
- api/Projects/{id} - deletes specific project
---------------------
**Areas**

__GET:__
- api/Areas/GetAll                 - returns all areas in database
- api/Areas/GetFromCurrentProject  - returns all areas of current project
- api/Areas/GetByProject/{id}      - returns all areas of specific project
- api/Areas/Get/{id}               - returns specific area (by id)

__POST:__
- api/Areas - adds area to currently choosen or specific project
	- ex.: {"Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10}
	- ex.: {"Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ParentAreaId": 1}
	- ex.: {"Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ProjectId": 1}
	- ex.: {"Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ParentAreaId": 1, "ProjectId": 1}
	
__PUT:__
- api/Areas  - changes specific area
	- ex.: {"AreaId": 1, "Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10}
	- ex.: {"AreaId": 1, "Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ParentAreaId": 1}
	- ex.: {"AreaId": 1, "Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ProjectId": 1}
	- ex.: {"AreaId": 1, "Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ParentAreaId": 1, "ProjectId": 1}
	
__DELETE:__
- api/Areas/{id} - deletes specific area
---------------------
**Instances**

__GET:__
- api/Instances/GetAll       - returns all instances in database
- api/Instances/GetByProject - returns all instances of project - current or specific
	- ex.: {"CategoryId": 1}
	- ex.: {"ObjectTypeId": 1}
	- ex.: {"ProjectId": 1}
	- ex.: {"ProjectId": 1, "CategoryId": 1}
	- ex.: {"ProjectId": 1, "ObjectTypeId": 1}
- api/Instances/GetByArea    - returns all instances of specific area
	- ex.: {"AreaId": 1}
	- ex.: {"AreaId": 1, "CategoryId": 1}
	- ex.: {"AreaId": 1, "ObjectTypeId": 1}
- api/Instances/Get/{id}     - returns specific instance (by id INTEGER)

__POST:__
- api/Instances - add instance to specific area
	- ex.: {"X": 0, "Y": 0, "ObjectId": 1, "AreaId": 1}
	
__PUT:__
- api/Instances - modifies specific instance
	- ex.: {"InstanceId": 1, "X": 0, "Y": 0, "ObjectId": 1, "AreaId": 1}
	
__DELETE:__
- api/Instances/{id} - deletes specific instance by Id INTEGER
---------------------
**Categories**

__GET:__
- api/Categories/GetAll   - returns all categories in database
- api/Categories/Get/{id} - returns specific category by id INTEGER

__POST:__
- api/Categories - adds category to database
	- ex.: {"Name": "test"}
	
__PUT:__
- api/Categories - adds category to database
	- ex.: {"CategoryId":1, "Name": "test"}
	
__DELETE:__
- api/Categories/{id} - removes specific category by id INTEGER
---------------------
**Objects**

__GET:__
- api/Objects/GetAll             - returns all objects in database
- api/Objects/Get/{id}           - returns specific object (by id INTEGER)
- api/Objects/GetImage/{id}      - returns image (base64 encoded data) from specific object (by id INTEGER)
- api/Objects/GetByCategory/{id} - returns all objects by specific category (by id INTEGER)

__POST:__
- api/Objects - add object to specific category
	- ex.: {"Name": "test", "Width": 3, "Height": 3, "CategoryId", 1}
		
__PUT:__
- api/Objects             - modifies object of specific category
	- ex.: {"ObjectId": 1, "Name": "test", "Width": 3, "Height": 3, "CategoryId", 1}
- api/Objects/ChangeImage - changes object image or creates if there is none
	- ex.: {"ObjectId": 1, "Name": "test.jpg", "ImgData": "base64 encoded data"}
	- ex.: {"ObjectId": 1, "Name": "", "ImgData": ""}

__DELETE:__
- api/Objects/{id} - removes specific object by id INTEGER and instances of this object