How to build and start the application
----
1. Rename the .env-example to .env and modify according to your needs
    -> important is at least the ip address of the server
2. Get ssl certificates and place them in a folder named ssl in the ClientApp (./ClientApp/ssl/my-site.crt && ./ClientApp/ssl/my-site.key) -> the file names must be exactly the same.
3. Add required photos to `./ClientApp/public/photos/` (see "Required Photos" section below)
4. run docker compose up --build

Required Photos
-----
The application requires the following photos to be placed in `./ClientApp/public/photos/`:
- `workshop.jpg` - Image for Workshop topic type
- `presentation.png` - Image for Vortrag (Presentation) topic type
- `sport.jpg` - Image for Sport topic type
- `discussion.jpg` - Image for Diskussion (Discussion) topic type
- `misc.jpg` - Image for Sonstiges (Other) topic type

These images are used in the topic type explanation cards. If any photos are missing, the corresponding cards will not display a background image.

Program Archives (Optional)
-----
To display previous years' programs on the home page, add SVG files to `./ClientApp/public/programs/`:
- `2024.svg`, `2025.svg`, etc.
- Files are automatically discovered at build-time and displayed sorted by year (newest first)
- Images lazy-load when scrolling to the bottom of the home page

Prepare running the app for the first time in dev mode
-----
- ```dotnet restore```
- ```dotnet build```
- ```cd Backend/App```
- ```mkdir data```
- ```touch data/database.db```
- ```dotnet ef database update``` This requires the dotnet-ef package to be installed:
- ```dotnet tool install --global dotnet-ef --version <your_dotnet_version>```
- ```cd Client && touch /schedule.svg```
- ```cd Backend/App && dotnet run```


How to add and update SupportTasks
-----
- Create admin user (registering as user with username as set in `.env` file)
- Obtain the auth-token e.g.:
```
curl -k -X POST https://localhost:8888/auth/login \
-H "Content-Type: application/json" \
-d '{
  "UserName": "",
  "password":""
}'
```
- Run the actual query e.g.:
```
curl -k POST https://localhost:8888/supporttask/addnew \
-H "Content-Type: application/json" 
-H "Authorization: Bearer <your bearer key obtained earlier>" 
-d '{              
  "Title": "Sample Task",
  "Description": "This is a sample task description",
  "Duration": "120",
  "RequiredSupporters": 5
}'
```

How to access the application
-----
The application will be accessible from https://<IP_ADDRESS>:<PROXY_PORT_SSL>, according to the values set in the .env
