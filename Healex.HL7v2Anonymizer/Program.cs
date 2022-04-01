using Healex.HL7v2Anonymizer.Services;
using HL7.Dotnetcore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CommandLine;
using Healex.HL7v2Anonymizer.Options;
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
            var pathsToV2Messages = GetPathsToV2Messages(directory);
            var anonymizer = new Anonymizer(GetReplacementOptions());
            if (pathsToV2Messages is not null) {
                if (pathsToV2Messages.Length == 0) {
                    Console.WriteLine($"=> No v2 messages found in {directory}");
                }
                foreach (string path in pathsToV2Messages) {
                    var message = ReadAndParseMessage(path);
                    if (message is not null) {
                        var success = anonymizer.Anonymize(message);
                        SerializeAndWriteMessageOrLogError(success, message, path);
                    }
                }
            }
        }

        private static void SerializeAndWriteMessageOrLogError(bool success, Message message, string path) {
            if (success) {
                Console.WriteLine($"=> Anonymization successful: {path}");
                var serializedMessage = message.SerializeMessage(true);
                File.WriteAllText(path, serializedMessage);
            }
            else {
                Console.WriteLine($"=> Anonymization fail: {path}");
            }
        }

        private static string[] GetPathsToV2Messages(string directory) {
            try {
                var v2Messages = Directory.GetFiles(directory, "*.hl7");
                return v2Messages;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private static Message ReadAndParseMessage(string path) {
            try {
                var message = new Message(File.ReadAllText(path));
                message.ParseMessage();
                return message;
            }
            catch (Exception e) {
                Console.Write(e.Message);
                return null;
            }
        }

        private static ReplacementOptions GetReplacementOptions() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            var replacementOptions = config.GetSection("ReplacementOptions").Get<ReplacementOptions>();
            return replacementOptions;
        }
    }
}
