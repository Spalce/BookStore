using AutoFixture;
using BookStore.Core.Interfaces.Services;
using BookStore.Core.Models;
using BookStoreApi.V1.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookStoreTest
{
    public class BooksControllerTest
    {
        private readonly IFixture _fixture;
        private readonly Mock<IBookService> _serMock;
        private readonly BooksController _sutController;
        
        public BooksControllerTest()
        {
            _fixture = new Fixture();
            _serMock = _fixture.Freeze<Mock<IBookService>>();
            _sutController = new BooksController(_serMock.Object, null);
        }

        [Fact]
        public async Task Get_ShouldReturnOkResponse_WhenDataFound()
        {
            // Arrange
            var itemMock = _fixture.Create<IEnumerable<Book>>();
            _serMock.Setup(e => e.GetRecordsAsync()).ReturnsAsync(itemMock);

            // Act
            var result = await _sutController.Get().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ActionResult<IEnumerable<Book>>>();
            result.Result.Should().BeAssignableTo<OkObjectResult>();
            result.Result.As<OkObjectResult>().Value
                .Should()
                .NotBeNull()
                .And.BeOfType(itemMock.GetType());
            _serMock.Verify(e => e.GetRecordsAsync(), Times.Once());
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenDataNotFound()
        {
            // Arrange
            List<Book> response = null;
            _serMock.Setup(e => e.GetRecordsAsync()).ReturnsAsync(response);

            // Act
            var result = await _sutController.Get().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<NoContentResult>();
            _serMock.Verify(e => e.GetRecordsAsync(), Times.Once());
        }

        [Fact]
        public async Task GetById_ShouldReturnOkResponse_WhenValInput()
        {
            // Arrange
            var itemMock = _fixture.Create<Book>();
            var id = _fixture.Create<int>();
            _serMock.Setup(e => e.GetByIdAsync(id)).ReturnsAsync(itemMock);

            // Act
            var result = await _sutController.GetById(id).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ActionResult<Book>>();
            result.Result.Should().BeAssignableTo<OkObjectResult>();
            result.Result.As<OkObjectResult>().Value
                .Should()
                .NotBeNull()
                .And.BeOfType(itemMock.GetType());
            _serMock.Verify(e => e.GetByIdAsync(id), Times.Once());
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenDataNotFound()
        {
            // Arrange
            Book response = null;
            var id = _fixture.Create<int>();
            _serMock.Setup(e => e.GetByIdAsync(id)).ReturnsAsync(response);

            // Act
            var result = await _sutController.GetById(id).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<NotFoundResult>();
            _serMock.Verify(e => e.GetByIdAsync(id), Times.Once());
        }

        [Fact]
        public async Task GetById_ShouldReturnBadRequest_WhenInputIsZero()
        {
            // Arrange
            Book response = _fixture.Create<Book>();
            var id = 0;
            _serMock.Setup(e => e.GetByIdAsync(id)).ReturnsAsync(response);

            // Act
            var result = await _sutController.GetById(id).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<BadRequestResult>();
            _serMock.Verify(e => e.GetByIdAsync(id), Times.Once());
        }

        [Fact]
        public async Task Post_ShouldReturnOkResponse_WhenValidRequest()
        {
            // Arrange
            var request = _fixture.Create<Book>();
            var response = _fixture.Create<Book>();
            _serMock.Setup(e => e.AddAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _sutController.Post(request).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<ActionResult<Book>>();
            result.Result.Should().BeAssignableTo<CreatedAtRouteResult>();
            _serMock.Verify(e => e.AddAsync(response), Times.Never);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenInvalidRequest()
        {
            // Arrange
            var request = _fixture.Create<Book>();
            _sutController.ModelState.AddModelError("Subject", "The subject field is required!");
            var response = _fixture.Create<Book>();
            var id = _fixture.Create<int>();
            _serMock.Setup(e => e.AddAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _sutController.Post(request).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<BadRequestResult>();
            _serMock.Verify(e => e.AddAsync(request), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenDeletedRecord()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _serMock.Setup(e => e.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _sutController.Delete(id).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenRecordNotFound()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _serMock.Setup(e => e.DeleteAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _sutController.Delete(id).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnBadRequest_WhenInputIsZero()
        {
            // Arrange
            var id = 0;
            _serMock.Setup(e => e.DeleteAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _sutController.Delete(id).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<BadRequestResult>();
            _serMock.Verify(e => e.DeleteAsync(id), Times.Never);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenInputIsZero()
        {
            // Arrange
            var id = 0;
            var request = _fixture.Create<Book>();
            _serMock.Setup(e => e.UpdateAsync(request)).ReturnsAsync(false);

            // Act
            var result = await _sutController.Put(id, request).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<BadRequestResult>();
            _serMock.Verify(e => e.UpdateAsync(request), Times.Never);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenInvalidRequest()
        {
            // Arrange
            var request = _fixture.Create<Book>();
            _sutController.ModelState.AddModelError("Subject", "The subject field is required!");
            var id = _fixture.Create<int>();
            _serMock.Setup(e => e.UpdateAsync(request)).ReturnsAsync(false);

            // Act
            var result = await _sutController.Put(id, request).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<BadRequestResult>();
            _serMock.Verify(e => e.UpdateAsync(request), Times.Never);
        }

        [Fact]
        public async Task Put_ShouldReturnOkResponse_WhenRecordIsUpdated()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var request = _fixture.Create<Book>();
            _serMock.Setup(e => e.UpdateAsync(request)).ReturnsAsync(true);

            // Act
            var result = await _sutController.Put(id, request).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<OkObjectResult>();
            _serMock.Verify(e => e.UpdateAsync(request), Times.Never);
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenRecordNotFound()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var request = _fixture.Create<Book>();
            _serMock.Setup(e => e.UpdateAsync(request)).ReturnsAsync(false);

            // Act
            var result = await _sutController.Put(id, request).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeAssignableTo<NotFoundResult>();
            _serMock.Verify(e => e.UpdateAsync(request), Times.Never);
        }
    }
}