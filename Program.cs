using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

class Program
{
   static async Task Main(string[] args)
{
    bool keepRunning = true;

    while (keepRunning)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=======================================");
        Console.WriteLine("       🌤️  Sarah's Simple Weather App 🌤️");
        Console.WriteLine("=======================================\n");
        Console.ResetColor();

        Console.WriteLine("Would you like to search by City or ZIP code? (C/Z):");
        string searchType = Console.ReadLine().Trim().ToUpper();

        string query = "";
        if (searchType == "C")
        {
            Console.WriteLine("Enter city name:");
            query = $"q={Console.ReadLine()}";
        }
        else if (searchType == "Z")
        {
            Console.WriteLine("Enter ZIP code (US only, e.g. 10001):");
            query = $"zip={Console.ReadLine()},us";
        }
        else
        {
            Console.WriteLine("Invalid choice. Defaulting to city search.");
            Console.WriteLine("Enter city name:");
            query = $"q={Console.ReadLine()}";
        }

        Console.WriteLine("Would you like Current Weather or 5-Day Forecast? (C/F):");
        string mode = Console.ReadLine().Trim().ToUpper();

        Console.WriteLine("Do you want the temperature in Celsius, Fahrenheit, or Both? (C/F/B):");
        string choice = Console.ReadLine().Trim().ToUpper();

        string apiKey = "b776678d59382c63f91c239497034aed"; // Replace with your key

        if (mode == "C")
        {
            await GetCurrentWeather(query, apiKey, choice);
        }
        else if (mode == "F")
        {
            await GetForecast(query, apiKey, choice);
        }
        else
        {
            Console.WriteLine("Invalid option. Defaulting to current weather.");
            await GetCurrentWeather(query, apiKey, choice);
        }

        // 🔁 Ask if the user wants to search again
        Console.WriteLine("\nWould you like to search again? (Y/N):");
        string again = Console.ReadLine().Trim().ToUpper();
        if (again != "Y")
        {
            keepRunning = false;
            Console.WriteLine("\nThanks for using the Weather App! 🌍 Goodbye!");
        }

        Console.Clear(); // optional: clears the screen for a fresh start
    }
}

    static async Task GetCurrentWeather(string query, string apiKey, string choice)
    {
        string url = $"https://api.openweathermap.org/data/2.5/weather?{query}&appid={apiKey}&units=metric";

        using HttpClient client = new HttpClient();

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            string location = root.GetProperty("name").GetString();
            string weather = root.GetProperty("weather")[0].GetProperty("description").GetString();
            double tempC = root.GetProperty("main").GetProperty("temp").GetDouble();
            double feelsLikeC = root.GetProperty("main").GetProperty("feels_like").GetDouble();
            int humidity = root.GetProperty("main").GetProperty("humidity").GetInt32();

            double tempF = (tempC * 9 / 5) + 32;
            double feelsLikeF = (feelsLikeC * 9 / 5) + 32;

            string weatherIcon = GetWeatherIcon(weather);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n=======================================");
            Console.WriteLine($"   Current Weather for {location}");
            Console.WriteLine("=======================================\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{weatherIcon} Condition: {weather}");
            Console.ResetColor();

            PrintTemps(choice, tempC, feelsLikeC, tempF, feelsLikeF);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Humidity: {humidity}%");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error fetching weather data: " + ex.Message);
            Console.ResetColor();
        }
    }

    static async Task GetForecast(string query, string apiKey, string choice)
    {
        string url = $"https://api.openweathermap.org/data/2.5/forecast?{query}&appid={apiKey}&units=metric";

        using HttpClient client = new HttpClient();

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            string location = root.GetProperty("city").GetProperty("name").GetString();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n=======================================");
            Console.WriteLine($"   5-Day Forecast for {location}");
            Console.WriteLine("=======================================\n");
            Console.ResetColor();

            // Group forecasts by date
            var forecasts = root.GetProperty("list").EnumerateArray()
                .GroupBy(f => DateTime.Parse(f.GetProperty("dt_txt").GetString()).Date);

            foreach (var day in forecasts)
            {
                string date = day.Key.ToString("dddd, MMM dd");

                // Avg temps
                double avgTempC = day.Average(f => f.GetProperty("main").GetProperty("temp").GetDouble());
                double avgFeelsC = day.Average(f => f.GetProperty("main").GetProperty("feels_like").GetDouble());
                string weather = day.First().GetProperty("weather")[0].GetProperty("description").GetString();

                double avgTempF = (avgTempC * 9 / 5) + 32;
                double avgFeelsF = (avgFeelsC * 9 / 5) + 32;

                string icon = GetWeatherIcon(weather);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{date}");
                Console.ResetColor();

                Console.WriteLine($"{icon} Condition: {weather}");
                PrintTemps(choice, avgTempC, avgFeelsC, avgTempF, avgFeelsF);
                Console.WriteLine("---------------------------------------");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error fetching forecast data: " + ex.Message);
            Console.ResetColor();
        }
    }

    static void PrintTemps(string choice, double tempC, double feelsC, double tempF, double feelsF)
    {
        if (choice == "C")
        {
            Console.WriteLine($"Temperature: {tempC:F1} °C");
            Console.WriteLine($"Feels like: {feelsC:F1} °C");
        }
        else if (choice == "F")
        {
            Console.WriteLine($"Temperature: {tempF:F1} °F");
            Console.WriteLine($"Feels like: {feelsF:F1} °F");
        }
        else
        {
            Console.WriteLine($"Temperature: {tempC:F1} °C / {tempF:F1} °F");
            Console.WriteLine($"Feels like: {feelsC:F1} °C / {feelsF:F1} °F");
        }
    }

    static string GetWeatherIcon(string description)
    {
        description = description.ToLower();
        if (description.Contains("clear")) return "☀️";
        if (description.Contains("cloud")) return "☁️";
        if (description.Contains("rain")) return "🌧️";
        if (description.Contains("thunder")) return "⛈️";
        if (description.Contains("snow")) return "❄️";
        if (description.Contains("mist") || description.Contains("fog")) return "🌫️";
        if (description.Contains("drizzle")) return "🌦️";
        return "🌍";
    }
}
