using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace WordChainGame
{
    static class LocalDbQuery
    {
        static public List<City> Select(Dictionary<string, string> dict) // Выбор городов из БД с заданными параметрами
        {
            string argValues = String.Empty;
            foreach (KeyValuePair<string, string> pair in dict)
            {
                argValues += String.Format("({0} = {1}) AND", pair.Key, pair.Value);
            }
            argValues = argValues.Substring(0, argValues.Length - 4);
            string queryString = String.Format("SELECT name, region, country, id, disable, first FROM City WHERE ({0});", argValues); // Строка запроса
            List<City> results = new List<City>();
            using (SqlConnection connection = new SqlConnection(global::WordChainGame.Properties.Settings.Default.LocalDBConnectionString)) // Установка соединения с БД
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        results.Add(new City(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString(), reader[5].ToString()));
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return results;
        }
        static public void DisableCity(string id) // Пометить город неиспользованным
        {
            string queryString = String.Format("UPDATE City SET disable = 1 WHERE id = '{0}';", id);
            SqlConnection connection = new SqlConnection(global::WordChainGame.Properties.Settings.Default.LocalDBConnectionString);
            SqlCommand command = new SqlCommand(queryString, connection);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
        static public void DisableAll() // Пометить все города неиспользованными
        {
            string queryString = "UPDATE City SET disable = 0;";
            SqlConnection connection = new SqlConnection(global::WordChainGame.Properties.Settings.Default.LocalDBConnectionString);
            SqlCommand command = new SqlCommand(queryString, connection);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}