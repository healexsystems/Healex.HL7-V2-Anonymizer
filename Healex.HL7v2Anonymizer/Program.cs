using Healex.HL7v2Anonymizer.Services;
using HL7.Dotnetcore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Healex.HL7v2Anonymizer
{
    class Program
    {
        static void Main(string[] args)
        {
            WaitForInput();
        }

        private static void WaitForInput()
        {
            Console.WriteLine("Welcome to the Healex HL7v2 anonymizer!");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            Console.WriteLine("WARNING: THIS APPLICATION OVERWRITES THE ORIGINAL v2 MESSAGES. BACKUP YOUR FILES FIRST!");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            Console.WriteLine("Enter the directory to your v2 messages and press enter:");
            var directory = Console.ReadLine();
            Console.WriteLine();
            TryAnonymizeMessages(directory);
            Console.WriteLine();
            WaitForInput();
        }

        private static void TryAnonymizeMessages(string directory)
        {
            var pathsToV2Messages = GetPathsToV2Messages(directory);
            var anonymizer = new Anonymizer(GetReplacementOptions());

            if (pathsToV2Messages is not null)
            {
                if (pathsToV2Messages.Length == 0)
                    Console.WriteLine($"=> No v2 messages found in {directory}");

                foreach (string path in pathsToV2Messages)
                {
                    var message = ReadAndParseMessage(path);
                    if (message is not null)
                    {
                        var success = anonymizer.Anonymize(message);
                        SerializeAndWriteMessageOrLogError(success, message, path);
                    }
                }
            }
        }

        private static void SerializeAndWriteMessageOrLogError(bool success, Message message, string path)
        {
            if (success)
            {
                Console.WriteLine($"=> Anonymization successful: {path}");
                var serializedMessage = message.SerializeMessage(true);
                File.WriteAllText(path, serializedMessage);
            }
            else
            {
                Console.WriteLine($"=> Anonymization fail: {path}");
            }
        }

        private static string[] GetPathsToV2Messages(string directory)
        {
            try
            {
                var v2Messages = Directory.GetFiles(directory, "*.hl7");
                return v2Messages;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private static Message ReadAndParseMessage(string path)
        {
            try
            {
                var message = new Message(File.ReadAllText(path));
                message.ParseMessage();
                return message;
            } catch (Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }

        private static ReplacementOptions GetReplacementOptions()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            var replacementOptions = config.GetSection("ReplacementOptions").Get<ReplacementOptions>();
            return replacementOptions;
        }
    }
}
