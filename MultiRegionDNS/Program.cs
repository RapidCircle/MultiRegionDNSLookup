using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.IO;

/*# Multi-Region DNS NameServer Propogation Check
#
# Uses the dns-lg.com API to retrieve the NS records for a zone (domain name)
# at 19 (more or less) different locations globally. Use this to monitor the 
# propogation of nameserver changes at your registrar. 
# 
# Note: There is no such thing as a guarantee when it comes to whether your
# new nameservers have propogated fully or not. Rule of thumb is three days
# but often it is much faster and this check can help you weigh the risks.

Inspired by the work of leighmcculloch in Ruby available at
https://gist.github.com/leighmcculloch/48485350c76b2566e410
*/
namespace MultiRegionDNS
{
    class Program
    {
        static void Main(string[] args)
        {
            string domainName = string.Empty;
            string jsonCountryNodes;
            WebClient client = new WebClient();
            Console.WriteLine("Enter domain name: ");
            domainName = Console.ReadLine();
            Stream nodeData = client.OpenRead("http://www.dns-lg.com/nodes.json");
            using (StreamReader reader = new StreamReader(nodeData))
            {
                jsonCountryNodes = reader.ReadToEnd();
            }
            var NodeList = JsonConvert.DeserializeObject<IDictionary<string, List<Node>>>(jsonCountryNodes);

            foreach (var node in NodeList.Values.First().ToList())
            {
                try
                {
                    int nodeCounter = 0;
                    string jsonNodeStatus;
                    Console.WriteLine("Node # {0}", ++nodeCounter);
                    Console.WriteLine("Name# {0}", node.name);
                    Console.WriteLine("Country: {0}", node.country);
                    Console.WriteLine("ASN: {0}", node.asn);
                    Console.WriteLine("Operator:{0}", node.@operator);
                    Console.WriteLine("\nRecords -->");
                    string url = string.Format("http://www.dns-lg.com/{0}/{1}/ns", node.name, domainName);
                    Stream dnsData = client.OpenRead(url);
                    using (StreamReader reader = new StreamReader(dnsData))
                    {
                        jsonNodeStatus = reader.ReadToEnd();
                        var dyn = JsonConvert.DeserializeObject<IDictionary<string, dynamic>>(jsonNodeStatus);
                        string questionString = dyn.Values.First().ToString();
                        string answerString = dyn.Values.Last().ToString();
                        var questions = JsonConvert.DeserializeObject<List<NsQustion>>(questionString);
                        var answers = JsonConvert.DeserializeObject<List<NsAnswer>>(answerString);
                        int answerCounter = 0;
                        foreach (NsAnswer nsA in answers)
                        {
                            Console.WriteLine("Record # {0}", ++answerCounter);
                            Console.WriteLine("Name: {0}", nsA.name);
                            Console.WriteLine("Type: {0}", nsA.type);
                            Console.WriteLine("Class: {0}", nsA.@class);
                            Console.WriteLine("TTL: {0}", nsA.ttl);
                            Console.WriteLine("RDLength: {0}", nsA.rdlength);
                            Console.WriteLine("RData: {0}", nsA.rdata);
                            Console.WriteLine("-------\n");
                        }
                        Console.WriteLine("-------------------------------------------------\n");
                        Console.Read();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching status from node");
                }
            }
        }
    }
    class Node
    {
        public string name;
        public string isocc;
        public string country;
        public string asn;
        public string @operator;
    }
    public class NsQustion
    {
        public string name;
        public string type;
        public string @class;
    }
    public class NsAnswer
    {
        public string name;
        public string type;
        public string @class;
        public string ttl;
        public string rdlength;
        public string rdata;
    }
}