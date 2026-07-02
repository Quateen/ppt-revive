using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace PPTRevive.Domain.Common;
public static class TypeExtensions
{
    public static string GetDescription(this Enum genericEnum) //Hint: Change the method signature and input paramter to use the type parameter T
    {
        Type genericEnumType = genericEnum.GetType();
        MemberInfo[] memberInfo = genericEnumType.GetMember(genericEnum.ToString());

        if ((memberInfo != null && memberInfo.Length > 0))
        {
            var attributes = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if ((attributes != null && attributes.Count() > 0))
            {
                return ((System.ComponentModel.DescriptionAttribute)attributes.ElementAt(0)).Description;
            }
        }

        return genericEnum.ToString();
    }

    public static string GenerateRandomPassword()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);

        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        StringBuilder result = new StringBuilder((bytes.Length + 4) / 5 * 8);

        int byteIndex = 0, bitIndex = 0;
        int currentByte = 0, nextByte = 0;

        while (byteIndex < bytes.Length)
        {
            if (bitIndex > 3)
            {
                nextByte = bytes[byteIndex++];
                currentByte |= nextByte >> 8 - bitIndex & 0x1F;
                bitIndex -= 5;
            }
            else
            {
                currentByte = bytes[byteIndex] >> 3 - bitIndex & 0x1F;
                bitIndex += 5;
            }

            result.Append(base32Chars[currentByte]);
        }

        return result.ToString();
    }

    public static string ConvertUtcToTimeZone(DateTime utcTime, string timeZoneId)
    {
        try
        {
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

            if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId == "string")
            {
                timeZoneId = "Asia/Karachi"; // Default timezone
            }

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);

            return localTime.ToString("dd/MM/yyyy hh:mm tt");
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException($"Invalid timezone ID: {timeZoneId}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error converting time: {ex.Message}");
        }
    }

    public static string AddSpaces(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var result = new StringBuilder();
        result.Append(input[0]);

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && !char.IsWhiteSpace(input[i - 1]))
            {
                result.Append(' ');
            }
            result.Append(input[i]);
        }

        return result.ToString();
    }

    public static string ToReadableString(this Enum value)
    {
        return System.Text.RegularExpressions.Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");
    }


    //public static ContentType GetContentTypeEnum(string contentType)
    //{
    //    switch (contentType.ToLower())
    //    {

    //        case "image/png":
    //            return ContentType.PNG;

    //        case "image/jpeg":
    //            return ContentType.JPG;

    //        case "image/gif":
    //            return ContentType.GIF;

    //        case "video/mp4":
    //            return ContentType.MP4;

    //        case "video/x-msvideo":
    //            return ContentType.AVI;

    //        case "video/x-matroska":
    //            return ContentType.MKV;

    //        case "video/quicktime":
    //            return ContentType.MOV;

    //        case "video/x-ms-wmv":
    //            return ContentType.WMV;
    //        // Add other cases as necessary
    //        default:
    //            throw new ArgumentException("Unsupported content type");
    //    }
    //}
}
