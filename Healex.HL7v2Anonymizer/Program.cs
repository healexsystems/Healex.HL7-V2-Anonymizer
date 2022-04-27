using CommandLine;
using Healex.HL7v2Anonymizer.Options;
using Healex.HL7v2Anonymizer.Services;
using HL7.Dotnetcore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Healex.HL7v2Anonymizer {

    class Program {

        static void Main(string[] args) {
            try {
                var input = args.Select(s => s.Trim());
                Parser.Default
                     .ParseArguments<Process>(input)
                     .MapResult((Process options) =>  ProcessCommandHandler(options),
                                 error => -1);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error while processing commad.\nMessage:{ex.Message} \nStack{ex.StackTrace}");
            }
        }

        private static int ProcessCommandHandler(Process options) {
            TryAnonymizeMessages(options.Directory);
            return 1;
        }

        private static void TryAnonymizeMessages(string directory) {
            var files = Directory.GetFiles(directory, "*.hl7");
            if (files.Length == 0) {
                Console.WriteLine($"=> No v2 messages found in {directory}");
                return;
            }
            var anonymizer = new Anonymizer(GetReplacementOptions());
            foreach (var path in files) {
                var message = ReadAndParseMessage(path);
                if (message is not null) {
                    anonymizer.Anonymize(message);
                    SerializeAndWriteMessageOrLogError(message, path);
                }
            }
        }

        private static void SerializeAndWriteMessageOrLogError(Message message, string path) {
            Console.WriteLine($"=> Anonymization successful: {path}");
            var serializedMessage = message.SerializeMessage(true);
            File.WriteAllText(path, serializedMessage);
        }

        private static Message ReadAndParseMessage(string path) {
            var message = new Message(File.ReadAllText(path));
            message.ParseMessage();
            return message;
        }

        private static ReplacementOptions GetReplacementOptions() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);
            var config = builder.Build();
            return config.GetSection("ReplacementOptions").Get<ReplacementOptions>();
        }
    }
}
