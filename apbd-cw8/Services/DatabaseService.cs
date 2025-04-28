using apbd_cw8.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace apbd_cw8.Services;

public class DatabaseService
{
    private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Trip>> GetAllTripsAsync()
        {
            var trips = new List<Trip>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                                 c.IdCountry, c.Name AS CountryName
                          FROM Trip t
                          LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                          LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
                          ORDER BY t.IdTrip";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            Trip currentTrip = null;
            while (await reader.ReadAsync())
            {
                var tripId = reader.GetInt32(0);
                if (currentTrip == null || currentTrip.IdTrip != tripId)
                {
                    currentTrip = new Trip
                    {
                        IdTrip = tripId,
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        Countries = new List<Country>()
                    };
                    trips.Add(currentTrip);
                }

                if (!reader.IsDBNull(6))
                {
                    currentTrip.Countries.Add(new Country
                    {
                        IdCountry = reader.GetInt32(6),
                        Name = reader.GetString(7)
                    });
                }
            }

            return trips;
        }

        public async Task<List<Trip>> GetTripsForClientAsync(int clientId)
        {
            var trips = new List<Trip>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                                 ct.RegisteredAt, ct.PaymentDate
                          FROM Trip t
                          INNER JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
                          WHERE ct.IdClient = @ClientId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClientId", clientId);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                trips.Add(new Trip
                {
                    IdTrip = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                });
            }

            return trips;
        }

        public async Task<int> CreateClientAsync(Client client)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                          OUTPUT INSERTED.IdClient
                          VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", client.FirstName);
            command.Parameters.AddWithValue("@LastName", client.LastName);
            command.Parameters.AddWithValue("@Email", client.Email);
            command.Parameters.AddWithValue("@Telephone", client.Telephone);
            command.Parameters.AddWithValue("@Pesel", client.Pesel);

            var id = (int)await command.ExecuteScalarAsync();
            return id;
        }

        public async Task<bool> RegisterClientForTripAsync(int clientId, int tripId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var checkCapacityQuery = @"SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId";
            var getMaxPeopleQuery = @"SELECT MaxPeople FROM Trip WHERE IdTrip = @TripId";

            using var checkCommand = new SqlCommand(checkCapacityQuery, connection);
            checkCommand.Parameters.AddWithValue("@TripId", tripId);
            var registeredClients = (int)await checkCommand.ExecuteScalarAsync();

            using var maxPeopleCommand = new SqlCommand(getMaxPeopleQuery, connection);
            maxPeopleCommand.Parameters.AddWithValue("@TripId", tripId);
            var maxPeople = (int)await maxPeopleCommand.ExecuteScalarAsync();

            if (registeredClients >= maxPeople)
            {
                return false; // Brak miejsc
            }

            var insertQuery = @"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                                VALUES (@ClientId, @TripId, @RegisteredAt)";

            using var insertCommand = new SqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@ClientId", clientId);
            insertCommand.Parameters.AddWithValue("@TripId", tripId);
            insertCommand.Parameters.AddWithValue("@RegisteredAt", DateTime.Now);

            await insertCommand.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> UnregisterClientFromTripAsync(int clientId, int tripId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var deleteQuery = @"DELETE FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip = @TripId";

            using var command = new SqlCommand(deleteQuery, connection);
            command.Parameters.AddWithValue("@ClientId", clientId);
            command.Parameters.AddWithValue("@TripId", tripId);

            var affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
}