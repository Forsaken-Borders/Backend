using System.Net;
using System.Net.Http.Json;
using System.Text;
using ForSakenBorders.Backend.Api.v1.Payloads;
using Xunit;

namespace ForSakenBorders.Backend.Tests.Api.v1
{
    public class UsersControllerTest
    {
        private readonly HttpClient _client = new();
        private static bool hasRan;

        public UsersControllerTest()
        {
            _client.BaseAddress = new(new("http://localhost:5000/"));
            if (!hasRan) // The constructor is called everytime a new test is ran. I'd rather wait 3 seconds instead of some 21
            {
                Environment.CurrentDirectory = "../../../../ForSakenBorders.Backend/";
                Task.Run(() => Program.Main(new[] { "--dev=true", "--logging:disabled=true" }));
                Task.Delay(3000).Wait();
                hasRan = true;
            }
        }

        public static UserPayload CreateExampleUser(string email) => new()
        {
            Email = email,
            Password = "password",
            Username = "example",
            FirstName = "Example",
            LastName = "User"
        };

        #region GetMethods
        [Fact]
        public async Task Get_MissingUser()
        {
            UserPayload userPayload = CreateExampleUser("Get_MissingUser@example.com");
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await response.Content.ReadAsStringAsync()).Replace("\"", "");

            HttpRequestMessage request = new(HttpMethod.Get, $"/api/v1/users/" + Guid.Empty);
            request.Headers.Add("Authorization", token);

            HttpResponseMessage apiResponse = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.NotFound == apiResponse.StatusCode, $"/api/v1/users/{Guid.Empty} did not throw a 404 NotFound. Instead it threw: {(int)apiResponse.StatusCode} {apiResponse.StatusCode}, {await apiResponse.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Get_ValidUser()
        {
            UserPayload userPayload = CreateExampleUser("Get_ValidUser@example.com");
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await response.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = response.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Get, $"/api/v1/users/" + userId);
            request.Headers.Add("Authorization", token);

            HttpResponseMessage apiResponse = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.OK == apiResponse.StatusCode, $"/api/v1/users/{userId} did not return 200 OK. Instead it threw: {(int)apiResponse.StatusCode} {apiResponse.StatusCode}, {await apiResponse.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Get_Unauthorized()
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"/api/v1/users/" + Guid.Empty);
            HttpResponseMessage apiResponse = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.Unauthorized == apiResponse.StatusCode, $"/api/v1/users/{Guid.Empty} did not return 401 Unauthorized. Instead it threw: {(int)apiResponse.StatusCode} {apiResponse.StatusCode}, {await apiResponse.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Get_DeletedUser()
        {
            UserPayload userPayload = CreateExampleUser("Get_DeletedUser@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");

            HttpRequestMessage request = new(HttpMethod.Delete, $"/api/v1/users/" + userId);
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"/api/v1/users/ did not return 200 OK. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");

            userPayload = CreateExampleUser("Get_DeletedUser2@example.com");
            createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token2 = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");

            request = new(HttpMethod.Get, $"/api/v1/users/" + userId);
            request.Headers.Add("Authorization", token2);

            response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.Gone == response.StatusCode, $"/api/v1/users/ did not return 410 Gone. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }
        #endregion GetMethods

