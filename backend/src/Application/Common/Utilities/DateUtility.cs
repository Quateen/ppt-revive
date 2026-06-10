using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPTRevive.Application.Common.Utilities;

public class DateUtility
{
    public static DateTime GetFirstDateOfNextMonth(DateTime inputDate)
    {
        int nextMonth = inputDate.Month == 12 ? 1 : inputDate.Month + 1;
        int year = inputDate.Month == 12 ? inputDate.Year + 1 : inputDate.Year;

        return new DateTime(year, nextMonth, 1);
    }

    public static string CalculateAgeFromDOB(DateTime dateOfBirth)
    {
        DateTime today = DateTime.Today;

        if (dateOfBirth > today)
        {
            throw new ArgumentException("Date of birth cannot be in the future.");
        }

        int years = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-years)) years--;

        if (years > 1)
        {
            return $"{years} years";
        }
        else
        {
            int months = ((today.Year - dateOfBirth.Year) * 12) + today.Month - dateOfBirth.Month;
            if (dateOfBirth.Day > today.Day)
            {
                months--;
            }

            if (months >= 1)
            {
                return $"{months} months";
            }
            else
            {
                int days = (today - dateOfBirth).Days;
                return $"{days} days";
            }
        }
    }

    public static string CalculateHoursAndMinutes(DateTime? checkInTime, DateTime? checkOutTime)
    {
        // Default values in case of incorrect times
        DateTime defaultCheckInTime = new DateTime(2024, 1, 1, 9, 0, 0); // 9:00 AM on 1st Jan 2024
        DateTime defaultCheckOutTime = new DateTime(2024, 1, 1, 9, 0, 0); // 9:00 PM on 1st Jan 2024

        // Validate check-in and check-out times
        DateTime validCheckInTime = checkInTime ?? defaultCheckInTime;
        DateTime validCheckOutTime = checkOutTime ?? defaultCheckOutTime;

        // Ensure that check-out time is after check-in time
        if (validCheckOutTime <= validCheckInTime)
        {
            // If not, use default times
            validCheckInTime = defaultCheckInTime;
            validCheckOutTime = defaultCheckOutTime;
        }

        // Calculate the difference in hours and minutes
        TimeSpan timeDifference = validCheckOutTime - validCheckInTime;
        int hours = (int)timeDifference.TotalHours;
        int minutes = timeDifference.Minutes;

        // Format the output as "Hours:Minutes"
        return $"{hours:D2}:{minutes:D2}";
    }
}
