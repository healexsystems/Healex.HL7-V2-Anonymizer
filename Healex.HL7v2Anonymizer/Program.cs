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
            waitForInput();
        }

        private static void waitForInput()
        {
            Console.WriteLine("Welcome to the Healex HL7v2 anonymizer!");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            Console.WriteLine("WARNING: THIS APPLICATION OVERWRITES THE ORIGINAL v2 MESSAGES. BACKUP YOUR FILES FIRST!");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            Console.WriteLine("Enter the directory to your v2 messages and press enter:");
            var directory = Console.ReadLine();
            Console.WriteLine();
            tryAnonymizeMessages(directory);
            Console.WriteLine();
            waitForInput();
        }

        private static void tryAnonymizeMessages(string directory)
        {
            var pathsToV2Messages = getPathsToV2Messages(directory);
            var anonymizer = new Anonymizer(getReplacementOptions());

            if (pathsToV2Messages is not null)
            {
                if (pathsToV2Messages.Length == 0)
                    Console.WriteLine($"=> No v2 messages found in {directory}");

                foreach (string path in pathsToV2Messages)
                {
                    var message = readAndParseMessage(path);
                    var success = anonymizer.Anonymize(message);
                    serializeAndWriteMessageOrLogError(success, message, path);
                }
            }
        }

        private static void serializeAndWriteMessageOrLogError(bool success, Message message, string path)
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

        private static string[] getPathsToV2Messages(string directory)
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

        private static Message readAndParseMessage(string path)
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

        private static ReplacementOptions getReplacementOptions()
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