        #region PostMethods
        [Fact]
        public async Task Post_NullPayload()
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", "");
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Post_InvalidEmail()
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", CreateExampleUser("Post_InvalidEmail @example.com"));
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Post_InvalidUsernameWhitespace()
        {
            UserPayload userPayload = CreateExampleUser("Post_InvalidUsernameWhitespace@example.com");
            userPayload.Username = "   ";

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Post_InvalidUsernameTooManyChars()
        {
            UserPayload userPayload = CreateExampleUser("Post_InvalidUsernameTooManyChars@example.com");
            userPayload.Username = "usernameWithALengthOf33CharsExact";

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Post_InvalidUsernameTooFewChars()
        {
            UserPayload userPayload = CreateExampleUser("Post_InvalidUsernameTooFewChars@example.com");
            userPayload.Username = "hi";

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Post_InvalidFirstName()
        {
            UserPayload userPayload = CreateExampleUser("Post_InvalidFirstName@example.com");
            userPayload.Username = "aFirstNameWhichIs33CharactersWide";

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Post_InvalidLastName()
        {
            UserPayload userPayload = CreateExampleUser("Post_InvalidLastName@example.com");
            userPayload.LastName = "someLastNameThatIs33CharsInLength";

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Post_WithConflictingEmails()
        {
            UserPayload userPayload = CreateExampleUser("Post_WithConflictingEmails@example.com");

            // Post with valid data.
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            Assert.True(HttpStatusCode.Created == response.StatusCode, $"/api/v1/users/ did not return 201 Created. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");

            // Try reusing the same email.
            response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            Assert.True(HttpStatusCode.Conflict == response.StatusCode, $"/api/v1/users/ did not return 409 Conflict. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }
        #endregion PostMethods

        #region PutMethods
        [Fact]
        public async Task Put_Unauthenticated()
        {
            UserPayload userPayload = CreateExampleUser("Put_Unauthenticated@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpResponseMessage putUserResponse = await _client.PutAsJsonAsync($"/api/v1/users/{userId}", userPayload);
            Assert.True(HttpStatusCode.Unauthorized == putUserResponse.StatusCode, $"/api/v1/users/{userId} did not return 401 Unauthorized. Instead it threw: {(int)putUserResponse.StatusCode} {putUserResponse.StatusCode}, {await putUserResponse.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Put_NullPayload()
        {
            UserPayload userPayload = CreateExampleUser("Put_NullPayload@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            request.Headers.Add("Authorization", token);
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }


        [Fact]
        public async Task Put_InvalidEmail()
        {
            UserPayload userPayload = CreateExampleUser("Put_InvalidEmail@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            userPayload.Email = "Put_InvalidEmail @example.com";
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Put_InvalidFirstName()
        {
            UserPayload userPayload = CreateExampleUser("Put_InvalidFirstName@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            userPayload.FirstName = "aFirstNameWhichIs33CharactersWide";
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Put_InvalidLastName()
        {
            UserPayload userPayload = CreateExampleUser("Put_InvalidLastName@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            userPayload.FirstName = "someLastNameThatIs33CharsInLength";
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Put_UsernameWhitespace()
        {
            UserPayload userPayload = CreateExampleUser("Put_UsernameWhitespace@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            userPayload.Username = "   ";
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Put_UsernameTooManyChars()
        {
            UserPayload userPayload = CreateExampleUser("Put_UsernameTooManyChars@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            userPayload.Username = "usernameWithALengthOf33CharsExact";
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Put_UsernameTooFewChars()
        {
            UserPayload userPayload = CreateExampleUser("Put_UsernameTooFewChars@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            userPayload.Username = "hi";
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"/api/v1/users/ did not return 400 BadRequest. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Put_ConflictingEmails()
        {
            UserPayload userPayload = CreateExampleUser("Put_ConflictingEmails@example.com");
            HttpResponseMessage createUserResponse1 = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse1.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse1.Headers.Location.ToString().Split('/').Last();

            userPayload.Email = "Put_ConflictingEmails2@example.com";
            await _client.PostAsJsonAsync("/api/v1/users", userPayload);

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.Conflict == response.StatusCode, $"/api/v1/users/ did not return 409 Conflict. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Put_NoChange()
        {
            UserPayload userPayload = CreateExampleUser("Put_NoChange@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpRequestMessage request = new(HttpMethod.Put, $"/api/v1/users/" + userId);
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.NotModified == response.StatusCode, $"/api/v1/users/ did not return 304 NotModified. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }
        #endregion PutMethods

        #region DeleteMethods
        [Fact]
        public async Task Delete_Unauthenticated()
        {
            UserPayload userPayload = CreateExampleUser("Delete_Unauthenticated@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();

            HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/users/{userId}");
            Assert.True(HttpStatusCode.Unauthorized == response.StatusCode, $"/api/v1/users/ did not return 401 Unauthorized. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Delete_MissingUser()
        {
            UserPayload userPayload = CreateExampleUser("Delete_MissingUser@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");

            HttpRequestMessage request = new(HttpMethod.Delete, $"/api/v1/users/" + Guid.Empty);
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.NotFound == response.StatusCode, $"/api/v1/users/ did not return 404 NotFound. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Delete_ValidUser()
        {
            UserPayload userPayload = CreateExampleUser("Delete_ValidUser@example.com");
            HttpResponseMessage createUserResponse = await _client.PostAsJsonAsync("/api/v1/users", userPayload);
            string userId = createUserResponse.Headers.Location.ToString().Split('/').Last();
            string token = (await createUserResponse.Content.ReadAsStringAsync()).Replace("\"", "");

            HttpRequestMessage request = new(HttpMethod.Delete, $"/api/v1/users/" + userId);
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(userPayload);

            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(HttpStatusCode.OK == response.StatusCode, $"/api/v1/users/ did not return 200 OK. Instead it threw: {(int)response.StatusCode} {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }
        #endregion DeleteMethods
    }
}