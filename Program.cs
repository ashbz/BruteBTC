using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BruteBTC
{
    class Program
    {
        private static string LOGO_ASCII = "";
        private static string DATA_FILE_PATH = @"data.tsv";
        private static string FOUND_FILE_PATH = @"found.txt";

        private static ConcurrentDictionary<string,bool> DICT_WALLETS_WITH_BALANCE = new ConcurrentDictionary<string, bool>();
        private static ReaderWriterLockSlim _file_lock = new ReaderWriterLockSlim();

        static void Main(string[] args)
        {
            LOGO_ASCII =
@"====================================================================
██████╗ ██████╗ ██╗   ██╗████████╗███████╗██████╗ ████████╗ ██████╗
██╔══██╗██╔══██╗██║   ██║╚══██╔══╝██╔════╝██╔══██╗╚══██╔══╝██╔════╝
██████╔╝██████╔╝██║   ██║   ██║   █████╗  ██████╔╝   ██║   ██║     
██╔══██╗██╔══██╗██║   ██║   ██║   ██╔══╝  ██╔══██╗   ██║   ██║     
██████╔╝██║  ██║╚██████╔╝   ██║   ███████╗██████╔╝   ██║   ╚██████╗
╚═════╝ ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚══════╝╚═════╝    ╚═╝    ╚═════╝
====================================================================                                                    

";
            if (args.Length > 0)
            {
                DATA_FILE_PATH = args[0];
            }

            if (!File.Exists(DATA_FILE_PATH))
            {
                Console.WriteLine("File not found: " + DATA_FILE_PATH);
                return;
            }

            ReadWalletDataIntoMemory();
            DoBruteChecking();
            Console.ReadLine();
        }

        static void DoBruteChecking()
        {
            // max checks, before we rest to show a message to the user
            var max_to_check = 10000;
            // total tries
            var total_tries = 0;
            // count of wallets we find. Just in case :)
            var wallets_found_count = 0;


            // stopwatch to check how many checks we can do every second
            Stopwatch sw = new Stopwatch();

            // we just don't stop unless user closes the program
            while(true)
            {
                sw.Restart();
                
                // run a loop but with multi-threading
                // more cores = more checks
                Parallel.For(0, max_to_check, (i) =>
                {
                    total_tries++;

                    // generate a random private key
                    Key private_key = new Key();

                    // get its public key
                    PubKey publicKey = private_key.PubKey;

                    // get it's public address, but the legacy version, as most wallets are (for example: 1Jj1rSnVAjgRR8wLiLQ2BoHjahSndspjMS)
                    var public_address = publicKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);

                    // we use TryGetValue as it's supposedly faster than ContainsKey
                    // ContainsKey = accesses dictionary 2 times
                    // TryGetValue = accesses dictionary 1 time
                    if (DICT_WALLETS_WITH_BALANCE.TryGetValue(public_address.ToString(), out _))
                    {
                        wallets_found_count++;

                        dynamic wallet = new ExpandoObject();
                        // base64 encoded bytes
                        wallet.private_key64 = Convert.ToBase64String(private_key.ToBytes()); 
                        wallet.public_key = private_key.PubKey.Hash.ToString();
                        wallet.public_address = private_key.PubKey.Hash.GetAddress(Network.Main).ToString();

                        // let's save all sorts of info
                        var s = "[ " + Environment.NewLine +  JsonConvert.SerializeObject(wallet) + Environment.NewLine;
                        s += " ]" + Environment.NewLine;

                        // let's use a lock in case someone has opened the file
                        _file_lock.EnterWriteLock();
                        try
                        {
                            File.AppendAllText(FOUND_FILE_PATH, s);
                        }
                        finally
                        {
                            _file_lock.ExitWriteLock();
                        }
                    }
                });

                var total_checks_per_second = max_to_check / sw.Elapsed.TotalSeconds;


                Console.Clear();
                Console.WriteLine(LOGO_ASCII);
                Console.WriteLine(total_tries + " - Found: " + wallets_found_count.ToString());
                Console.WriteLine("");
                Console.WriteLine("Checks: " + Math.Truncate(total_checks_per_second) + "/s");

            }
        }

        static void ReadWalletDataIntoMemory()
        {
            // total lines/wallets in file
            var total_count = 0;

            // buffer counter to show console message
            var temp_buffer_count = 0;
            // temp buffer size
            var max_buffer_count = 100000;

            // each row is tab separated
            char tab_delimiter = Convert.ToChar(9);

            // the file is huge, so we have to read lines to not load everything into memory
            foreach (string line in File.ReadLines(DATA_FILE_PATH))
            {
                total_count++;
                temp_buffer_count++;

                string[] row_data = line.Split(tab_delimiter);
                
                if (row_data.Length == 0) continue;

                DICT_WALLETS_WITH_BALANCE[row_data[0]] = false;
                
                // let's show a message to the user that we are still loading
                if (temp_buffer_count > max_buffer_count)
                {
                    temp_buffer_count = 0;
                    Console.Clear();
                    Console.WriteLine(LOGO_ASCII);
                    Console.WriteLine("Loaded wallets with balance in memory: " + total_count);
                }
            }
        }
    }

}
