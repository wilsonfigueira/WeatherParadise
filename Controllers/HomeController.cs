using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using WeatherParadise.Models;

namespace WeatherParadise.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult validation(string username, string password)
        {

            //Verificar se o user e a password estão corretos
            if (username == "wc@gmail.com" && password == "123")
            {
                Weather wm = new Weather();
                wm.Period = "Today";
                return View("Main", wm);
            }

            else
            {
                ViewData["auth_error"] = "Username e/ou password inválidos. Tente novamente.";
                return View("Index");
            }

        }

        public IActionResult Results()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> WeatherAPIForm(Weather wm)

        {
            string API_URL;
            switch (wm.Period)
            {
                case "Today":
                    API_URL = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/"
                        + wm.LocationName
                        + "/today?unitGroup=metric&key=5EVJKWNQHX6QBPFJAS89ZRQ4H&contentType=json";
                    break;

                case "Tomorrow":
                    API_URL = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/"
                        + wm.LocationName
                        + "/tomorrow?unitGroup=metric&include=current&key=5EVJKWNQHX6QBPFJAS89ZRQ4H&contentType=json";
                    break;

                case "Next 7 days":
                    API_URL = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/"
                        + wm.LocationName
                        + "/next7days?unitGroup=metric&key=5EVJKWNQHX6QBPFJAS89ZRQ4H&contentType=json";
                    break;

                case "Next 15 days":
                    API_URL = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/"
                        + wm.LocationName
                        + "/?unitGroup=metric&key=5EVJKWNQHX6QBPFJAS89ZRQ4H&contentType=json";
                    break;
                default:
                    API_URL = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/"
                        + wm.LocationName
                        + "/today?unitGroup=metric&key=5EVJKWNQHX6QBPFJAS89ZRQ4H&contentType=json";
                    break;
            }

            //HTTP Request Call Api
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, API_URL);
            var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();//lança uma exeção
            if(!response.IsSuccessStatusCode)
            {
                ViewData["api_error"] = "A localização introduzida não é válida";
                return View("Results", wm);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();//Espera pela resposta - como string

                dynamic weather = JsonConvert.DeserializeObject(body); //Transforma o esparguete do Jason
                List<string> results = new List<string>();
                List<string> stationIds = new List<string>();


                foreach (var day in weather.days)
                {
                    results.Add("");
                    results.Add(" ~ Informações metereológicas para o dia " + day.datetime + " ~");
                    results.Add(" As condições metereológicas gerais serão: " + day.description);
                    results.Add(" A temperatura máxima será de: " + day.tempmax + " º");
                    results.Add(" A temperatura mínima será de: " + day.tempmin + " º");
                    results.Add(" O nascer do sol será às: " + day.sunrise);
                    results.Add(" O pôr-do-sol será às: " + day.sunset);
                    results.Add(" As estações utilizadas para a análise desta estação são: " + day.stations);
                    results.Add("");
                    results.Add(" (Nota: Todas as temperaturas encontram-se em Graus Celsius)");
                    results.Add("");
                }
                ViewBag.WeatherInfo = results;


                results = new List<string>();

                results.Add("Informações relativas às estações associadas a esta localização:");
                

                if (weather.days[0].stations.HasValues) {
                    var weatherStations = weather.days[0].stations;
                    string stationDetails = "";

                    foreach (string stationID in weatherStations)
                    {
                        //Station details are distance, latitude, longitude, useCount, id, name, quality, contribution
                        results.Add("Station ID: " + weather.stations[stationID].id);
                        results.Add("Distance: " + weather.stations[stationID].distance);
                        results.Add("Latitude: " + weather.stations[stationID].latitude);
                        results.Add("Longitude: " + weather.stations[stationID].longitude);
                        results.Add("Use Count: " + weather.stations[stationID].useCount);
                        results.Add("Name: " + weather.stations[stationID].name);
                        results.Add("Quality: " + weather.stations[stationID].quality);
                        results.Add("Contribution: " + weather.stations[stationID].contribution);
                        results.Add("");
                    }
                    ViewBag.StationsInfo = results;
                    return View("Results", wm);
                }else
                {
                    //Console.WriteLine("Não existem estações para esta loca")
                    results.Add("Não existem informações relativas às estações");
                    ViewBag.Output = results;
                    return View("Results", wm);
                }
                
                }
            }
        } 
    }
}