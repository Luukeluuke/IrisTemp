using Iris.Database;
using System;
using System.Threading.Tasks;

namespace Iris.Structures
{
    internal class Loaner
    {
        #region Properties and Variables
        /// <summary>
        /// ID of the loaner.
        /// </summary>
        public int ID { get; private set; }
        /// <summary>
        /// Name of the loaner.
        /// </summary>
        public string Name { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Used for database.
        /// </summary>
        /// <param name="id">The ID from the database.</param>
        /// <param name="name">The name of the loaner.</param>
        public Loaner(int id, string name)
        {
            ID = id;
            Name = name;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initial create of a loaner. Creates it in the database.
        /// </summary>
        /// <param name="name">Name of the loaner.</param>
        /// <returns>The created loaner.</returns>
        public static async Task<Loaner> CreateNewLoaner(string name)
        {
            return await DatabaseHandler.InsertLoaner(name);
        }

        public override string ToString()
        {
            return $"ID: '{ID}', Name: '{Name}'";
        }
        #endregion
    }
}
