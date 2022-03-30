using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using BCrypt.Net;

namespace SimpleAds30.Data
{
    public class AdsRepository
    {
        private string _connString;

        public AdsRepository(string connString)
        {
            _connString = connString;
        }

        public string GetUserNameById(int id)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Name FROM Users WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            return (string)cmd.ExecuteScalar();
        }

        public List<Ad> GetAds()
        {
            return GetAds(0);
        }

        public List<Ad> GetAds(int id)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Ads ";
            if (id != 0)
            {
                cmd.CommandText += "WHERE UserId = @id ";
                cmd.Parameters.AddWithValue("@id", id);
            }
            cmd.CommandText += "ORDER BY DateAdded DESC";
            conn.Open();

            var reader = cmd.ExecuteReader();
            List<Ad> ads = new();            
            while (reader.Read())
            {
                int userId = (int)reader["UserId"];
                ads.Add(new Ad
                {
                    Id = (int)reader["Id"],
                    UserId = userId,
                    UserName = GetUserNameById(userId),
                    PhoneNumber = (string)reader["PhoneNumber"],
                    DateAdded = (DateTime)reader["DateAdded"],
                    Details = (string)reader["Details"]
                });
            }
            return ads;
        }

        public void AddUser(User user, string password)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Users (Name, EmailAddress, PasswordHash) VALUES (@name, @email, @hash)";
            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@email", user.EmailAddress);
            cmd.Parameters.AddWithValue("@hash", BCrypt.Net.BCrypt.HashPassword(password));
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public User LogIn(string emailAddress, string password)
        {
            User user = GetUserByEmail(emailAddress);
            if (user == null)
            {
                return null;
            }
            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isValid ? user : null;
        }

        public User GetUserByEmail(string email)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Users WHERE EmailAddress = @email";
            cmd.Parameters.AddWithValue("@email", email);
            conn.Open();
            var reader = cmd.ExecuteReader();
            if (reader.Read()) 
            {
                return new User
                {
                    Id = (int)reader["Id"],
                    Name = (string)reader["Name"],
                    EmailAddress = email,
                    PasswordHash = (string)reader["PasswordHash"]
                };
            };
            return null;
        }

        public int AddAd(NewAd newAd)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Ads (PhoneNumber, Details, DateAdded, UserId) VALUES (@number, @details, @date, @id) 
                                SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@number", newAd.PhoneNumber);
            cmd.Parameters.AddWithValue("@details", newAd.Details);
            cmd.Parameters.AddWithValue("@date", DateTime.Now);
            cmd.Parameters.AddWithValue("@id", newAd.UserId);
            conn.Open();
            return (int)(decimal)cmd.ExecuteScalar();
        }

        public void DeleteAd(int id)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM Ads WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }       
        
        public int GetUserIdByEmail(string email)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id FROM Users WHERE EmailAddress = @email";
            cmd.Parameters.AddWithValue("@email", email);
            conn.Open();
            object userId = cmd.ExecuteScalar();
            if (userId == null)
            {
                userId = 0;
            }
            return (int)userId;
        }
    }
}
