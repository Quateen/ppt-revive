using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPTRevive.Application.Common.Interfaces;
public interface ITotpService
{
    string GenerateTotpUrI(string account, string key, int duration);
    string GenerateCode(string account, string key, int duration);
    bool VerifyCode(string uri, string code);
    bool VerifyCode(string account, string key, int duration, string code);
}
