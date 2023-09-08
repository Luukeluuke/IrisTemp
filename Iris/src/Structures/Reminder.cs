using Iris.Database;
using Iris.Structures;
using System;
using System.Threading.Tasks;

namespace Iris.src.Structures
{
    internal class Reminder
    {
        public int ID { get; private set; }
        public int BorrowingID { get; private set; }
        public DateTime Timestamp { get; private set; }

        private Reminder(int id, int borrowingId, DateTime timestamp)
        {
            ID = id;
            BorrowingID = borrowingId;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Creates a new Reminder in the database. 
        /// </summary>
        /// <returns>The new created reminder object.</returns>
        public static async Task<Reminder?> CreateNewReminder(Borrowing borrowing, DateTime timestamp)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
            int offsetStartDate = timeZoneInfo.IsDaylightSavingTime(timestamp) ? 2 : 1;
            long unixTimestamp = new DateTimeOffset(timestamp.AddHours(offsetStartDate)).ToUnixTimeSeconds();

            return await DatabaseHandler.InsertReminder(borrowing, unixTimestamp);
        }

        public static Reminder CreateReminder(int id, int borrowingId, long unixTimestamp)
        {
            return new Reminder(id, borrowingId, DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime);
        }
    }
}
