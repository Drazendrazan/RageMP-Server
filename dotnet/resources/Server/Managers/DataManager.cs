﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using GTANetworkAPI;
using Newtonsoft.Json;
using Server.Database;

namespace ServerSide
{
    public static class DataManager
    {
        static List<KeyValuePair<int, int>> stationsData = new List<KeyValuePair<int, int>>();

        public static void InstantiatePetrolStationsData()
        {

            System.Timers.Timer checkStations = new System.Timers.Timer(60000);
            checkStations.Elapsed += CheckStations;
            checkStations.Enabled = true;

            stationsData = new List<KeyValuePair<int, int>>();

            using var context = new ServerDB();

            var values = context.Data.ToList()?[0].StationsValues;
            var prices = context.Data.ToList()?[0].StationsPrices;

            List<int> valuesList = JsonConvert.DeserializeObject<List<int>>(values);

            List<int> pricesList = JsonConvert.DeserializeObject<List<int>>(prices);

            for (int i = 0; i < valuesList.Count; i++)
            {
                stationsData.Add(new KeyValuePair<int, int>(valuesList[i], pricesList[i]));
            }
        }

        public static void UpdatePetrolStationValues(string values)
        {
            Dictionary<int, float> data = JsonConvert.DeserializeObject<Dictionary<int, float>>(values);
            foreach(KeyValuePair<int, float> station in data)
            {
                stationsData[station.Key] = new KeyValuePair<int, int>(stationsData[station.Key].Key + Convert.ToInt32(station.Value), stationsData[station.Key].Value);
            }
            SavePetrolStationsToDB();
        }

        public static void SavePetrolStationsToDB()
        {
            List<int> values = new List<int>();
            List<int> prices = new List<int>();
            stationsData.ForEach(station => { values.Add(station.Key); prices.Add(station.Value); });

            using var context = new ServerDB();
            var data = context.Data.ToList()[0];
            data.StationsPrices = JsonConvert.SerializeObject(prices);
            data.StationsValues = JsonConvert.SerializeObject(values);
            context.SaveChanges();
        }

        public static void CheckStations(System.Object source, ElapsedEventArgs e)
        {
            NAPI.Task.Run(() =>
            {
                if(DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0)
                {
                    for(int i = 0; i < stationsData.Count; i++)
                    {
                        KeyValuePair<int, int> station = stationsData[i];
                        KeyValuePair<int, int> newData;
                        if (station.Key < 3000)
                        {
                            newData = new KeyValuePair<int, int>(0, Math.Clamp(station.Value + 1, 2, 8));
                        }
                        else if (station.Key > 5000)
                        {
                            newData = new KeyValuePair<int, int>(0, Math.Clamp(station.Value - 1, 2, 8));
                        }
                        else
                        {
                            newData = new KeyValuePair<int, int>(0, station.Value);
                        }
                        stationsData[stationsData.IndexOf(station)] = newData;
                    }
                    SavePetrolStationsToDB();
                }
            });
        }

        public static int GetPetrolPriceAtIndex(int index)
        {
            return stationsData[index].Value;
        }
    }
}
