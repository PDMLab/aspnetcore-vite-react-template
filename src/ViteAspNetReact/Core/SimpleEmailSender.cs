using Amazon;
using Amazon.Internal;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ViteAspNetReact.Core;

public static class SimpleEmailSenderRegistration
{
  public static IServiceCollection UseSimpleEmailSender(
    this IServiceCollection services
  )
  {
    var sender = new SimpleEmailSender();
    return services.AddSingleton<IEmailSender>(sender);
  }

  public static IServiceCollection UseSimpleEmailSender(
    this IServiceCollection services,
    IConfigurationSection configurationSection
  )
  {
    var sender = new SimpleEmailSender(configurationSection);
    return services.AddSingleton<IEmailSender>(sender);
  }
}

public class SimpleEmailSenderConfiguration
{
  private const string DefaultCredentialLocation = "~/.aws/credentials";
  private const string DefaultProfileName = "default";
  public string AwsCredentialsLocation { get; set; } = DefaultCredentialLocation;
  public string AwsProfileName { get; set; } = DefaultProfileName;
}

public class SimpleEmailSender : IEmailSender
{
  public static readonly string SimpleEmailSenderConfigSectionKey = "SimpleEmailSender";
  private readonly AWSCredentials _credentials;

  public SimpleEmailSender()
  {
    var chain = new CredentialProfileStoreChain();
    if (chain.TryGetAWSCredentials("default", out var credentials))
    {
      _credentials = credentials;
    }
    else
    {
      throw new ArgumentException($"Couldn't parse AWS credentials for {nameof(SimpleEmailSender)}");
    }
  }

  public SimpleEmailSender(
    IConfigurationSection configurationSection
  )
  {
    var configuration = configurationSection.Get<SimpleEmailSenderConfiguration>() ??
                        throw new ArgumentNullException(
                          $"Could not read {nameof(SimpleEmailSenderConfiguration)} using key {SimpleEmailSenderConfigSectionKey}"
                        );
    var chain = new CredentialProfileStoreChain(configuration.AwsCredentialsLocation);
    if (chain.TryGetAWSCredentials(configuration.AwsProfileName, out var credentials))
    {
      _credentials = credentials;
    }
    else
    {
      throw new ArgumentException($"Couldn't parse AWS credentials for {nameof(SimpleEmailSender)}");
    }
  }

  public async Task SendEmailAsync(
    string email,
    string subject,
    string htmlMessage
  )
  {
    var sendRequest = new SendEmailRequest()
    {
      Source = Constants.ReplyEmail,
      Destination = new Destination(
        new()
        {
          email
        }
      ),
      Message = new Message(new Content(subject), new Body(new Content(htmlMessage)))
    };

    using var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.EUCentral1);
    await client.SendEmailAsync(sendRequest);
  }
}
