using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using LibAtem.Commands;
using LibAtem.Commands.MixEffects;
using LibAtem.Commands.SuperSource;
using LibAtem.Common;
using LibAtem.Net;
using Newtonsoft.Json;

namespace AtemKeyboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var log = LogManager.GetLogger(typeof(Program));
            log.Info("Starting");

            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            var client = new AtemClient(config.AtemAddress);
            
            ConsoleKeyInfo key;
            Console.WriteLine("Press escape to exit");
            while ((key = Console.ReadKey()).Key != ConsoleKey.Escape)
            {
                if (config.MixEffect != null)
                {
                    foreach (KeyValuePair<MixEffectBlockId, Config.MixEffectConfig> me in config.MixEffect)
                    {
                        if (me.Value.Program != null && me.Value.Program.TryGetValue(key.KeyChar, out VideoSource src))
                            client.SendCommand(new ProgramInputSetCommand {Index = me.Key, Source = src});

                        if (me.Value.Preview != null && me.Value.Preview.TryGetValue(key.KeyChar, out src))
                            client.SendCommand(new PreviewInputSetCommand {Index = me.Key, Source = src});

                        if (me.Value.Cut == key.KeyChar)
                            client.SendCommand(new MixEffectCutCommand {Index = me.Key});
                        if (me.Value.Auto == key.KeyChar)
                            client.SendCommand(new MixEffectAutoCommand {Index = me.Key});
                    }
                }

                if (config.Auxiliary != null)
                {
                    foreach (KeyValuePair<AuxiliaryId, Dictionary<char, VideoSource>> aux in config.Auxiliary)
                    {
                        if (aux.Value == null)
                            continue;

                        if (aux.Value.TryGetValue(key.KeyChar, out VideoSource src))
                            client.SendCommand(new AuxSourceSetCommand {Id = aux.Key, Source = src});
                    }
                }

                if (config.SuperSource != null)
                {
                    foreach (KeyValuePair<SuperSourceBoxId, Dictionary<char, VideoSource>> box in config.SuperSource)
                    {
                        if (box.Value == null)
                            continue;

                        if (box.Value.TryGetValue(key.KeyChar, out VideoSource src))
                            client.SendCommand(new SuperSourceBoxSetCommand {Mask = SuperSourceBoxSetCommand.MaskFlags.Source, Index = box.Key, InputSource = src});
                    }
                }


            }
        }
    }
}
