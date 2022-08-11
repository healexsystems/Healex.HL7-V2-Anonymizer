using CommandLine;
using Healex.HL7v2Anonymizer.Options;
using Healex.HL7v2Anonymizer.Services;
using HL7.Dotnetcore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Healex.HL7v2Anonymizer {

    static class Program {
        static void Main(string[] args) {
            try {
                var input = args.Select(s => s.Trim());
                Parser.Default
                     .ParseArguments<Process>(input)
                     .MapResult((Process options) =>  ProcessCommandHandler(options),
                                 error => -1);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error while processing command.\nMessage:{ex.Message} \nStack{ex.StackTrace}");
            }
        }

        private static int ProcessCommandHandler(Process options) {
            if (string.IsNullOrEmpty(options.OutputDirectory)) {
                // if no output parameter is given, set the output to the same directory as the input directory
                options.OutputDirectory = options.Directory;
            }
            if (!Directory.Exists(options.Directory)) {
                Console.WriteLine($"=> The directory '{options.Directory}' does not exist.");
                return -1;
            }
            if (!Directory.Exists(options.OutputDirectory)) {
                Console.WriteLine($"=> The directory '{options.OutputDirectory}' does not exist.");
                return -1;
            }
            TryAnonymizeMessages(options.Directory, options.OutputDirectory, options.Suffix);
            return 1;
        }

        private static void TryAnonymizeMessages(string directory, string outputDirectory, string suffix) {
            var files = Directory.GetFiles(directory, "*.hl7");
            if (files.Length == 0) {
                Console.WriteLine($"=> No v2 messages found in {directory}");
                return;
            }
            var anonymizer = new Anonymizer(GetReplacementOptions());
            foreach (var path in files)
            {
                var fileName = Path.GetFileName(path);

                if (!string.IsNullOrEmpty(suffix))
                {
                    fileName = fileName.Replace(".hl7", $"{suffix}.hl7");
                }
                var outputPath = Path.Join(outputDirectory, fileName);
                var message = ReadAndParseMessage(path);
                if (message is not null) {
                    anonymizer.Anonymize(message);
                    SerializeAndWriteMessageOrLogError(message, path, outputPath);
                }
            }
        }

        private static void SerializeAndWriteMessageOrLogError(Message message, string originalPath, string outputPath) {
            var serializedMessage = message.SerializeMessage(true);
            File.WriteAllText(outputPath, serializedMessage);
            Console.WriteLine($"{originalPath} anonymized and moved to {outputPath}");
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
