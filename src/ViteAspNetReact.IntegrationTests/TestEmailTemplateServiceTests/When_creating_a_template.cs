using Shouldly;
using ViteAspNetReact.Core;
using ViteAspNetReact.IntegrationTests.Helpers;
using ViteAspNetReact.IntegrationTests.TestData;
using Xunit.Abstractions;

namespace ViteAspNetReact.IntegrationTests.TestEmailTemplateServiceTests;

public class When_rendering_confirm_email_template : IAsyncLifetime
{
  private readonly ITestOutputHelper _testOutputHelper;
  private TestEmailTemplateService? _testEmailTemplateService;
  private string? _email;
  private Guid _code;

  public When_rendering_confirm_email_template(
    ITestOutputHelper testOutputHelper
  ) => _testOutputHelper = testOutputHelper;

  public async Task InitializeAsync()
  {
    _testEmailTemplateService = new TestEmailTemplateService();
    _code = Guid.NewGuid();
    var template = new EmailTemplate.ConfirmEmail(Nive.MarkTurner.EMail, _code.ToString());
    _email = await _testEmailTemplateService.RenderTemplate(template);
  }

  [Fact]
  public void should_return_guid_as_message_body() => _email.ShouldBe(_code.ToString());


  public async Task DisposeAsync() => await Task.CompletedTask;
}
