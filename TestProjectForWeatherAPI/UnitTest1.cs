using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TestProjectForWeatherAPI
{
    public class Tests
    {
        #region 

        //ServiceBaseURL + "search/?query=min"; //"latt_long":"53.90255,27.563101" 
        // ServiceBaseURL + "834463/"; //18-23.04 "the_temp":15.0
        //ServiceBaseURL + "834463/2017/4/20/"; // -5 years- M starts from 2016/11/16/ "weather_state_name":"Light Cloud"

        private const string ServiceBaseURL = "https://www.metaweather.com/api/location/";
        private const string expectedCity = "Minsk";
        private const string expectedLattLong = "53.90255,27.563101";
        private const double temp_min = 2.0;
        private const double temp_max = 20.0;
        static string woeid = "";

        static List<Location> locations = new List<Location>();
        static List<todayForecust> todayForecusts = new List<todayForecust>();

        #endregion

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public static async Task Test1() //get all the items for '/min' and choose Minsk, check Minsk real lon/lat with responded
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
                            if (e.title == "Minsk")
                            {
                                index = i; //choose Minsk
                                woeid = e.woeid.ToString() + "/";
                            }
                            Console.WriteLine(i + " current item title " + e.title);
                        }
                        Console.WriteLine("Current item's amount =>" + locations.Count);
                        
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
        public static async Task Test2() // todays forecust, tempetature is between 2..20 for all days in response
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(ServiceBaseURL + woeid))
                {
                    using (HttpContent content = response.Content)
                    {
                        string mycontent = await content.ReadAsStringAsync();
                        todayForecust tdf = JsonConvert.DeserializeObject<todayForecust>(mycontent);
                        bool isNormalTemp = true;
                        foreach (var e in tdf.consolidated_weather)
                        {
                            Console.WriteLine("the_temp " + e.the_temp);
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
        
       
    }
}