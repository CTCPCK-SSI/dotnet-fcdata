﻿using Newtonsoft.Json;
using SSI.FastConnect.DataContracts.Realtime;
using SSI.FastConnect.RealTimeClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSI.FastConnect.Client.Realtime.Sample
{
    class Event
    {
        public long count { get; set; }
        public string lastContent { get; set; }
    }
    class Program
    {
        public static ConcurrentDictionary<int, Event> clients = new ConcurrentDictionary<int, Event>();
        static void Main(string[] args)
        {
            var allRows = File.ReadAllLines(@"D:\fcdata_connection.csv");
            int i = 1;
            List<Task<MarketDataStreamingClientV2>> a = new List<Task<MarketDataStreamingClientV2>>();
            foreach (var row in allRows)
            {
                var columns = row.Split(';');

                a.Add(InitMarketStreaming("http://192.168.213.98:1188/v2.0/",
               columns[0],
                 columns[1],
                columns[2],
                i));
                i++; Thread.Sleep(500);

            }

            foreach (var row in allRows)
            {
                var columns = row.Split(';');

                a.Add(InitMarketStreaming("http://192.168.213.98:1188/v2.0/",
               columns[0],
                 columns[1],
                columns[2],
                i));
                i++; Thread.Sleep(500);
            }
            foreach (var row in allRows)
            {
                var columns = row.Split(';');

                a.Add(InitMarketStreaming("http://192.168.213.98:1188/v2.0/",
               columns[0],
                 columns[1],
                columns[2],
                i));
                i++; Thread.Sleep(500);
            }
            while (true)
            {
                foreach (var c in clients)
                {
                    Console.WriteLine("{0,4}     {1,5}", c.Key, c.Value.count);
                }
                Thread.Sleep(10000);
            }
            Console.Read();
        }

        private static async Task<MarketDataStreamingClientV2> InitMarketStreaming(string fcURL, string fcId, string fcSecret, string channelSub, int i)
        {
            try
            {
                var client = new MarketDataStreamingClientV2(fcURL, new Client.AuthenProvider("http://192.168.213.98:1188", fcId, fcSecret));
                client.CreateHandleCallBack(data =>
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<MdhfData>(data);
                        if (clients.TryGetValue(i, out var c))
                        {
                            c.count++;
                            c.lastContent = result.Content;
                        }
                        else
                        {
                            c = new Event
                            {
                                lastContent = result.Content,
                                count = 1
                            };
                            clients.TryAdd(i, c);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(data);
                    }
                });
                await client.Start();
                await client.SwitchChannels(channelSub);

                return client;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        private static void OnReceive(string data)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<MdhfData>(data);
                //Console.WriteLine(result.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(data);
            }
        }
    }
}
