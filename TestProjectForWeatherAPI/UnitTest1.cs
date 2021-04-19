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
        static List<weather> weathers = new List<weather>();

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
                        JArray a = JArray.Parse(mycontent);
                        for (int i = 0; i < a.Count; i++)
                        {
                            string wea = a[i].ToString();
                            weather e = JsonConvert.DeserializeObject<weather>(wea);
                            weathers.Add(e); //get all the items
                            Console.WriteLine(i + " current item title " + e.title);
                        }
                        Console.WriteLine("Current item's amount =>" + weathers.Count);
                        
                        Assert.IsNotNull(mycontent); //get all the items
                        Assert.AreEqual("Minsk", weathers[4].title); //choose Minsk
                        Assert.AreEqual("53.90255, 27.563101", weathers[4].latt_long); //check Minsk real lon/lat with responded

                        if (response != null)
                            response.Dispose();
                        if (client != null)
                            client.Dispose();
                    }
                }
            }            
        }

        [Test]
        public static async Task Test2() // todays forecust
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(ServiceBaseURL + "834463/"))
                {
                    using (HttpContent content = response.Content)
                    {
                        string mycontent = await content.ReadAsStringAsync();
                        JArray a = JArray.Parse(mycontent);
                        //
                        Assert.IsNotNull(mycontent); //get all the items
                        //

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