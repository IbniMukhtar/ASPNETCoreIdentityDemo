using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
namespace ASPNETCoreIdentityDemo.Services
{
        public interface ISMSSender
        {
            Task<bool> SendSmsAsync(string to, string message);
        }
        public class SMSSender : ISMSSender
        {
            private readonly IConfiguration _configuration;
            private readonly string? AccountSID;
            private readonly string? AuthToken;
            private readonly string? FromNumber;
            public SMSSender(IConfiguration configuration)
            {
                _configuration = configuration;
                AccountSID = _configuration["SMSSettings:AccountSID"];
                AuthToken = _configuration["SMSSettings:AuthToken"];
                FromNumber = _configuration["SMSSettings:FromNumber"];
            }
            public Task<bool> SendSmsAsync(string to, string message)
            {
                try
                {
                    //Initialize base client with AccountSID and AuthToken
                    TwilioClient.Init(AccountSID, AuthToken);
                    //Construct a new CreateMessageOptions
                    var messageOptions = new CreateMessageOptions(new PhoneNumber(to))
                    {
                        From = new PhoneNumber(FromNumber),
                        Body = message
                    };
                    //Send a message
                    var msg = MessageResource.Create(messageOptions);
                    //Return true if no error
                    return Task.FromResult(true);
                }
                catch (Exception)
                {
                    //Log the Error Message and Return false
                    return Task.FromResult(false);
                }
            }
        }
}
