using Eventmi.Core.Models.Event;
using Eventmi.Infrastructure.Data.Contexts;
using Eventmi.Infrastructure.Migrations;
using Eventmi.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal.Execution;
using RestSharp;
using Event = Eventmi.Infrastructure.Models.Event;

namespace EventMe.Tests
{
    public class Tests
    {
        private RestClient _client;
        private readonly string _baseUrl = "https://localhost:7236";
        [SetUp]
        public void Setup()
        {
            _client = new RestClient(_baseUrl);
        }

        [Test]
        public async Task GetAllEvents_ReturnsSuccessStatusCode()
        {
            //Arange
            var request = new RestRequest("/Event/All", Method.Get);
            //Act
            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Add_GetRequest_ReturnsAddVew()

        {
            //Arange
            var request = new RestRequest("/Event/Add", Method.Get);
            //Act
            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));


        }

        [Test]
        public async Task Add_PostRequest_AddsNewEventAndRedirects ()
        {
            //Arange
            var input = new EventFormModel
            {
                Name = "CSKA Conf",
                Place = "Klub Burgas",
                Start = new DateTime(2024, 12, 12, 12, 0, 0),
                End = new DateTime(2024, 12, 12, 14, 0, 0)
            };
            var request = new RestRequest("/Event/Add", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("Name", input.Name);
            request.AddParameter("Place", input.Place);
            request.AddParameter("Start",input.Start.ToString("MM/dd/yyyy hh: mm tt"));
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh: mm tt"));
            //Act
            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.True(CheckIfEventExist(input.Name));

        }

        [Test]
        public async Task Details_GetRequest_ShouldReturnDetailedVew()
        {
            //Arrange
            var eventId = 1;
            var request = new RestRequest($"/Event/Details/{eventId}", Method.Get);

            //Act
            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Edit_GetRequest_ShouldReturnEditVew()
        {
            //Arrange
            var eventId = 1;
            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Get);

            //Act
            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Details_GetRequest_ShouldReturnNotFoundIfNotGiven()
        {
            //Arrange
            var eventId = 1;
            var request = new RestRequest($"/Event/Details/", Method.Get);

            //Act
            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Edit_PostRequest_ShouldEditAnEvent()
        {
            var eventId = 1;
            var dbEvent = GetEventById(eventId);
            var input = new EventFormModel()
            {
              Id = dbEvent.Id,
              End = dbEvent.End,
              Name = $"{dbEvent.Name} UPDATED!!!",
              Place = dbEvent.Place,
              Start = dbEvent.Start,
            };
            var request = new RestRequest($"/Event/Edit/{dbEvent.Id}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("Id", input.Id);
            request.AddParameter("Name", input.Name);
            request.AddParameter("Place", input.Place);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh: mm tt"));
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh: mm tt"));

            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        }

        [Test]
        public async Task Edit_WithIdMismatch_ShouldReturnNotFound()
        {  var eventId = 3;
            var dbEvent = GetEventById(eventId);
            var input = new EventFormModel()
            {
                Id = 445,
                End = dbEvent.End,
                Name = $"{dbEvent.Name} UPDATED!!!",
                Place = dbEvent.Place,
                Start = dbEvent.Start,
            };
            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("Id", input.Id);
            request.AddParameter("Name", input.Name);
            request.AddParameter("Place", input.Place);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh: mm tt"));
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh: mm tt"));

            //act
            var response = await _client.ExecuteAsync(request);
            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Edit_PostRequest_ShouldReturnBackSameVewModelErrorsArePresent()
        {
            var eventId = 1;
            var dbEvent = GetEventById(eventId);
            var input = new EventFormModel()
            {
                Id = dbEvent.Id,
                Place = dbEvent.Place,
              
            };
            var request = new RestRequest($"/Event/Edit/{dbEvent.Id}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("Id", input.Id);
            request.AddParameter("Name", input.Name);
            

            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        }

        [Test]
        public async Task DeleteAction_WithValid_RedirectToAllEvents() 
        
        {
            //Arange
            var input = new EventFormModel
            {
                Name = "Event For Deleting",
                Place = "Klub Burgas",
                Start = new DateTime(2024, 12, 12, 12, 0, 0),
                End = new DateTime(2024, 12, 12, 14, 0, 0)
            };
            var request = new RestRequest("/Event/Add", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("Name", input.Name);
            request.AddParameter("Place", input.Place);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh: mm tt"));
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh: mm tt"));
           
            await _client.ExecuteAsync(request);
            var eventInDb =GetEventByName(input.Name);
            var eventIdToDelete = eventInDb.Id;
            var deleteRequest = new RestRequest($"/Event/Delete/{eventIdToDelete}", Method.Post);
            //Act
            var response = await _client.ExecuteAsync(deleteRequest);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        }

        [Test]
        public async Task Delete_WithNoId_ShouldReturnNotFound()
        {
            //Arange
            var request = new RestRequest("/Event/Delete/", Method.Post);
            //Act
            var response = await _client.ExecuteAsync(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }





        private bool CheckIfEventExist(string name) 
        {
            var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer
                ("Server=.\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;
            var context = new EventmiContext(options);
            return context.Events.Any(x => x.Name == name);
        }

        private Event GetEventByName(string name)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer
                ("Server=.\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;
            var context = new EventmiContext(options);
            return context.Events.FirstOrDefault(x => x.Name == name);
        }


        private Event GetEventById(int id)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>().UseSqlServer
                ("Server=.\\SQLEXPRESS;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true").Options;
            var context = new EventmiContext(options);
            return context.Events.FirstOrDefault(x => x.Id == id);

        }

    }
}