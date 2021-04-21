using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TestProjectForWeatherAPI
{
    public class Tests
    {
        #region Variables             

        private const string ServiceBaseURL = "https://www.metaweather.com/api/location/";
        private const string expectedCity = "Minsk";
        private const string expectedLattLong = "53.90255,27.563101";
        private const double temp_min = 2.0;
        private const double temp_max = 20.0;
        static string woeid = "";        
        static string stateName = "";

        static List<Location> locations = new List<Location>();
        static List<consolidated_weather> cw = new List<consolidated_weather>();

        #endregion

        [Test]
        public static async Task Test1ChooseMinsk() //get all the items for '/min' and choose Minsk, check Minsk real lon/lat with responded
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(ServiceBaseURL + "search/?query=min"))
                {
                    using (HttpContent content = response.Content)
                    {
                        string mycontent = await content.ReadAsStringAsync();
                        int index = 0;
                        var a = JArray.Parse(mycontent);
                        for (int i = 0; i < a.Count; i++)
                        {
                            string loc = a[i].ToString();
                            Location e = JsonConvert.DeserializeObject<Location>(loc);
                            locations.Add(e); //get all the items
                            if (e.title == expectedCity)
                            {
                                index = i; //choose Minsk
                                woeid = e.woeid.ToString() + "/";
                            }
                            Console.WriteLine(i + " current item title " + e.title);
                        }                        

                        Assert.IsNotNull(mycontent); //check to get all the items
                        Assert.AreEqual(expectedCity, locations[index].title); //check to choose Minsk
                        Assert.AreEqual(expectedLattLong, locations[index].latt_long); //check Minsk real lat/lon 53.9 / 27.6 with responded

                        if (response != null)
                            response.Dispose();
                        if (client != null)
                            client.Dispose();
                    }
                }
            }
        }

        [Test]
        public static async Task Test2TodaysForecust() // todays forecust, tempetature is between 2..20 for all days in response
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(ServiceBaseURL + woeid))
                {
                    using (HttpContent content = response.Content)
                    {
                        string mycontent = await content.ReadAsStringAsync();
                        todayForecust tdf = JsonConvert.DeserializeObject<todayForecust>(mycontent);
                        stateName = tdf.consolidated_weather[0].weather_state_name;
                        bool isNormalTemp = true;
                        foreach (var e in tdf.consolidated_weather)
                        {
                            Console.WriteLine("the_stateName " + e.weather_state_name);
                            
                            double the_temp = Convert.ToDouble(e.the_temp);
                            if (the_temp < temp_min || the_temp > temp_max)
                            {
                                isNormalTemp = false;
                            }
                        }

                        Assert.IsNotNull(mycontent); //check to get all the items                        
                        Assert.IsTrue(isNormalTemp); //tempetature is between min..max for all days in response

                        if (response != null)
                            response.Dispose();
                        if (client != null)
                            client.Dispose();
                    }
                }
            }
        }

        [Test]
        public static async Task Test3FourYearsLaterForecust() // 4 years later forecust, state is the same as todays
        {
            using (HttpClient client = new HttpClient())
            {
                var laterDate = DateTime.Now.AddYears(-4).ToString("yyyy/M/d/"); // if to chose 5 years, there would be no data available, only after 2016/11/16/
                string[] laterDate_split = new string[3];
                laterDate_split[0] = laterDate.Substring(0, 4);
                laterDate_split[1] = laterDate.Substring(5, 1);
                laterDate_split[2] = laterDate.Substring(7, 2);
                laterDate = string.Join("/", laterDate_split) + "/";
                Console.WriteLine(laterDate);
                using (HttpResponseMessage response = await client.GetAsync(ServiceBaseURL + woeid + laterDate)) 
                {
                    using (HttpContent content = response.Content)
                    {
                        bool isLaterStateTheSame = false;                  
                        
                        string mycontent = await content.ReadAsStringAsync();                        
                        var a = JArray.Parse(mycontent);
                        for (int i = 0; i < a.Count; i++)
                        {
                            string conw = a[i].ToString();
                            consolidated_weather e = JsonConvert.DeserializeObject<consolidated_weather>(conw);
                            cw.Add(e); //get all the items
                            if (e.weather_state_name == stateName)
                            {
                                isLaterStateTheSame = true;
                            }
                            Console.WriteLine(e.weather_state_name);
                        }

                        Assert.IsTrue(isLaterStateTheSame);

                        if (response != null)
                            response.Dispose();
                        if (client != null)
                            client.Dispose();
                    }
                }
            }
        }
    }
}