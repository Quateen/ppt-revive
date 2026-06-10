using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.Extensions.Configuration;
using Petrsnd.OtpCore;
using PPTRevive.Application.Common.Interfaces;

namespace PPTRevive.Infrastructure.Services;
public class TotpService(IConfiguration configuration) : ITotpService
{
    public string GenerateTotpUrI(string account, string key, int expiryInSeconds)
    {
        var issuer = configuration["Otp:Issuer"];
        var codeLength = Convert.ToInt32(configuration["Otp:CodeLength"]);
        var codeValiditySeconds = expiryInSeconds;

        var decodedKey = Encoding.ASCII.GetBytes(key);


        var otpAuthUri = new OtpAuthUri(
                            OtpType.Totp,
                            decodedKey,
                            account,
                            issuer,
                            algorithm: OtpHmacAlgorithm.HmacSha1,
                            digits: codeLength,
                            counterOrPeriod: codeValiditySeconds
                        );
        return otpAuthUri.ToString();
    }

    public bool VerifyCode(string uri, string code)
    {
        var authenticator = Totp.GetAuthenticator(uri);
        return authenticator.GetCode() == code;
    }

    public string GenerateCode(string account, string key, int duration)
    {
        var uri = GenerateTotpUrI(account, key, duration);
        var authenticator = Totp.GetAuthenticator(uri);
        return authenticator.GetCode();
    }

    public bool VerifyCode(string account, string key, int duration, string code)
    {
        return GenerateCode(account, key, duration) == code;
    }
}
