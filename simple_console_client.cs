using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using IniParser;
using IniParser.Model;
using IBM.WMQ;
namespace PutMqMessage
{
    class Program
    {
        const String configureFile = "configure.ini";

        [STAThread]
        static void Main(String[] args)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData configure;
            String[] resources;
            String message;

            String queueName;
            String queueHost;
            String queueChannel;
            String queueManagerName;

            int queueAccessFlag = 0;
            MQQueueManager queueManager;
            MQQueue queue;
            MQMessage queueMessage;
            MQPutMessageOptions putMessageOptions = new MQPutMessageOptions();
            MQGetMessageOptions getMessageOptions = new MQGetMessageOptions();

            // Load configure.ini
            try
            {
                configure = parser.ReadFile(configureFile);
                queueName = configure["MQ"]["QNAME"];
                queueChannel = configure["MQ"]["CHANNEL"];
                queueHost = configure["MQ"]["HOST"];
                queueManagerName = configure["MQ"]["MANAGER_NAME"];
                queueAccessFlag |= MQC.MQOO_OUTPUT | MQC.MQOO_INPUT_SHARED | MQC.MQOO_INQUIRE;

            }
            catch (Exception error)
            {
                Console.WriteLine("\nIniLoadError:" + error.Message);
                Console.ReadKey();
                return;
            }


            // Read resource directory
            try
            {
                resources = Directory.GetFiles(configure["PATH"]["RESOURCE_PATH"]);
            }
            catch (Exception error)
            {
                Console.WriteLine("\nReadResourceError:" + error.Message);
                Console.ReadKey();
                return;
            }
            //  Terminate process if no resource files.
            if (resources.Length == 0)
            {
                Console.WriteLine("\nNo resource files found.");
                Console.ReadKey();
                return;
            }

            // get mq_manger instance
            Console.Write("\nConnect to " + queueChannel + "/TCP/" + queueHost + "...");
            try
            {
                queueManager = new MQQueueManager(queueManagerName, queueChannel, queueHost);
                queue = queueManager.AccessQueue(queueName, queueAccessFlag);
                Console.Write(" Success.\n");
                /*
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(queue))
                {
                    try
                    {
                        Console.WriteLine("{0}:\t{1}", property.Name, property.GetValue(queue));
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine("{0}:\t{1}", property.Name, error.Message);
                    }
                }
                */
            }
            catch (MQException error)
            {
                Console.WriteLine("\nMQConnectError:" + error.Message);
                Console.ReadKey();
                return;
            }
            // read & send messages
            /*
            try
            {
                foreach (String resource in resources)
                {
                    message = File.ReadAllText(resource, UTF8Encoding.UTF8);
                    queueMessage = new MQMessage();
                    queueMessage.WriteString(message);
                    queueMessage.Format = MQC.MQFMT_STRING;
                    queue.Put(queueMessage, putMessageOptions);
                    Console.WriteLine("\nPut [{0}]: (length: {1})'{2}'", resource, message.Length, message);
                    // sleep 500ms to continue
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("PutMessageError:" + error.Message);
            }
            */
            // try to get messages
            queueMessage = new MQMessage();
            getMessageOptions.WaitInterval = 15000; // ms
            try
            {

                queue.Get(queueMessage, getMessageOptions);
                if (queueMessage.Format.CompareTo(MQC.MQFMT_STRING) == 0)
                {
                    Console.WriteLine(queueMessage.ReadString(queueMessage.MessageLength));
                }
                else
                {
                    Console.WriteLine("Non-text message receive.");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("GetMessageError:" + error.Message);
            }

            try
            {
                queueManager.Disconnect();
                Console.WriteLine("Disconnect.");
            }
            catch (Exception error)
            {
                Console.WriteLine("MQConnectError:" + error.Message);
            }

            Console.ReadKey();
            return;
        }
    }
}
