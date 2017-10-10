using System;

namespace WebApplicationTest.Utils
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}