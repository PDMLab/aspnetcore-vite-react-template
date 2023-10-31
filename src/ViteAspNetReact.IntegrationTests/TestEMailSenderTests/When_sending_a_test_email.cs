using Shouldly;
using ViteAspNetReact.IntegrationTests.Helpers;
using ViteAspNetReact.IntegrationTests.TestData;
using Xunit.Abstractions;

namespace ViteAspNetReact.IntegrationTests.TestEMailSenderTests;

public class When_sending_a_test_email : IAsyncLifetime
{
  private readonly ITestOutputHelper _testOutputHelper;
  private TestEmailSender? _emailService;
  private string? _messageId;

  public When_sending_a_test_email(
    ITestOutputHelper testOutputHelper
  ) => _testOutputHelper = testOutputHelper;

  public async Task InitializeAsync()
  {
    _emailService = new TestEmailSender();
    _messageId = Guid.NewGuid().ToString();
    var subject = Guid.NewGuid()
      .ToString();
    await _emailService.SendEmailAsync(
      Nive.MarkTurner.EMail,
      subject,
      _messageId
    );
  }

  [Fact]
  public void should_add_key_to_body() => _emailService?.SentEmails.ShouldContain(e => e.Message == _messageId);


  public async Task DisposeAsync() => await Task.CompletedTask;
}
