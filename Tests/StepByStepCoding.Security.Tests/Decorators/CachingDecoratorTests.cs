using StepByStepCoding.Application.Contracts.Caching;
using StepByStepCoding.Application.Decorators;
using StepByStepCoding.Application.Features.Courses.Commands.CreateCourse;
using StepByStepCoding.Application.Features.Files.Commands.UpdateLectureFile;
using StepByStepCoding.Application.Models.Caching;
using StepByStepCoding.Application.Services;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Utilities.Caching;

namespace StepByStepCoding.Security.Tests.Decorators
{
    public sealed class CachingDecoratorTests : TestBase
    {
        public CachingDecoratorTests(ITestOutputHelper output) : base(output) { }

        #region Cached reads

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_ReturnsCachedValueOrStoresHandlerResult(bool hit)
        {
            // Arrange
            var key = Fixture.Create<string>();
            var expected = Fixture.Create<string>();
            var request = new Mock<CacheableRequest<string>>("courses");
            request.Setup(query => query.Key()).Returns(key);
            var inner = new Mock<IRequestHandler<CacheableRequest<string>, string>>();
            var redis = new Mock<IRedisService>();
            redis.Setup(service => service.Get<CachedResponse<string>>(key, CancellationToken.None))
                .ReturnsAsync(hit ? new CachedResponse<string>(expected) : null);
            inner.Setup(handler => handler.Handle(request.Object, CancellationToken.None)).ReturnsAsync(expected);
            var decorator = new CachingDecorator<CacheableRequest<string>, string>(inner.Object,
                new Lazy<IRedisService>(() => redis.Object),
                new Lazy<AcademicCachePolicy>(() => throw new InvalidOperationException("Non-academic requests do not need this policy.")));

            WriteOutput("Arranged input", (nameof(key), key), (nameof(expected), expected), (nameof(hit), hit));

            // Act
            var result = await decorator.Handle(request.Object, CancellationToken.None);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().Be(expected);
            inner.Verify(handler => handler.Handle(request.Object, CancellationToken.None), hit ? Times.Never() : Times.Once());
            redis.Verify(service => service.Set(key, It.Is<CachedResponse<string>>(value => value.Value == expected),
                null, null, CancellationToken.None), hit ? Times.Never() : Times.Once());
        }

        #endregion

        #region Invalidation after writes

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_InvalidatesPrefixOnlyAfterSuccessfulWrite(bool fails)
        {
            // Arrange
            var request = Fixture.Create<CreateCourseCommand>();
            var expected = Fixture.Create<Guid>();
            var failure = new InvalidOperationException(Fixture.Create<string>());
            var inner = new Mock<IRequestHandler<CreateCourseCommand, Guid>>();
            var redis = new Mock<IRedisService>();
            if (fails)
                inner.Setup(handler => handler.Handle(request, CancellationToken.None)).ThrowsAsync(failure);
            else
                inner.Setup(handler => handler.Handle(request, CancellationToken.None)).ReturnsAsync(expected);

            var decorator = new CachingDecorator<CreateCourseCommand, Guid>(inner.Object,
                new Lazy<IRedisService>(() => redis.Object),
                new Lazy<AcademicCachePolicy>(() => throw new InvalidOperationException("Writes do not need a read policy.")));
            Guid? result = null;

            WriteOutput("Arranged input", (nameof(request), request), (nameof(fails), fails));

            // Act
            var exception = await Record.ExceptionAsync(async () => result = await decorator.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), (nameof(exception), exception));

            // Assert
            if (fails)
                exception.Should().BeSameAs(failure);
            else
            {
                exception.Should().BeNull();
                result.Should().Be(expected);
            }

            redis.Verify(service => service.RemoveByPrefix(request.Prefix + ":", CancellationToken.None), fails ? Times.Never() : Times.Once());
        }

        [Fact]
        public async Task Handle_VoidUpdate_InvalidatesSharedPrefixAndFileKeyAfterHandler()
        {
            // Arrange
            var request = Fixture.Create<UpdateLectureFileCommand>();
            var inner = new Mock<IRequestHandler<UpdateLectureFileCommand>>(MockBehavior.Strict);
            var redis = new Mock<IRedisService>(MockBehavior.Strict);
            var sequence = new MockSequence();
            inner.InSequence(sequence).Setup(handler => handler.Handle(request, CancellationToken.None)).Returns(Task.CompletedTask);
            redis.InSequence(sequence).Setup(service => service.RemoveByPrefix(request.Prefix + ":", CancellationToken.None)).Returns(Task.CompletedTask);
            redis.InSequence(sequence).Setup(service => service.RemoveByPrefix(request.Key(), CancellationToken.None)).Returns(Task.CompletedTask);
            var decorator = new CachingDecorator<UpdateLectureFileCommand>(inner.Object, new Lazy<IRedisService>(() => redis.Object));

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await Record.ExceptionAsync(() => decorator.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeNull();
            inner.VerifyAll();
            redis.VerifyAll();
        }

        #endregion
    }
}
