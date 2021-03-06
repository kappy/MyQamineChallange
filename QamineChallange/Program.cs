﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;

namespace QamineChallange
{
    class Program
    {
        private static readonly Uri Url = new Uri("http://powerful-fortress-5090.appspot.com");
        private const string Challenge = "challenge";
        private const string Answer = "answer";


        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var challangeString = MakeRequest(new Uri(Url, Challenge));
            Console.WriteLine("got challange on {0} mark", stopwatch.Elapsed);
            
            var challange = ParseChallengeData(challangeString);
            Console.WriteLine("parsed challenge on {0} mark", stopwatch.Elapsed);
            
            var postData = string.Format("payload={0}&contact={1}&id={2}", challange.Result.ToString(),
                             Uri.EscapeDataString("kappy@acydburne.com.pt"), challange.Id);
            Console.WriteLine(postData);
            Console.WriteLine("calculates challange on {0} mark", stopwatch.Elapsed);

            var response = MakeRequest(new Uri(Url, Answer),
                               data: postData,
                               method: "POST",
                               contentType: "application/x-www-form-urlencoded");
            Console.WriteLine("submitted challenge on {0} mark", stopwatch.Elapsed);

            Console.WriteLine(response);
            Console.ReadLine();
        }

        private static ChallengeData ParseChallengeData(string challengeString)
        {
            var data = new ChallengeData();
            const string rx = @"^If you could just (\bsubtract|add|multiply|divide\b) (\d+) (\bby|to|times\b) (\d+) that would be great..Your Id is also ([0-9a-fA-F]+)";

            Console.WriteLine(challengeString);
            var match = Regex.Match(challengeString, rx);
            if (match.Success)
            {
                data.Operator = match.Groups[1].Value;
                data.Value1 = Convert.ToInt32(match.Groups[2].Value);
                data.Value2 = Convert.ToInt32(match.Groups[4].Value);
                data.Id = match.Groups[5].Value;
            }
            else
            {
                throw new Exception("Invalid matches count");
            }
            return data;
        }

        private static string MakeRequest(Uri uri, string method = "GET", string data = null, string contentType = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            if (!string.IsNullOrEmpty(contentType))
            {
                request.ContentType = contentType;                
            }
            if (!string.IsNullOrEmpty(data))
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                request.ContentLength = bytes.Length;
                var requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);   
            }

            var response = (HttpWebResponse)request.GetResponse();
            using (var responseStream = response.GetResponseStream())
            {
                using (var streamReader = new StreamReader(responseStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }

    public class ChallengeData
    {
        public string Id { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public string Operator { get; set; }

        public int Result
        {
            get
            {
                return Operators[Operator](Value1, Value2);
            }
        }

        private static readonly IDictionary<string, Func<int, int, int>> Operators = new Dictionary<string, Func<int, int, int>>
        {
            {"add", (i1, i2) => i1 + i2},
            {"subtract", (i1, i2) => i2 - i1},
            {"multiply", (i1, i2) => i1 * i2},
            {"divide", (i1, i2) => i1 / i2}
        };
    }
}
