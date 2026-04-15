using Foody.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Foody
{
    public class Tests
    {
        //private RestClient _client;
        private RestClient client;
        private static string foodId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("irina333", "123456");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:81/")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client = new RestClient(options);
        }
        private string GetJwtToken(string username, string password)
        {
            RestClient client = new RestClient("http://144.91.123.158:81/");
            RestRequest request = new RestRequest("api/User/Authentication", Method.Post);  
            request.AddJsonBody(new { username, password });      
           RestResponse response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
               var content =JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrEmpty(token))
               {
                  throw new InvalidOperationException("Token is null or empty.");
                }   
                return token;
            }
           else
            {
                throw new InvalidOperationException($"Authentication failed with status code: {response.StatusCode}");
            }
        }
        [Order(1)]
        [Test]
        public void CreateFoody_WithRequiredFields_ShouldReturnSuccess()
        {
            FoodDTO food = new FoodDTO
            {
                Name = "Pizza",
                Description = "Delicious cheese pizza",
                Url = ""
            };

            RestRequest request = new RestRequest("api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            ApiResponseDTO content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

             Assert.That(content.FoodId,Is.Not.Null.And.Not.Empty);

               foodId = content.FoodId;
        }
        [Order(2)]
        [Test]
        public void Edit_TitleOfCreatedFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"api/Food/Edit/{foodId}", Method.Patch);
          
            request.AddJsonBody(new []
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Updated Pizza"
                }
            });

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse, Is.Not.Null);
            Assert.That(editResponse!.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void GetAllFoods_ShouldListAllFoods()
        {
            var request = new RestRequest("api/Food/All", Method.Get);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var foods = JsonSerializer.Deserialize<List<FoodDTO>>(response.Content!);
            Assert.That(foods, Is.Not.Null);
            Assert.That(foods, Is.Not.Empty);   
            Assert.That(foods.Count, Is.GreaterThan(0));
        }

        [Order(4)]
        [Test]
        public void Delete_CreatedFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"api/Food/Delete/{foodId}", Method.Delete);
            var response = this.client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteResponse, Is.Not.Null);
            Assert.That(deleteResponse!.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var foodCreated = new FoodDTO
            {
                Name = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("api/Food/Create", Method.Post);
            request.AddJsonBody(foodCreated);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Order(6)]
        [Test]
        public void Edit_NonExistentFood_ShouldReturnNotFound()
        {
            var nonExistentFoodId = "non-existent-id";
            var request = new RestRequest($"api/Food/Edit/{nonExistentFoodId}", Method.Patch);

            request.AddJsonBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Updated Pizza"
                }
            });

            var response = this.client.Execute(request);
            ApiResponseDTO content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(content, Is.Not.Null);
            Assert.That(content!.Msg, Is.EqualTo("No food revues..."));
        }
        //Adding a comment 

        [Order(7)]
        [Test]
        public void Delete_NonExistentFood_ShouldReturnNotFound()
        {
            var nonExistentFoodId = "non-existent-id";
            var request = new RestRequest($"api/Food/Delete/{nonExistentFoodId}", Method.Delete);
            var response = this.client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deleteResponse, Is.Not.Null);
            Assert.That(deleteResponse!.Msg, Is.EqualTo("Unable to delete this food revue!"));
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
           
        }
    }
}