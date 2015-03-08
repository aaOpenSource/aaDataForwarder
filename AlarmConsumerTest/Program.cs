using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WNWRAPCONSUMERLib;
using System.Xml;
using Newtonsoft.Json;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;

namespace aaDataForwarder
{
    class Program
    {

        private static WNWRAPCONSUMERLib.wwAlarmConsumer _almConsumer;
        private static System.Net.Sockets.TcpClient _socket;
        private static System.Net.Sockets.NetworkStream _localStream;
        private static System.IO.StreamWriter _localStreamWriter;
        private static MqttClient _mqttClient;
        private static DateTime _lastAlarmTimestamp;
        
        public class localMQTTSettings
        {
            public string host { get; set; }
            public int port { get; set; }
            public string username { get; set; }
            public string password { get; set; }        
        }

        static void Main(string[] args)
        {
            try
            {
      
                SetupAlarmConsumer();
                ConnectToSplunk();
                ConnectToMQTT();

                // Setup a 1 second timer
                Timer t = new Timer(GetCurrentAlarms, null, 0, 1000);

                Console.ReadKey();

                _almConsumer.DeregisterConsumer();
                _almConsumer.UninitializeConsumer();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

       static void ConnectToSplunk()
        {

            try
            {

                Console.WriteLine("Connecting to Splunk");

                // use raw TCP socket to connect to Splunk.  Configure for your selected port
                _socket = new System.Net.Sockets.TcpClient("localhost", 13450);

                _localStream = _socket.GetStream();
                _localStreamWriter = new System.IO.StreamWriter(_localStream);                
            }
           catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           finally
            {
                Console.WriteLine("Connection status to Splunk is is " + _socket.Connected.ToString());
            }
        }

        static void ConnectToMQTT()
       {
           try
           {

               localMQTTSettings settings = JsonConvert.DeserializeObject<localMQTTSettings>(System.IO.File.ReadAllText("mqttsettings.json"));

               Console.WriteLine("Connecting to MQTT");
               _mqttClient = new MqttClient(settings.host, settings.port, false, null);
               _mqttClient.Connect("AlarmTester", settings.username, settings.password);
              
           }
            catch(Exception ex)
           {
               Console.WriteLine(ex.ToString());
           }
            finally
           {
               Console.WriteLine("MQTT connection status is " + _mqttClient.IsConnected.ToString());
           }
       }

        static void SetupAlarmConsumer()
        {
            _almConsumer = new wwAlarmConsumer();

            int result = _almConsumer.InitializeConsumer("AlarmForwarder");
            result = _almConsumer.RegisterConsumer(0, "aaOpenSource.AlarmForwarder", "AlarmForwarder.Console", "0.0.1");
            string xmlquery = @"<QUERIES FROM_PRIORITY=""1"" TO_PRIORITY=""999""  ALARM_STATE=""ALL"" DISPLAY_MODE=""History""><QUERY><NODE>localhost</NODE><PROVIDER>Galaxy</PROVIDER><GROUP>Area_001</GROUP></QUERY></QUERIES>";
            _almConsumer.SetXmlAlarmQuery(xmlquery);

            _lastAlarmTimestamp = new DateTime(0);

        }

        static void GetCurrentAlarms(object o)
        {
            try
            {

                object currentXMLAlarms;
                string jsonString;
                DateTime thisAlarmTimestamp;

                // Get 100 alarms from history
                _almConsumer.GetXmlCurrentAlarms2(100, out currentXMLAlarms);
                System.Xml.XmlNodeList nodes;
                System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
                xdoc.LoadXml(currentXMLAlarms.ToString());
                nodes = xdoc.SelectNodes("/ALARM_RECORDS/ALARM");

                foreach (System.Xml.XmlNode node in nodes)
                {

                    thisAlarmTimestamp = DateTime.Parse(node["DATE"].InnerText + " " + node["TIME"].InnerText);

                    if (thisAlarmTimestamp > _lastAlarmTimestamp)
                    {
                        jsonString = JsonConvert.SerializeXmlNode(node);
                        _localStreamWriter.WriteLine(jsonString);
                        _mqttClient.Publish("data/alarms", System.Text.Encoding.UTF8.GetBytes(jsonString));
                        _lastAlarmTimestamp = thisAlarmTimestamp;
                        Console.WriteLine(JsonConvert.SerializeXmlNode(node));
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                _localStreamWriter.Flush();
            }                
        }
    }
}
